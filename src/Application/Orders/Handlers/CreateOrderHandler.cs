using GoodHamburger.Application.Common;
using GoodHamburger.Application.DTOs;
using GoodHamburger.Application.Orders.Commands;
using GoodHamburger.Domain.Common;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GoodHamburger.Application.Orders.Handlers;

public class CreateOrderHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IEnumerable<IDiscountStrategy> _strategies;
    private readonly ILogger<CreateOrderHandler> _logger;

    public CreateOrderHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IEnumerable<IDiscountStrategy> strategies,
        ILogger<CreateOrderHandler> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _strategies = strategies;
        _logger = logger;
    }

    public async Task<Result<OrderDto>> HandleAsync(CreateOrderCommand command, CancellationToken ct = default)
    {
        var order = Order.Create(command.Customer, command.Note, command.ServiceType);

        foreach (var productId in command.Items)
        {
            var product = await _productRepository.GetByIdAsync(productId, ct);
            if (product is null)
            {
                _logger.LogWarning("Product {ProductId} not found when creating order", productId);
                return Result<OrderDto>.Failure($"Product '{productId}' not found.");
            }

            var result = order.AddItem(product, _strategies);
            if (result.IsFailure)
            {
                _logger.LogWarning("Order item validation failed: {Error}", result.Error);
                return Result<OrderDto>.Failure(result.Error);
            }
        }

        await _orderRepository.AddAsync(order, ct);
        _logger.LogInformation("Order {OrderId} created for customer '{Customer}' with {ItemCount} item(s)",
            order.Id, order.Customer, order.Items.Count);

        return Result<OrderDto>.Success(OrderMapper.ToDto(order));
    }
}
