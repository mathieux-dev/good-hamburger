using FluentAssertions;
using GoodHamburger.Application.Orders.Commands;
using GoodHamburger.Application.Orders.Handlers;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;
using NSubstitute;

namespace GoodHamburger.Tests.Unit.Application.Orders;

public class DeleteOrderHandlerTests
{
    private readonly IOrderRepository _orders = Substitute.For<IOrderRepository>();

    private DeleteOrderHandler Handler() => new(_orders);

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenOrderNotFound()
    {
        var id = Guid.NewGuid();
        _orders.GetByIdAsync(id).Returns((Order?)null);

        var result = await Handler().HandleAsync(new DeleteOrderCommand(id));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain(id.ToString());
        await _orders.DidNotReceive().DeleteAsync(Arg.Any<Order>());
    }

    [Fact]
    public async Task HandleAsync_ShouldSoftDelete_WhenOrderExists()
    {
        var order = Order.Create("Test");
        _orders.GetByIdAsync(order.Id).Returns(order);

        var result = await Handler().HandleAsync(new DeleteOrderCommand(order.Id));

        result.IsSuccess.Should().BeTrue();
        await _orders.Received(1).DeleteAsync(order);
    }
}
