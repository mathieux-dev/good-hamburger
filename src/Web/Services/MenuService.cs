using GoodHamburger.Web.Models;

namespace GoodHamburger.Web.Services;

public interface IMenuService
{
    Task<List<MenuItemDto>> GetMenuAsync(CancellationToken ct = default);
}

public class MenuService : IMenuService
{
    private readonly ApiClient _api;
    public MenuService(ApiClient api) => _api = api;

    public async Task<List<MenuItemDto>> GetMenuAsync(CancellationToken ct = default)
        => await _api.GetMenuAsync(ct);
}
