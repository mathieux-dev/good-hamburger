# Good Hamburger — Sistema de Pedidos

API REST para gerenciamento de pedidos de uma lanchonete, com regras de negócio para montagem de combos e aplicação automática de descontos.

---

## Tecnologias

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Docker / Docker Compose

---

## Arquitetura

Clean Architecture com DDD Lite, organizada em camadas:

```
src/
 ├── Api             # Controllers, entrada HTTP
 ├── Application     # Casos de uso (CQRS leve)
 ├── Domain          # Entidades e regras de negócio
 ├── Infrastructure  # Repositórios e persistência
 └── Tests           # Testes unitários e de integração
```

Padrões utilizados: Repository, Service Layer, Strategy (descontos), Result Pattern, Dependency Injection.

---

## Cardápio

| Produto       | Categoria | Preço  |
|---------------|-----------|--------|
| X Burger      | SANDWICH  | R$ 5,00 |
| X Egg         | SANDWICH  | R$ 4,50 |
| X Bacon       | SANDWICH  | R$ 7,00 |
| Batata Frita  | SIDE      | R$ 2,00 |
| Refrigerante  | DRINK     | R$ 2,50 |

---

## Regras de Negócio

Um pedido pode conter no máximo **um item por categoria** (`SANDWICH`, `SIDE`, `DRINK`). Itens duplicados retornam erro.

### Descontos aplicados automaticamente

| Combinação                        | Desconto |
|-----------------------------------|----------|
| Sanduíche + Batata + Refrigerante | 20%      |
| Sanduíche + Refrigerante          | 15%      |
| Sanduíche + Batata                | 10%      |

---

## Endpoints

### Pedidos

| Método | Rota           | Descrição       |
|--------|----------------|-----------------|
| POST   | /orders        | Criar pedido    |
| GET    | /orders        | Listar pedidos  |
| GET    | /orders/{id}   | Buscar por ID   |
| PUT    | /orders/{id}   | Atualizar       |
| DELETE | /orders/{id}   | Remover         |

### Cardápio

| Método | Rota   | Descrição        |
|--------|--------|------------------|
| GET    | /menu  | Listar produtos  |

---

## Como rodar

```bash
docker-compose up --build
```

A API ficará disponível em `http://localhost:5000`.

---

## Testes

```bash
dotnet test
```

Cobertura:
- **Unitários** — regras de desconto e validação de itens duplicados
- **Integração** — fluxo completo de criação de pedido
