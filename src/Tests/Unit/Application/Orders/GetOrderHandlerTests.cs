using FluentAssertions;
using GoodHamburger.Application.Orders.Handlers;
using GoodHamburger.Application.Orders.Queries;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;
using NSubstitute;

namespace GoodHamburger.Tests.Unit.Application.Orders;

public class GetOrderHandlerTests
{
    private readonly IOrderRepository _orders = Substitute.For<IOrderRepository>();

    private GetOrderHandler Handler() => new(_orders);

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenOrderNotFound()
    {
        var id = Guid.NewGuid();
        _orders.GetByIdAsync(id).Returns((Order?)null);

        var result = await Handler().HandleAsync(new GetOrderQuery(id));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain(id.ToString());
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnDto_WhenOrderExists()
    {
        var order = Order.Create("Mesa 3", "", "Retirada");
        _orders.GetByIdAsync(order.Id).Returns(order);

        var result = await Handler().HandleAsync(new GetOrderQuery(order.Id));

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(order.Id.ToString());
        result.Value.Customer.Should().Be("Mesa 3");
        result.Value.ServiceType.Should().Be("Retirada");
    }
}
