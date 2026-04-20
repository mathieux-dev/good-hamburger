using GoodHamburger.Web.Models;

namespace GoodHamburger.Web.Services;

public interface IMenuService
{
    Task<List<MenuItemDto>> GetMenuAsync(CancellationToken ct = default);
}

public class MenuService : IMenuService
{
    private readonly ApiClient _api;
    private List<MenuItemDto>? _cache;

    public MenuService(ApiClient api) => _api = api;

    public async Task<List<MenuItemDto>> GetMenuAsync(CancellationToken ct = default)
    {
        _cache ??= await _api.GetMenuAsync(ct);
        return _cache;
    }
}
