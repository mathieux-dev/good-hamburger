using GoodHamburger.Application.Menu.Handlers;
using GoodHamburger.Application.Menu.Queries;
using Microsoft.AspNetCore.Mvc;

namespace GoodHamburger.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly GetMenuHandler _handler;

    public MenuController(GetMenuHandler handler) => _handler = handler;

    [HttpGet]
    public async Task<IActionResult> GetMenu(CancellationToken ct)
    {
        var result = await _handler.HandleAsync(new GetMenuQuery(), ct);
        return Ok(result);
    }
}
