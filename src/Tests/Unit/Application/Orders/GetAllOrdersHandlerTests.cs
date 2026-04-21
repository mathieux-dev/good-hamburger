using FluentAssertions;
using GoodHamburger.Application.Orders.Handlers;
using GoodHamburger.Application.Orders.Queries;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;
using NSubstitute;

namespace GoodHamburger.Tests.Unit.Application.Orders;

public class GetAllOrdersHandlerTests
{
    private readonly IOrderRepository _orders = Substitute.For<IOrderRepository>();

    private GetAllOrdersHandler Handler() => new(_orders);

    [Fact]
    public async Task HandleAsync_ShouldReturnEmpty_WhenNoOrders()
    {
        _orders.GetAllAsync().Returns([]);

        var result = await Handler().HandleAsync(new GetAllOrdersQuery());

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ShouldMapAllOrders()
    {
        var orders = new[] { Order.Create("A"), Order.Create("B"), Order.Create("C") };
        _orders.GetAllAsync().Returns(orders);

        var result = await Handler().HandleAsync(new GetAllOrdersQuery());

        result.Should().HaveCount(3);
        result.Select(o => o.Customer).Should().BeEquivalentTo(["A", "B", "C"]);
    }
}
