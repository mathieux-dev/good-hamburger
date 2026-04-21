using FluentAssertions;
using GoodHamburger.Application.Menu.Handlers;
using GoodHamburger.Application.Menu.Queries;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;
using GoodHamburger.Domain.Interfaces;
using NSubstitute;

namespace GoodHamburger.Tests.Unit.Application.Menu;

public class GetMenuHandlerTests
{
    private readonly IProductRepository _repo = Substitute.For<IProductRepository>();

    private GetMenuHandler Handler() => new(_repo);

    [Fact]
    public async Task HandleAsync_ShouldReturnOnlyActiveProducts()
    {
        var active   = Product.Create("X Burger",    5.00m, ProductCategory.Sandwich, "", "");
        var inactive = Product.Create("Descontinuado",1.00m, ProductCategory.Side,    "", "");
        inactive.Deactivate();
        _repo.GetAllAsync().Returns([active, inactive]);

        var result = await Handler().HandleAsync(new GetMenuQuery());

        result.Should().HaveCount(1);
        result.Single().Name.Should().Be("X Burger");
    }

    [Fact]
    public async Task HandleAsync_ShouldMapPlaceholderByCategory()
    {
        var sandwich = Product.Create("X Burger",    5.00m, ProductCategory.Sandwich, "", "");
        var side     = Product.Create("Batata Frita",2.00m, ProductCategory.Side,     "", "");
        var drink    = Product.Create("Refrigerante",2.50m, ProductCategory.Drink,    "", "");
        _repo.GetAllAsync().Returns([sandwich, side, drink]);

        var result = (await Handler().HandleAsync(new GetMenuQuery())).ToList();

        result.First(p => p.Category == "Sandwich").Placeholder.Should().Be("burger");
        result.First(p => p.Category == "Side").Placeholder.Should().Be("fries");
        result.First(p => p.Category == "Drink").Placeholder.Should().Be("soda");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnEmpty_WhenNoActiveProducts()
    {
        _repo.GetAllAsync().Returns([]);

        var result = await Handler().HandleAsync(new GetMenuQuery());

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ShouldIncludeImageUrl_WhenSet()
    {
        var product = Product.Create("Onion Ring", 3.00m, ProductCategory.Side, "", "", "http://img.png");
        _repo.GetAllAsync().Returns([product]);

        var result = await Handler().HandleAsync(new GetMenuQuery());

        result.Single().ImageUrl.Should().Be("http://img.png");
    }
}
