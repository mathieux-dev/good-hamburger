# Good Hamburger — Sistema de Pedidos

API REST para gerenciamento de pedidos de uma lanchonete, com regras de negócio para montagem de combos e aplicação automática de descontos.

---

## Tecnologias

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core + Npgsql
- PostgreSQL
- Docker / Docker Compose
- xUnit + FluentAssertions + Testcontainers

---

## Como rodar

```bash
docker-compose up --build
```

A API ficará disponível em `http://localhost:5000`.  
Swagger em `http://localhost:5000/swagger`.

---

## Testes

```bash
# Todos os testes
dotnet test

# Apenas unitários (sem Docker)
dotnet test --filter "FullyQualifiedName~Unit"
```

Os testes de integração sobem um container PostgreSQL real via Testcontainers — Docker em execução é necessário.

---

## Cardápio

| Produto       | Categoria | Preço   |
|---------------|-----------|---------|
| X Burger      | SANDWICH  | R$ 5,00 |
| X Egg         | SANDWICH  | R$ 4,50 |
| X Bacon       | SANDWICH  | R$ 7,00 |
| Batata Frita  | SIDE      | R$ 2,00 |
| Refrigerante  | DRINK     | R$ 2,50 |

---

## Regras de Negócio

Um pedido pode conter no máximo **um item por categoria** (`SANDWICH`, `SIDE`, `DRINK`).

### Descontos aplicados automaticamente

| Combinação                        | Desconto |
|-----------------------------------|----------|
| Sanduíche + Batata + Refrigerante | 20%      |
| Sanduíche + Refrigerante          | 15%      |
| Sanduíche + Batata                | 10%      |

---

## Endpoints

### Pedidos

| Método | Rota           | Descrição      | Sucesso |
|--------|----------------|----------------|---------|
| POST   | /orders        | Criar pedido   | 201     |
| GET    | /orders        | Listar pedidos | 200     |
| GET    | /orders/{id}   | Buscar por ID  | 200     |
| PUT    | /orders/{id}   | Atualizar      | 200     |
| DELETE | /orders/{id}   | Remover        | 204     |

### Cardápio

| Método | Rota  | Descrição       |
|--------|-------|-----------------|
| GET    | /menu | Listar produtos |

---

## Arquitetura

Clean Architecture com DDD Lite em quatro camadas:

```
src/
 ├── Api             — Controllers, entrada HTTP, DI wiring
 ├── Application     — Casos de uso (CQRS leve: commands/queries + handlers)
 ├── Domain          — Entidades, regras de negócio, interfaces
 └── Infrastructure  — EF Core, repositórios, migrations, seed
```

**Direção de dependência:** `Api → Application → Domain ← Infrastructure`

### Decisões arquiteturais

**Clean Architecture**  
Mantém o domínio isolado de frameworks e infraestrutura. A lógica de negócio (descontos, validação de duplicatas) vive exclusivamente em `Domain`, sem dependência de EF Core ou ASP.NET.

**Strategy Pattern para descontos**  
Cada regra de desconto é uma classe independente que implementa `IDiscountStrategy`. Adicionar uma nova regra = nova classe, sem alterar as existentes. As strategies são registradas no DI como `IEnumerable<IDiscountStrategy>` e avaliadas em ordem de prioridade (maior combo primeiro).

**Result Pattern**  
Erros de negócio são retornados como `Result<T>` em vez de exceções. O controller inspeciona `IsSuccess` e escolhe o status HTTP adequado (400, 404, 201 etc.).

**CQRS leve**  
Sem MediatR — handlers são classes simples injetadas diretamente nos controllers. Separa leitura de escrita sem adicionar dependências externas.

**Seed via EF Core `HasData`**  
O cardápio com IDs fixos é semeado pela migration inicial, garantindo que os dados estejam presentes em qualquer ambiente ao executar `dotnet ef database update` ou via `Migrate()` no startup.

---

## O que foi deixado de fora

| Item | Motivo |
|------|--------|
| Autenticação / JWT | Fora do escopo do desafio |
| Frontend em Blazor | Diferencial opcional; priorizei cobertura de testes |
| Cache (Redis) | Sem requisito de performance no desafio |
| Paginação em `GET /orders` | Volume de dados não justifica neste contexto |
| Soft delete | Não especificado; `DELETE` remove permanentemente |
