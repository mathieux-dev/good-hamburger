using GoodHamburger.Application.Common;
using GoodHamburger.Application.DTOs;
using GoodHamburger.Application.Orders.Commands;
using GoodHamburger.Domain.Common;
using GoodHamburger.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GoodHamburger.Application.Orders.Handlers;

public class UpdateOrderHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IEnumerable<IDiscountStrategy> _strategies;
    private readonly ILogger<UpdateOrderHandler> _logger;

    public UpdateOrderHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IEnumerable<IDiscountStrategy> strategies,
        ILogger<UpdateOrderHandler> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _strategies = strategies;
        _logger = logger;
    }

    public async Task<Result<OrderDto>> HandleAsync(UpdateOrderCommand command, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(command.OrderId, ct);
        if (order is null)
        {
            _logger.LogWarning("Order {OrderId} not found for update", command.OrderId);
            return Result<OrderDto>.Failure($"Order '{command.OrderId}' not found.");
        }

        var products = new List<Domain.Entities.Product>();

        foreach (var productId in command.Items)
        {
            var product = await _productRepository.GetByIdAsync(productId, ct);
            if (product is null)
            {
                _logger.LogWarning("Product {ProductId} not found when updating order {OrderId}", productId, command.OrderId);
                return Result<OrderDto>.Failure($"Product '{productId}' not found.");
            }

            products.Add(product);
        }

        var result = order.SetItems(products, _strategies);
        if (result.IsFailure)
        {
            _logger.LogWarning("Order {OrderId} item validation failed: {Error}", command.OrderId, result.Error);
            return Result<OrderDto>.Failure(result.Error);
        }

        order.Update(command.Customer, command.Note, command.ServiceType);

        await _orderRepository.UpdateAsync(order, ct);
        _logger.LogInformation("Order {OrderId} updated with {ItemCount} item(s)", order.Id, order.Items.Count);

        return Result<OrderDto>.Success(OrderMapper.ToDto(order));
    }
}
