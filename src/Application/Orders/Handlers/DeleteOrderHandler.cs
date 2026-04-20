using GoodHamburger.Application.Orders.Commands;
using GoodHamburger.Domain.Common;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Application.Orders.Handlers;

public class DeleteOrderHandler
{
    private readonly IOrderRepository _orderRepository;

    public DeleteOrderHandler(IOrderRepository orderRepository) =>
        _orderRepository = orderRepository;

    public async Task<Result> HandleAsync(DeleteOrderCommand command, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(command.OrderId, ct);
        if (order is null)
            return Result.Failure($"Order '{command.OrderId}' not found.");

        await _orderRepository.DeleteAsync(order, ct);
        return Result.Success();
    }
}
