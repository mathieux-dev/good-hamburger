using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GoodHamburger.Web.Models;

namespace GoodHamburger.Web.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<ApiClient> _log;

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public ApiClient(HttpClient http, ILogger<ApiClient> log)
    {
        _http = http;
        _log = log;
    }

    public async Task<List<MenuItemDto>> GetMenuAsync(CancellationToken ct = default)
    {
        var menu = await _http.GetFromJsonAsync<List<MenuItemDto>>("api/menu", _jsonOptions, ct);
        return menu ?? new List<MenuItemDto>();
    }

    public async Task<List<OrderDto>> GetOrdersAsync(CancellationToken ct = default)
    {
        try
        {
            var res = await _http.GetFromJsonAsync<List<OrderDto>>("api/orders", _jsonOptions, ct);
            return res ?? new();
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Falha ao consultar pedidos.");
            return new();
        }
    }

    public async Task<OrderDto?> GetOrderAsync(string id, CancellationToken ct = default)
    {
        try
        {
            return await _http.GetFromJsonAsync<OrderDto>($"api/orders/{id}", _jsonOptions, ct);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Falha ao consultar pedido {Id}.", id);
            return null;
        }
    }

    public async Task<OrderDto> CreateOrderAsync(OrderRequest req, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("api/orders", req, _jsonOptions, ct);
        if (!resp.IsSuccessStatusCode)
        {
            var error = await resp.Content.ReadFromJsonAsync<ApiError>(_jsonOptions, cancellationToken: ct);
            throw new ApiException(error?.Resolved ?? "Falha ao criar pedido.", error);
        }
        return (await resp.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions, cancellationToken: ct))!;
    }

    public async Task UpdateOrderAsync(string id, OrderRequest req, CancellationToken ct = default)
    {
        var resp = await _http.PutAsJsonAsync($"api/orders/{id}", req, _jsonOptions, ct);
        if (!resp.IsSuccessStatusCode)
        {
            var error = await resp.Content.ReadFromJsonAsync<ApiError>(_jsonOptions, cancellationToken: ct);
            throw new ApiException(error?.Resolved ?? "Falha ao atualizar pedido.", error);
        }
    }

    public async Task<List<MenuItemDto>> GetProductsAsync(CancellationToken ct = default)
    {
        var res = await _http.GetFromJsonAsync<List<MenuItemDto>>("api/products", _jsonOptions, ct);
        return res ?? new();
    }

    public async Task<MenuItemDto> CreateProductAsync(ProductWriteRequest req, CancellationToken ct = default)
    {
        var body = new { req.Name, req.Price, req.Category, req.Subtitle, req.Description, req.ImageUrl };
        var resp = await _http.PostAsJsonAsync("api/products", body, _jsonOptions, ct);
        if (!resp.IsSuccessStatusCode)
        {
            var error = await resp.Content.ReadFromJsonAsync<ApiError>(_jsonOptions, cancellationToken: ct);
            throw new ApiException(error?.Resolved ?? "Falha ao criar produto.", error);
        }
        return (await resp.Content.ReadFromJsonAsync<MenuItemDto>(_jsonOptions, cancellationToken: ct))!;
    }

    public async Task<MenuItemDto> UpdateProductAsync(string id, ProductWriteRequest req, CancellationToken ct = default)
    {
        var body = new { req.Name, req.Price, req.Category, req.Subtitle, req.Description, req.ImageUrl };
        var resp = await _http.PutAsJsonAsync($"api/products/{id}", body, _jsonOptions, ct);
        if (!resp.IsSuccessStatusCode)
        {
            var error = await resp.Content.ReadFromJsonAsync<ApiError>(_jsonOptions, cancellationToken: ct);
            throw new ApiException(error?.Resolved ?? "Falha ao atualizar produto.", error);
        }
        return (await resp.Content.ReadFromJsonAsync<MenuItemDto>(_jsonOptions, cancellationToken: ct))!;
    }

    public async Task DeleteProductAsync(string id, CancellationToken ct = default)
    {
        var resp = await _http.DeleteAsync($"api/products/{id}", ct);
        resp.EnsureSuccessStatusCode();
    }

    public async Task UpdateStatusAsync(string id, string status, CancellationToken ct = default)
    {
        var resp = await _http.PatchAsJsonAsync($"api/orders/{id}/status", new { status }, _jsonOptions, ct);
        resp.EnsureSuccessStatusCode();
    }

    public async Task DeleteOrderAsync(string id, CancellationToken ct = default)
    {
        var resp = await _http.DeleteAsync($"api/orders/{id}", ct);
        resp.EnsureSuccessStatusCode();
    }

}

public record ApiError(string? Error, string? Message, string? Category)
{
    public string Resolved => Message ?? Error ?? "Erro desconhecido.";
}

public class ApiException : Exception
{
    public ApiError? Error { get; }
    public ApiException(string message, ApiError? error) : base(message) => Error = error;
}
