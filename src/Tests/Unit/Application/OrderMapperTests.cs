using FluentAssertions;
using GoodHamburger.Application.Common;
using GoodHamburger.Domain.Discounts;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Tests.Unit.Application;

public class OrderMapperTests
{
    private static readonly IDiscountStrategy[] AllStrategies =
    [
        new FullComboDiscountStrategy(),
        new SandwichDrinkDiscountStrategy(),
        new SandwichSideDiscountStrategy(),
    ];

    private static Product Sandwich() => new(Guid.NewGuid(), "X Burger",    5.00m, ProductCategory.Sandwich);
    private static Product Side()     => new(Guid.NewGuid(), "Batata Frita",2.00m, ProductCategory.Side);
    private static Product Drink()    => new(Guid.NewGuid(), "Refrigerante",2.50m, ProductCategory.Drink);

    [Fact]
    public void ToDto_ShouldMapScalarProperties()
    {
        var order = Order.Create("Mesa 1", "sem cebola", "Delivery");

        var dto = OrderMapper.ToDto(order);

        dto.Customer.Should().Be("Mesa 1");
        dto.Note.Should().Be("sem cebola");
        dto.ServiceType.Should().Be("Delivery");
        dto.Status.Should().Be("preparando");
        dto.Id.Should().Be(order.Id.ToString());
    }

    [Theory]
    [InlineData(0.20, "COMBO COMPLETO")]
    [InlineData(0.15, "SANDUÍCHE + BEBIDA")]
    [InlineData(0.10, "SANDUÍCHE + BATATA")]
    public void ToDto_ShouldMapDiscountLabel(double rate, string expectedLabel)
    {
        var order = Order.Create();
        if (rate == 0.20)
        {
            order.AddItem(Sandwich(), AllStrategies);
            order.AddItem(Side(),     AllStrategies);
            order.AddItem(Drink(),    AllStrategies);
        }
        else if (rate == 0.15)
        {
            order.AddItem(Sandwich(), AllStrategies);
            order.AddItem(Drink(),    AllStrategies);
        }
        else
        {
            order.AddItem(Sandwich(), AllStrategies);
            order.AddItem(Side(),     AllStrategies);
        }

        var dto = OrderMapper.ToDto(order);

        dto.DiscountLabel.Should().Be(expectedLabel);
    }

    [Fact]
    public void ToDto_ShouldHaveNullDiscountLabel_WhenNoDiscount()
    {
        var order = Order.Create();
        order.AddItem(Sandwich(), AllStrategies);

        var dto = OrderMapper.ToDto(order);

        dto.DiscountLabel.Should().BeNull();
        dto.DiscountAmount.Should().Be(0);
    }

    [Fact]
    public void ToDto_ShouldMapItems_WithCorrectPricesAndCategories()
    {
        var sandwich = Sandwich();
        var side = Side();
        var order = Order.Create();
        order.AddItem(sandwich, []);
        order.AddItem(side,     []);

        var dto = OrderMapper.ToDto(order);

        dto.Items.Should().HaveCount(2);
        dto.Items.Should().Contain(i => i.Name == "X Burger"     && i.Price == 5.00m);
        dto.Items.Should().Contain(i => i.Name == "Batata Frita" && i.Price == 2.00m);
    }

    [Fact]
    public void ToDto_ShouldCalculateTotalsCorrectly_ForFullCombo()
    {
        var order = Order.Create();
        order.AddItem(Sandwich(), AllStrategies);
        order.AddItem(Side(),     AllStrategies);
        order.AddItem(Drink(),    AllStrategies);

        var dto = OrderMapper.ToDto(order);

        dto.Subtotal.Should().Be(9.50m);
        dto.DiscountRate.Should().Be(0.20m);
        dto.DiscountAmount.Should().Be(1.90m);
        dto.Total.Should().Be(7.60m);
    }
}
