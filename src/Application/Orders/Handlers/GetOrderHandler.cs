using GoodHamburger.Application.Common;
using GoodHamburger.Application.DTOs;
using GoodHamburger.Application.Orders.Queries;
using GoodHamburger.Domain.Common;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Application.Orders.Handlers;

public class GetOrderHandler
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderHandler(IOrderRepository orderRepository) =>
        _orderRepository = orderRepository;

    public async Task<Result<OrderDto>> HandleAsync(GetOrderQuery query, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(query.OrderId, ct);
        if (order is null)
            return Result<OrderDto>.Failure($"Order '{query.OrderId}' not found.");

        return Result<OrderDto>.Success(OrderMapper.ToDto(order));
    }
}
