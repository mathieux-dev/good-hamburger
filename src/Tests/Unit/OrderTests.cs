using FluentAssertions;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;

namespace GoodHamburger.Tests.Unit;

public class OrderTests
{
    private static readonly List<Domain.Interfaces.IDiscountStrategy> NoStrategies = [];

    private static Product MakeSandwich() => new(Guid.NewGuid(), "X Burger", 5.00m, ProductCategory.Sandwich);
    private static Product MakeSide()     => new(Guid.NewGuid(), "Batata Frita", 2.00m, ProductCategory.Side);

    [Fact]
    public void AddItem_ShouldSucceed_WhenCategoryIsNew()
    {
        var order = Order.Create();
        var result = order.AddItem(MakeSandwich(), NoStrategies);

        result.IsSuccess.Should().BeTrue();
        order.Items.Should().HaveCount(1);
    }

    [Fact]
    public void AddItem_ShouldFail_WhenCategoryAlreadyExists()
    {
        var order = Order.Create();
        order.AddItem(MakeSandwich(), NoStrategies);

        var result = order.AddItem(MakeSandwich(), NoStrategies);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Sandwich");
        order.Items.Should().HaveCount(1);
    }

    [Fact]
    public void AddItem_ShouldAllow_DifferentCategories()
    {
        var order = Order.Create();
        order.AddItem(MakeSandwich(), NoStrategies);
        var result = order.AddItem(MakeSide(), NoStrategies);

        result.IsSuccess.Should().BeTrue();
        order.Items.Should().HaveCount(2);
    }

    [Fact]
    public void RemoveItem_ShouldFail_WhenProductNotInOrder()
    {
        var order = Order.Create();
        var result = order.RemoveItem(Guid.NewGuid(), NoStrategies);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void RemoveItem_ShouldSucceed_AndRecalculateTotals()
    {
        var sandwich = MakeSandwich();
        var side = MakeSide();
        var order = Order.Create();
        order.AddItem(sandwich, NoStrategies);
        order.AddItem(side, NoStrategies);

        order.RemoveItem(side.Id, NoStrategies);

        order.Items.Should().HaveCount(1);
        order.Subtotal.Should().Be(5.00m);
    }
}
