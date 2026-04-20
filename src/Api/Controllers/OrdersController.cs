using GoodHamburger.Application.Orders.Commands;
using GoodHamburger.Application.Orders.Handlers;
using GoodHamburger.Application.Orders.Queries;
using Microsoft.AspNetCore.Mvc;

namespace GoodHamburger.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly CreateOrderHandler _createHandler;
    private readonly UpdateOrderHandler _updateHandler;
    private readonly DeleteOrderHandler _deleteHandler;
    private readonly GetOrderHandler _getHandler;
    private readonly GetAllOrdersHandler _getAllHandler;

    public OrdersController(
        CreateOrderHandler createHandler,
        UpdateOrderHandler updateHandler,
        DeleteOrderHandler deleteHandler,
        GetOrderHandler getHandler,
        GetAllOrdersHandler getAllHandler)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _getHandler = getHandler;
        _getAllHandler = getAllHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _getAllHandler.HandleAsync(new GetAllOrdersQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _getHandler.HandleAsync(new GetOrderQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand command, CancellationToken ct)
    {
        var result = await _createHandler.HandleAsync(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrderRequest request, CancellationToken ct)
    {
        var result = await _updateHandler.HandleAsync(new UpdateOrderCommand(id, request.Customer, request.Note, request.Items), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _deleteHandler.HandleAsync(new DeleteOrderCommand(id), ct);
        return result.IsSuccess ? NoContent() : NotFound(new { error = result.Error });
    }
}

public record UpdateOrderRequest(string Customer, string Note, IEnumerable<Guid> Items);
