using GoodHamburger.Application.Orders.Commands;
using GoodHamburger.Domain.Common;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Application.Orders.Handlers;

public class UpdateStatusHandler
{
    private readonly IOrderRepository _repo;

    public UpdateStatusHandler(IOrderRepository repo) => _repo = repo;

    public async Task<Result> HandleAsync(UpdateStatusCommand command, CancellationToken ct = default)
    {
        var order = await _repo.GetByIdAsync(command.OrderId, ct);
        if (order is null)
            return Result.Failure($"Order '{command.OrderId}' not found.");

        order.SetStatus(command.Status);
        await _repo.UpdateAsync(order, ct);
        return Result.Success();
    }
}
