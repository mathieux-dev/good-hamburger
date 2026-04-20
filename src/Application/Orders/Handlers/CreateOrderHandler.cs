using GoodHamburger.Application.Common;
using GoodHamburger.Application.DTOs;
using GoodHamburger.Application.Orders.Commands;
using GoodHamburger.Domain.Common;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Application.Orders.Handlers;

public class CreateOrderHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IEnumerable<IDiscountStrategy> _strategies;

    public CreateOrderHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IEnumerable<IDiscountStrategy> strategies)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _strategies = strategies;
    }

    public async Task<Result<OrderDto>> HandleAsync(CreateOrderCommand command, CancellationToken ct = default)
    {
        var order = Order.Create(command.Customer, command.Note);

        foreach (var productId in command.Items)
        {
            var product = await _productRepository.GetByIdAsync(productId, ct);
            if (product is null)
                return Result<OrderDto>.Failure($"Product '{productId}' not found.");

            var result = order.AddItem(product, _strategies);
            if (result.IsFailure)
                return Result<OrderDto>.Failure(result.Error);
        }

        await _orderRepository.AddAsync(order, ct);
        return Result<OrderDto>.Success(OrderMapper.ToDto(order));
    }
}
