using GoodHamburger.Web.Models;

namespace GoodHamburger.Web.Services;

public interface IProductService
{
    Task<List<MenuItemDto>> ListAsync(CancellationToken ct = default);
    Task<MenuItemDto> CreateAsync(ProductWriteRequest req, CancellationToken ct = default);
    Task<MenuItemDto> UpdateAsync(string id, ProductWriteRequest req, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
}

public record ProductWriteRequest(string Name, decimal Price, string Category, string? ImageUrl);

public class ProductService : IProductService
{
    private readonly ApiClient _api;
    public ProductService(ApiClient api) => _api = api;

    public Task<List<MenuItemDto>> ListAsync(CancellationToken ct = default)  => _api.GetProductsAsync(ct);
    public Task<MenuItemDto> CreateAsync(ProductWriteRequest req, CancellationToken ct = default) => _api.CreateProductAsync(req, ct);
    public Task<MenuItemDto> UpdateAsync(string id, ProductWriteRequest req, CancellationToken ct = default) => _api.UpdateProductAsync(id, req, ct);
    public Task DeleteAsync(string id, CancellationToken ct = default) => _api.DeleteProductAsync(id, ct);
}
