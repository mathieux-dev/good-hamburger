using GoodHamburger.Application.Menu.Handlers;
using GoodHamburger.Application.Menu.Queries;
using GoodHamburger.Application.Products.Commands;
using GoodHamburger.Application.Products.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace GoodHamburger.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly GetMenuHandler _getMenu;
    private readonly CreateProductHandler _create;
    private readonly UpdateProductHandler _update;
    private readonly DeleteProductHandler _delete;

    public ProductsController(
        GetMenuHandler getMenu,
        CreateProductHandler create,
        UpdateProductHandler update,
        DeleteProductHandler delete)
    {
        _getMenu = getMenu;
        _create  = create;
        _update  = update;
        _delete  = delete;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _getMenu.HandleAsync(new GetMenuQuery(), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductRequest req, CancellationToken ct)
    {
        var result = await _create.HandleAsync(
            new CreateProductCommand(req.Name, req.Price, req.Category, req.ImageUrl), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAll), result.Value)
            : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductRequest req, CancellationToken ct)
    {
        var result = await _update.HandleAsync(
            new UpdateProductCommand(id, req.Name, req.Price, req.ImageUrl), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _delete.HandleAsync(new DeleteProductCommand(id), ct);
        return result.IsSuccess ? NoContent() : NotFound(new { error = result.Error });
    }
}

public record ProductRequest(string Name, decimal Price, string Category, string? ImageUrl);
