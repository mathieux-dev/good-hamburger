using GoodHamburger.Web.Models;

namespace GoodHamburger.Web.Services;

public interface IOrderService
{
    Task<List<OrderDto>> ListAsync(CancellationToken ct = default);
    Task<OrderDto?> GetAsync(string id, CancellationToken ct = default);
    Task<OrderDto> CreateAsync(OrderRequest req, CancellationToken ct = default);
    Task UpdateAsync(string id, OrderRequest req, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Valida localmente (antes de chamar a API) que não há itens duplicados por categoria.
    /// </summary>
    (bool ok, string? error) ValidateLocal(IEnumerable<MenuItemDto> items);
}

public class OrderService : IOrderService
{
    private readonly ApiClient _api;

    public OrderService(ApiClient api) => _api = api;

    public Task<List<OrderDto>> ListAsync(CancellationToken ct = default) => _api.GetOrdersAsync(ct);
    public Task<OrderDto?> GetAsync(string id, CancellationToken ct = default) => _api.GetOrderAsync(id, ct);
    public Task<OrderDto> CreateAsync(OrderRequest req, CancellationToken ct = default) => _api.CreateOrderAsync(req, ct);
    public Task UpdateAsync(string id, OrderRequest req, CancellationToken ct = default) => _api.UpdateOrderAsync(id, req, ct);
    public Task DeleteAsync(string id, CancellationToken ct = default) => _api.DeleteOrderAsync(id, ct);

    public (bool ok, string? error) ValidateLocal(IEnumerable<MenuItemDto> items)
    {
        var dupe = items.GroupBy(i => i.Category)
                        .FirstOrDefault(g => g.Count() > 1);

        if (dupe is not null)
            return (false, $"Só é permitido 1 {dupe.Key.Label().ToLower()} por pedido.");

        if (!items.Any())
            return (false, "Adicione pelo menos 1 item ao pedido.");

        return (true, null);
    }
}
