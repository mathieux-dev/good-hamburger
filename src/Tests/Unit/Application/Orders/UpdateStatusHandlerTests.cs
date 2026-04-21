using FluentAssertions;
using GoodHamburger.Application.Orders.Commands;
using GoodHamburger.Application.Orders.Handlers;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;
using NSubstitute;

namespace GoodHamburger.Tests.Unit.Application.Orders;

public class UpdateStatusHandlerTests
{
    private readonly IOrderRepository _orders = Substitute.For<IOrderRepository>();

    private UpdateStatusHandler Handler() => new(_orders);

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenOrderNotFound()
    {
        var id = Guid.NewGuid();
        _orders.GetByIdAsync(id).Returns((Order?)null);

        var result = await Handler().HandleAsync(new UpdateStatusCommand(id, "pronto"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain(id.ToString());
    }

    [Fact]
    public async Task HandleAsync_ShouldUpdateStatus_AndPersist()
    {
        var order = Order.Create();
        _orders.GetByIdAsync(order.Id).Returns(order);

        var result = await Handler().HandleAsync(new UpdateStatusCommand(order.Id, "pronto"));

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be("pronto");
        await _orders.Received(1).UpdateAsync(order);
    }

    [Theory]
    [InlineData("preparando")]
    [InlineData("pronto")]
    [InlineData("entregue")]
    [InlineData("cancelado")]
    public async Task HandleAsync_ShouldAcceptAllValidStatuses(string status)
    {
        var order = Order.Create();
        _orders.GetByIdAsync(order.Id).Returns(order);

        var result = await Handler().HandleAsync(new UpdateStatusCommand(order.Id, status));

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(status);
    }
}
