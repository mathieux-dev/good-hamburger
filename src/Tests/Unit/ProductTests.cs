using FluentAssertions;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;

namespace GoodHamburger.Tests.Unit;

public class ProductTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var product = Product.Create("X Burger", 5.00m, ProductCategory.Sandwich, "pão + burger", "O original.", "http://img.png");

        product.Id.Should().NotBeEmpty();
        product.Name.Should().Be("X Burger");
        product.Price.Should().Be(5.00m);
        product.Category.Should().Be(ProductCategory.Sandwich);
        product.Subtitle.Should().Be("pão + burger");
        product.Description.Should().Be("O original.");
        product.ImageUrl.Should().Be("http://img.png");
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldBeActive_ByDefault()
    {
        var product = Product.Create("X Egg", 4.50m, ProductCategory.Sandwich, "", "");

        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Update_ShouldChangeProperties()
    {
        var product = Product.Create("Old Name", 1.00m, ProductCategory.Side, "old sub", "old desc");

        product.Update("New Name", 9.99m, "new sub", "new desc", "http://new.png");

        product.Name.Should().Be("New Name");
        product.Price.Should().Be(9.99m);
        product.Subtitle.Should().Be("new sub");
        product.Description.Should().Be("new desc");
        product.ImageUrl.Should().Be("http://new.png");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var product = Product.Create("X Bacon", 7.00m, ProductCategory.Sandwich, "", "");

        product.Deactivate();

        product.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveTrue()
    {
        var product = Product.Create("X Bacon", 7.00m, ProductCategory.Sandwich, "", "");
        product.Deactivate();

        product.Activate();

        product.IsActive.Should().BeTrue();
    }
}
