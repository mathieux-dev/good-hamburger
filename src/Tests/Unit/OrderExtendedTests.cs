using FluentAssertions;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;

namespace GoodHamburger.Tests.Unit;

public class OrderExtendedTests
{
    private static readonly List<Domain.Interfaces.IDiscountStrategy> NoStrategies = [];

    private static Product MakeSandwich() => new(Guid.NewGuid(), "X Burger",    5.00m, ProductCategory.Sandwich);
    private static Product MakeSide()     => new(Guid.NewGuid(), "Batata Frita",2.00m, ProductCategory.Side);
    private static Product MakeDrink()    => new(Guid.NewGuid(), "Refrigerante",2.50m, ProductCategory.Drink);

    [Fact]
    public void Create_ShouldSetDefaults()
    {
        var order = Order.Create();

        order.Id.Should().NotBeEmpty();
        order.Customer.Should().Be("Balcão");
        order.Status.Should().Be("preparando");
        order.ServiceType.Should().Be("Salão");
        order.Items.Should().BeEmpty();
        order.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldSetCustomerAndServiceType()
    {
        var order = Order.Create("Mesa 5", "sem cebola", "Delivery");

        order.Customer.Should().Be("Mesa 5");
        order.Note.Should().Be("sem cebola");
        order.ServiceType.Should().Be("Delivery");
    }

    [Fact]
    public void Create_ShouldDefaultCustomer_WhenWhitespace()
    {
        var order = Order.Create("   ");

        order.Customer.Should().Be("Balcão");
    }

    [Fact]
    public void Update_ShouldChangeMetadata()
    {
        var order = Order.Create("Old", "", "Salão");

        order.Update("New", "obs nova", "Retirada");

        order.Customer.Should().Be("New");
        order.Note.Should().Be("obs nova");
        order.ServiceType.Should().Be("Retirada");
    }

    [Fact]
    public void SetStatus_ShouldChangeStatus()
    {
        var order = Order.Create();

        order.SetStatus("pronto");

        order.Status.Should().Be("pronto");
    }

    [Fact]
    public void SoftDelete_ShouldSetIsDeletedAndTimestamp()
    {
        var order = Order.Create();
        var before = DateTime.UtcNow;

        order.SoftDelete();

        order.IsDeleted.Should().BeTrue();
        order.DeletedAt.Should().NotBeNull();
        order.DeletedAt!.Value.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void HasCategory_ShouldReturnTrue_WhenCategoryPresent()
    {
        var order = Order.Create();
        order.AddItem(MakeSandwich(), NoStrategies);

        order.HasCategory(ProductCategory.Sandwich).Should().BeTrue();
        order.HasCategory(ProductCategory.Side).Should().BeFalse();
    }

    [Fact]
    public void SetItems_ShouldReplaceItems_AndRecalculate()
    {
        var strategies = new Domain.Discounts.FullComboDiscountStrategy[] { new() };
        var order = Order.Create();
        order.AddItem(MakeSandwich(), NoStrategies);

        var sandwich = MakeSandwich();
        var side = MakeSide();
        var drink = MakeDrink();
        var result = order.SetItems([sandwich, side, drink], strategies);

        result.IsSuccess.Should().BeTrue();
        order.Items.Should().HaveCount(3);
        order.Subtotal.Should().Be(9.50m);
        order.DiscountRate.Should().Be(0.20m);
    }

    [Fact]
    public void SetItems_ShouldFail_WhenDuplicateCategory()
    {
        var order = Order.Create();

        var result = order.SetItems([MakeSandwich(), MakeSandwich()], NoStrategies);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Sandwich");
        order.Items.Should().BeEmpty();
    }

    [Fact]
    public void AddItem_ShouldRecalculateTotals_WithFullCombo()
    {
        var strategies = new List<Domain.Interfaces.IDiscountStrategy>
        {
            new Domain.Discounts.FullComboDiscountStrategy(),
            new Domain.Discounts.SandwichDrinkDiscountStrategy(),
            new Domain.Discounts.SandwichSideDiscountStrategy(),
        };
        var order = Order.Create();
        order.AddItem(MakeSandwich(), strategies);
        order.AddItem(MakeSide(), strategies);
        order.AddItem(MakeDrink(), strategies);

        order.Subtotal.Should().Be(9.50m);
        order.DiscountRate.Should().Be(0.20m);
        order.Discount.Should().Be(1.90m);
        order.Total.Should().Be(7.60m);
    }
}
