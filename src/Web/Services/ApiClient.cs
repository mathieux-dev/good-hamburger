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
        try
        {
            var menu = await _http.GetFromJsonAsync<List<MenuItemDto>>("api/menu", _jsonOptions, ct);
            return menu ?? new List<MenuItemDto>();
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Falha ao consultar cardápio, usando fallback local.");
            return FallbackMenu();
        }
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
            throw new ApiException(error?.Message ?? "Falha ao criar pedido.", error);
        }
        return (await resp.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions, cancellationToken: ct))!;
    }

    public async Task UpdateOrderAsync(string id, OrderRequest req, CancellationToken ct = default)
    {
        var resp = await _http.PutAsJsonAsync($"api/orders/{id}", req, _jsonOptions, ct);
        resp.EnsureSuccessStatusCode();
    }

    public async Task DeleteOrderAsync(string id, CancellationToken ct = default)
    {
        var resp = await _http.DeleteAsync($"api/orders/{id}", ct);
        resp.EnsureSuccessStatusCode();
    }

    private static List<MenuItemDto> FallbackMenu() => new()
    {
        new("11111111-0000-0000-0000-000000000001", "X Burger",     5.00m, Category.Sandwich, "pão + burger + queijo", "O original.",                "burger"),
        new("11111111-0000-0000-0000-000000000002", "X Egg",        4.50m, Category.Sandwich, "pão + burger + ovo",    "Gema escorrendo.",            "egg"),
        new("11111111-0000-0000-0000-000000000003", "X Bacon",      7.00m, Category.Sandwich, "pão + burger + bacon",  "Favorito da casa.",           "bacon"),
        new("11111111-0000-0000-0000-000000000004", "Batata Frita", 2.00m, Category.Side,     "porção crocante",       "Corte rústico, sal grosso.",  "fries"),
        new("11111111-0000-0000-0000-000000000005", "Refrigerante", 2.50m, Category.Drink,    "lata 350ml",            "Gelado.",                     "soda"),
    };
}

public record ApiError(string Error, string Message, string? Category);

public class ApiException : Exception
{
    public ApiError? Error { get; }
    public ApiException(string message, ApiError? error) : base(message) => Error = error;
}
