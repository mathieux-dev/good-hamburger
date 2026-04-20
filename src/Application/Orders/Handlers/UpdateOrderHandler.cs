using GoodHamburger.Application.Common;
using GoodHamburger.Application.DTOs;
using GoodHamburger.Application.Orders.Commands;
using GoodHamburger.Domain.Common;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Application.Orders.Handlers;

public class UpdateOrderHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IEnumerable<IDiscountStrategy> _strategies;

    public UpdateOrderHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IEnumerable<IDiscountStrategy> strategies)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _strategies = strategies;
    }

    public async Task<Result<OrderDto>> HandleAsync(UpdateOrderCommand command, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(command.OrderId, ct);
        if (order is null)
            return Result<OrderDto>.Failure($"Order '{command.OrderId}' not found.");

        var newItems = new List<Domain.Entities.OrderItem>();
        var tempOrder = Order.Create();

        foreach (var productId in command.ProductIds)
        {
            var product = await _productRepository.GetByIdAsync(productId, ct);
            if (product is null)
                return Result<OrderDto>.Failure($"Product '{productId}' not found.");

            var result = tempOrder.AddItem(product, _strategies);
            if (result.IsFailure)
                return Result<OrderDto>.Failure(result.Error);
        }

        order.SetItems(tempOrder.Items, _strategies);
        await _orderRepository.UpdateAsync(order, ct);
        return Result<OrderDto>.Success(OrderMapper.ToDto(order));
    }
}
