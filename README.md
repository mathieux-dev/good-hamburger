# Good Hamburger — Sistema de Pedidos

[![CI](https://github.com/mathieux-dev/good-hamburger/actions/workflows/ci.yml/badge.svg)](https://github.com/mathieux-dev/good-hamburger/actions/workflows/ci.yml)

Sistema completo de gerenciamento de pedidos para lanchonete: API REST com regras de negócio para combos e descontos automáticos, e frontend Blazor Server para operação do caixa e gestão do cardápio.

---

## Tecnologias

**Backend**
- .NET 8 / ASP.NET Core Web API
- Entity Framework Core + Npgsql
- PostgreSQL
- xUnit + FluentAssertions + Testcontainers

**Frontend**
- Blazor Server (.NET 8)
- SignalR (circuit em tempo real)

**Infraestrutura**
- Docker / Docker Compose
- GitHub Actions (CI/CD)
- Render.com (hospedagem)

---

## Como rodar

```bash
docker-compose up --build
```

| Serviço   | URL                         |
|-----------|-----------------------------|
| Frontend  | http://localhost:5001        |
| API       | http://localhost:5000        |
| Scalar UI | http://localhost:5000/scalar |

---

## Testes

```bash
# Todos os testes
dotnet test

# Apenas unitários (sem Docker)
dotnet test --filter "FullyQualifiedName~Unit"
```

Os testes de integração sobem um container PostgreSQL real via Testcontainers — Docker em execução é necessário.

Cobertura mínima exigida: **90% de linhas** (verificada automaticamente no CI).

---

## CI/CD

O pipeline roda no GitHub Actions a cada push para `main` ou abertura de PR:

1. **build-and-test** — restore, build Release, testes com cobertura, gate de 90%
2. **deploy** — chama os deploy hooks do Render (somente em push para `main`, após CI verde)

Artifacts de cobertura são publicados como artefatos do workflow.

---

## Frontend

| Rota           | Descrição                                          |
|----------------|----------------------------------------------------|
| `/`            | Página inicial                                     |
| `/order`       | Montar novo pedido (menu + carrinho interativo)    |
| `/order/{id}`  | Editar pedido (restrito a status "preparando")     |
| `/ticket/{id}` | Comprovante imprimível                             |
| `/admin`       | Gestão de pedidos — filtros por status, tipo e busca |
| `/products`    | CRUD do cardápio com modal flutuante               |
| `/dashboard`   | KPIs e estatísticas por período                    |

---

## Cardápio

Itens iniciais (semeados via migration). Novos produtos podem ser adicionados pelo admin.

| Produto       | Categoria | Preço   |
|---------------|-----------|---------|
| X Burger      | Sandwich  | R$ 5,00 |
| X Egg         | Sandwich  | R$ 4,50 |
| X Bacon       | Sandwich  | R$ 7,00 |
| Batata Frita  | Side      | R$ 2,00 |
| Refrigerante  | Drink     | R$ 2,50 |

Produtos possuem `Subtitle`, `Description`, `ImageUrl` e `IsActive`.

---

## Regras de Negócio

Um pedido pode conter no máximo **um item por categoria** (`Sandwich`, `Side`, `Drink`).

### Descontos aplicados automaticamente

| Combinação                        | Desconto |
|-----------------------------------|----------|
| Sanduíche + Batata + Refrigerante | 20%      |
| Sanduíche + Refrigerante          | 15%      |
| Sanduíche + Batata                | 10%      |

### Tipo de atendimento

Cada pedido identifica o canal: **Salão**, **Retirada** ou **Delivery**.

### Edição e exclusão

- Edição de pedidos restrita a status `"preparando"`.
- Exclusão via **soft delete** (`IsDeleted` / `DeletedAt`), com filtro global no EF Core.

---

## API — Endpoints

### Pedidos (`/api/orders`)

| Método | Rota                    | Descrição           | Sucesso |
|--------|-------------------------|---------------------|---------|
| POST   | /api/orders             | Criar pedido        | 201     |
| GET    | /api/orders             | Listar pedidos      | 200     |
| GET    | /api/orders/{id}        | Buscar por ID       | 200     |
| PUT    | /api/orders/{id}        | Atualizar pedido    | 200     |
| PATCH  | /api/orders/{id}/status | Atualizar status    | 204     |
| DELETE | /api/orders/{id}        | Remover (soft)      | 204     |

### Cardápio (`/api/menu` e `/api/products`)

| Método | Rota                | Descrição           | Sucesso |
|--------|---------------------|---------------------|---------|
| GET    | /api/menu           | Listar itens ativos | 200     |
| GET    | /api/products       | Listar todos        | 200     |
| POST   | /api/products       | Criar produto       | 201     |
| PUT    | /api/products/{id}  | Atualizar produto   | 200     |
| DELETE | /api/products/{id}  | Remover produto     | 204     |

### Infra

| Método | Rota      | Descrição         | Sucesso |
|--------|-----------|-------------------|---------|
| GET    | /health   | Health check      | 200     |

---

## Arquitetura

Clean Architecture com DDD Lite em cinco camadas:

```
src/
 ├── Api             — Controllers, entrada HTTP, DI wiring
 ├── Application     — Casos de uso (CQRS leve: commands/queries + handlers)
 ├── Domain          — Entidades, regras de negócio, interfaces
 ├── Infrastructure  — EF Core, repositórios, migrations, seed
 └── Web             — Blazor Server (pages, components, services HTTP)
```

**Direção de dependência:** `Api → Application → Domain ← Infrastructure` / `Web → Api (HTTP)`

### Decisões arquiteturais

**Clean Architecture**
Domínio isolado de frameworks. Lógica de negócio (descontos, validação de duplicatas) vive exclusivamente em `Domain`, sem dependência de EF Core ou ASP.NET.

**Strategy Pattern para descontos**
Cada regra é uma classe independente que implementa `IDiscountStrategy`. Adicionar nova regra = nova classe, sem alterar as existentes. Registradas no DI como `IEnumerable<IDiscountStrategy>` e avaliadas por prioridade (maior combo primeiro).

**Result Pattern**
Erros de negócio retornados como `Result<T>` em vez de exceções. O controller inspeciona `IsSuccess` e escolhe o status HTTP adequado.

**CQRS leve**
Sem MediatR — handlers são classes simples injetadas diretamente nos controllers. Separa leitura de escrita sem dependências externas.

**Soft delete em pedidos**
`Order` possui `IsDeleted` e `DeletedAt`. Global query filter no EF Core garante que registros deletados nunca apareçam nas queries por padrão.

**Update de itens do pedido**
`OrderRepository.UpdateAsync` deleta explicitamente os `OrderItem` antigos e re-insere os novos em vez de usar `_context.Update()`, evitando conflitos de change tracking do EF Core com coleções substituídas.

---

## O que foi deixado de fora

- **Autenticação/Autorização** — sem JWT ou qualquer camada de auth; todas as rotas são públicas.
- **Paginação** — listagem de pedidos retorna todos os registros sem cursor ou page/size.
- **Rate limiting** — nenhum throttle na API.
- **Cache** — sem Redis; cada request bate direto no banco.
- **Observabilidade avançada** — logs estruturados estão presentes, mas sem tracing distribuído (OpenTelemetry) ou métricas exportadas.
- **Mensageria** — fluxo síncrono end-to-end; sem RabbitMQ ou eventos de domínio.
- **Testes de contrato** — não há testes de schema/contract entre API e frontend.
