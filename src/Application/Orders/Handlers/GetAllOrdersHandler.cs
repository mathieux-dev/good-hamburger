using GoodHamburger.Application.Common;
using GoodHamburger.Application.DTOs;
using GoodHamburger.Application.Orders.Queries;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Application.Orders.Handlers;

public class GetAllOrdersHandler
{
    private readonly IOrderRepository _orderRepository;

    public GetAllOrdersHandler(IOrderRepository orderRepository) =>
        _orderRepository = orderRepository;

    public async Task<IEnumerable<OrderDto>> HandleAsync(GetAllOrdersQuery query, CancellationToken ct = default)
    {
        var orders = await _orderRepository.GetAllAsync(ct);
        return orders.Select(OrderMapper.ToDto);
    }
}
