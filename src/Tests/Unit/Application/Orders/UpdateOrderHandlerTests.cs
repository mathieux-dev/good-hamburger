using FluentAssertions;
using GoodHamburger.Application.Orders.Commands;
using GoodHamburger.Application.Orders.Handlers;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;
using GoodHamburger.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace GoodHamburger.Tests.Unit.Application.Orders;

public class UpdateOrderHandlerTests
{
    private readonly IOrderRepository _orders = Substitute.For<IOrderRepository>();
    private readonly IProductRepository _products = Substitute.For<IProductRepository>();
    private readonly IDiscountStrategy[] _strategies = [];

    private UpdateOrderHandler Handler() => new(_orders, _products, _strategies, NullLogger<UpdateOrderHandler>.Instance);

    private static Product Sandwich() => new(Guid.NewGuid(), "X Burger",    5.00m, ProductCategory.Sandwich);
    private static Product Side()     => new(Guid.NewGuid(), "Batata Frita",2.00m, ProductCategory.Side);

    private static Order ExistingOrder()
    {
        var order = Order.Create("Old", "", "Salão");
        return order;
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenOrderNotFound()
    {
        var id = Guid.NewGuid();
        _orders.GetByIdAsync(id).Returns((Order?)null);

        var command = new UpdateOrderCommand(id, "Test", "", [], "Salão");
        var result = await Handler().HandleAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain(id.ToString());
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenProductNotFound()
    {
        var order = ExistingOrder();
        var missingId = Guid.NewGuid();
        _orders.GetByIdAsync(order.Id).Returns(order);
        _products.GetByIdAsync(missingId).Returns((Product?)null);

        var command = new UpdateOrderCommand(order.Id, "Test", "", [missingId], "Salão");
        var result = await Handler().HandleAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain(missingId.ToString());
    }

    [Fact]
    public async Task HandleAsync_ShouldUpdateOrder_AndPersist()
    {
        var order = ExistingOrder();
        var sandwich = Sandwich();
        var side = Side();
        _orders.GetByIdAsync(order.Id).Returns(order);
        _products.GetByIdAsync(sandwich.Id).Returns(sandwich);
        _products.GetByIdAsync(side.Id).Returns(side);

        var command = new UpdateOrderCommand(order.Id, "Nova Mesa", "obs", [sandwich.Id, side.Id], "Delivery");
        var result = await Handler().HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Customer.Should().Be("Nova Mesa");
        result.Value.ServiceType.Should().Be("Delivery");
        result.Value.Items.Should().HaveCount(2);
        await _orders.Received(1).UpdateAsync(order);
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenDuplicateCategory()
    {
        var order = ExistingOrder();
        var s1 = Sandwich();
        var s2 = Sandwich();
        _orders.GetByIdAsync(order.Id).Returns(order);
        _products.GetByIdAsync(s1.Id).Returns(s1);
        _products.GetByIdAsync(s2.Id).Returns(s2);

        var command = new UpdateOrderCommand(order.Id, "Test", "", [s1.Id, s2.Id], "Salão");
        var result = await Handler().HandleAsync(command);

        result.IsFailure.Should().BeTrue();
        await _orders.DidNotReceive().UpdateAsync(Arg.Any<Order>());
    }
}
