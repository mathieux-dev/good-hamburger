# Good Hamburger — Frontend Blazor

Frontend em **Blazor Server (.NET 8)** para o desafio técnico da Good Hamburger. Consome a API REST (`GoodHamburger.Api`) e entrega a experiência retrô/vintage com cálculo de desconto em tempo real, validação de itens duplicados e painel de pedidos.

## 🚀 Como rodar

```bash
# dentro da pasta GoodHamburger.Web
dotnet restore
dotnet run
```

Acesse: **http://localhost:5001**

> Antes, certifique-se que a API está rodando em `http://localhost:5000`. Ajuste em `appsettings.json` se necessário:
> ```json
> "GoodHamburger": { "ApiBaseUrl": "http://localhost:5000" }
> ```

## 📁 Estrutura

```
GoodHamburger.Web/
├── Program.cs                       # Bootstrap: DI, Blazor Server, HttpClient
├── appsettings.json
├── App.razor                        # Root router
├── _Imports.razor
├── Pages/
│   ├── _Host.cshtml                 # Host page
│   ├── Index.razor                  # Landing
│   ├── OrderPage.razor              # Montar pedido (consome cardápio)
│   ├── Admin.razor                  # Listagem com CRUD
│   └── Ticket.razor                 # Recibo final
├── Shared/
│   ├── MainLayout.razor             # Layout com sidebar
│   ├── NavMenu.razor
│   └── ConnectionStatus.razor       # Badge SignalR
├── Components/
│   ├── MenuItemCard.razor
│   ├── ComboCard.razor
│   ├── OrderCart.razor
│   ├── FoodIcon.razor               # Ilustrações SVG
│   └── StatusBadge.razor
├── Services/
│   ├── ApiClient.cs                 # HttpClient tipado
│   ├── IOrderService.cs
│   ├── OrderService.cs              # Orquestra chamadas + estado local
│   ├── IMenuService.cs
│   └── MenuService.cs
├── Models/
│   ├── MenuItemDto.cs
│   ├── OrderDto.cs
│   ├── OrderRequest.cs
│   ├── DiscountRule.cs
│   └── Category.cs
└── wwwroot/
    ├── css/site.css                 # Paleta verde/retrô
    └── favicon.ico
```

## 🎨 Design system

- **Paleta**: verde Palmeiras (`#1c5236`) + creme vintage (`#f3e8c8`) + mostarda (`#d4a537`) + vermelho diner (`#b83a2e`)
- **Tipografia**: Alfa Slab One (displays) + Archivo (body) + DM Mono (mono)
- **Look**: sombras offset sólidas (estilo risograph), bordas arredondadas, selos "carimbados"

## 💡 Decisões de arquitetura

- **Blazor Server** (não WebAssembly): SignalR nativo traz reatividade sem polling, e o footprint inicial é menor.
- **HttpClient tipado** (`ApiClient`) registrado via `AddHttpClient<>()` — testável e isolado.
- **OrderService** centraliza a lógica de UI (item selecionado, validação local antes de POST).
- **Validação dupla**: cliente valida itens duplicados antes de enviar (UX) + servidor também valida (segurança).
- **DiscountRule** é um enum + record espelhando a regra do backend — o frontend *calcula localmente* pra mostrar em tempo real, mas o total final **vem da API** (fonte de verdade).

## 🔌 Endpoints consumidos

| Método | Rota | Uso |
|---|---|---|
| GET | `/api/menu` | Carrega cardápio ao abrir `OrderPage` |
| POST | `/api/orders` | Cria pedido |
| GET | `/api/orders` | Lista no `Admin` |
| GET | `/api/orders/{id}` | Consulta individual |
| PUT | `/api/orders/{id}` | Atualização inline |
| DELETE | `/api/orders/{id}` | Remoção |

## ✅ Entregáveis cumpridos

- [x] Frontend Blazor consumindo a API
- [x] Cálculo de desconto visível em tempo real (10% / 15% / 20%)
- [x] Mensagens de erro claras (item duplicado, pedido vazio)
- [x] CRUD completo na tela de Admin
- [x] Ticket/recibo visual após criação
- [x] Tratamento de estados de loading e erro

## 🧪 Testar localmente

```bash
# terminal 1 — API
cd GoodHamburger.Api
dotnet run

# terminal 2 — Web
cd GoodHamburger.Web
dotnet run
```

Depois abra `http://localhost:5001` no navegador.
