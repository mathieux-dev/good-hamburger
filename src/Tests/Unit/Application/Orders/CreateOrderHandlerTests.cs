using FluentAssertions;
using GoodHamburger.Application.Orders.Commands;
using GoodHamburger.Application.Orders.Handlers;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;
using GoodHamburger.Domain.Interfaces;
using NSubstitute;

namespace GoodHamburger.Tests.Unit.Application.Orders;

public class CreateOrderHandlerTests
{
    private readonly IOrderRepository _orders = Substitute.For<IOrderRepository>();
    private readonly IProductRepository _products = Substitute.For<IProductRepository>();
    private readonly IDiscountStrategy[] _strategies = [];

    private CreateOrderHandler Handler() => new(_orders, _products, _strategies);

    private static Product Sandwich() => new(Guid.NewGuid(), "X Burger",    5.00m, ProductCategory.Sandwich);
    private static Product Side()     => new(Guid.NewGuid(), "Batata Frita",2.00m, ProductCategory.Side);
    private static Product Drink()    => new(Guid.NewGuid(), "Refrigerante",2.50m, ProductCategory.Drink);

    [Fact]
    public async Task HandleAsync_ShouldCreateOrder_WithCorrectTotals()
    {
        var sandwich = Sandwich();
        var side = Side();
        _products.GetByIdAsync(sandwich.Id).Returns(sandwich);
        _products.GetByIdAsync(side.Id).Returns(side);

        var command = new CreateOrderCommand("Mesa 1", "", [sandwich.Id, side.Id], "Salão");
        var result = await Handler().HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Subtotal.Should().Be(7.00m);
        result.Value.Customer.Should().Be("Mesa 1");
        await _orders.Received(1).AddAsync(Arg.Any<Order>());
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenProductNotFound()
    {
        var missingId = Guid.NewGuid();
        _products.GetByIdAsync(missingId).Returns((Product?)null);

        var command = new CreateOrderCommand("Test", "", [missingId], "Salão");
        var result = await Handler().HandleAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain(missingId.ToString());
        await _orders.DidNotReceive().AddAsync(Arg.Any<Order>());
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenDuplicateCategory()
    {
        var s1 = Sandwich();
        var s2 = Sandwich();
        _products.GetByIdAsync(s1.Id).Returns(s1);
        _products.GetByIdAsync(s2.Id).Returns(s2);

        var command = new CreateOrderCommand("Test", "", [s1.Id, s2.Id], "Salão");
        var result = await Handler().HandleAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Sandwich");
    }

    [Fact]
    public async Task HandleAsync_ShouldApplyDiscount_WithFullCombo()
    {
        var sandwich = Sandwich();
        var side = Side();
        var drink = Drink();
        _products.GetByIdAsync(sandwich.Id).Returns(sandwich);
        _products.GetByIdAsync(side.Id).Returns(side);
        _products.GetByIdAsync(drink.Id).Returns(drink);

        var strategies = new IDiscountStrategy[]
        {
            new Domain.Discounts.FullComboDiscountStrategy(),
            new Domain.Discounts.SandwichDrinkDiscountStrategy(),
            new Domain.Discounts.SandwichSideDiscountStrategy(),
        };
        var handler = new CreateOrderHandler(_orders, _products, strategies);
        var command = new CreateOrderCommand("Test", "", [sandwich.Id, side.Id, drink.Id], "Salão");

        var result = await handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.DiscountRate.Should().Be(0.20m);
        result.Value.Total.Should().Be(result.Value.Subtotal * 0.80m);
    }

    [Fact]
    public async Task HandleAsync_ShouldDefaultCustomer_WhenBlank()
    {
        var sandwich = Sandwich();
        _products.GetByIdAsync(sandwich.Id).Returns(sandwich);

        var command = new CreateOrderCommand("", "", [sandwich.Id], "Salão");
        var result = await Handler().HandleAsync(command);

        result.Value.Customer.Should().Be("Balcão");
    }
}
