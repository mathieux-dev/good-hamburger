using FluentAssertions;
using GoodHamburger.Domain.Discounts;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;

namespace GoodHamburger.Tests.Unit;

public class DiscountStrategyTests
{
    private static Product Sandwich => new(Guid.NewGuid(), "X Burger",    5.00m, ProductCategory.Sandwich);
    private static Product Side     => new(Guid.NewGuid(), "Batata Frita",2.00m, ProductCategory.Side);
    private static Product Drink    => new(Guid.NewGuid(), "Refrigerante",2.50m, ProductCategory.Drink);

    private static readonly IEnumerable<Domain.Interfaces.IDiscountStrategy> AllStrategies =
    [
        new FullComboDiscountStrategy(),
        new SandwichDrinkDiscountStrategy(),
        new SandwichSideDiscountStrategy()
    ];

    private static Order BuildOrder(params Product[] products)
    {
        var order = Order.Create();
        foreach (var p in products)
            order.AddItem(p, AllStrategies);
        return order;
    }

    [Fact]
    public void FullCombo_ShouldApply20PercentDiscount()
    {
        var order = BuildOrder(Sandwich, Side, Drink);

        order.Subtotal.Should().Be(9.50m);
        order.Discount.Should().Be(1.90m);
        order.Total.Should().Be(7.60m);
    }

    [Fact]
    public void SandwichDrink_ShouldApply15PercentDiscount()
    {
        var order = BuildOrder(Sandwich, Drink);

        order.Subtotal.Should().Be(7.50m);
        order.Discount.Should().Be(1.125m);
        order.Total.Should().Be(6.375m);
    }

    [Fact]
    public void SandwichSide_ShouldApply10PercentDiscount()
    {
        var order = BuildOrder(Sandwich, Side);

        order.Subtotal.Should().Be(7.00m);
        order.Discount.Should().Be(0.70m);
        order.Total.Should().Be(6.30m);
    }

    [Fact]
    public void SandwichOnly_ShouldApplyNoDiscount()
    {
        var order = BuildOrder(Sandwich);

        order.Discount.Should().Be(0);
        order.Total.Should().Be(order.Subtotal);
    }

    [Fact]
    public void DrinkOnly_ShouldApplyNoDiscount()
    {
        var order = BuildOrder(Drink);

        order.Discount.Should().Be(0);
        order.Total.Should().Be(order.Subtotal);
    }
}
