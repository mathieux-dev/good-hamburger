using GoodHamburger.Application.Orders.Commands;
using GoodHamburger.Domain.Common;
using GoodHamburger.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GoodHamburger.Application.Orders.Handlers;

public class DeleteOrderHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<DeleteOrderHandler> _logger;

    public DeleteOrderHandler(IOrderRepository orderRepository, ILogger<DeleteOrderHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(DeleteOrderCommand command, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(command.OrderId, ct);
        if (order is null)
        {
            _logger.LogWarning("Order {OrderId} not found for deletion", command.OrderId);
            return Result.Failure($"Order '{command.OrderId}' not found.");
        }

        await _orderRepository.DeleteAsync(order, ct);
        _logger.LogInformation("Order {OrderId} deleted", command.OrderId);

        return Result.Success();
    }
}
