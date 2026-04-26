using GoodHamburger.Application.Orders.Commands;
using GoodHamburger.Domain.Common;
using GoodHamburger.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GoodHamburger.Application.Orders.Handlers;

public class UpdateStatusHandler
{
    private readonly IOrderRepository _repo;
    private readonly ILogger<UpdateStatusHandler> _logger;

    public UpdateStatusHandler(IOrderRepository repo, ILogger<UpdateStatusHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(UpdateStatusCommand command, CancellationToken ct = default)
    {
        var order = await _repo.GetByIdAsync(command.OrderId, ct);
        if (order is null)
        {
            _logger.LogWarning("Order {OrderId} not found for status update", command.OrderId);
            return Result.Failure($"Order '{command.OrderId}' not found.");
        }

        var previous = order.Status;
        order.SetStatus(command.Status);
        await _repo.UpdateAsync(order, ct);

        _logger.LogInformation("Order {OrderId} status changed from '{Previous}' to '{Status}'",
            command.OrderId, previous, command.Status);

        return Result.Success();
    }
}
