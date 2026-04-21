using FluentAssertions;
using GoodHamburger.Application.Products.Commands;
using GoodHamburger.Application.Products.Handlers;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;
using NSubstitute;

namespace GoodHamburger.Tests.Unit.Application.Products;

public class CreateProductHandlerTests
{
    private readonly IProductRepository _repo = Substitute.For<IProductRepository>();

    private CreateProductHandler Handler() => new(_repo);

    private static CreateProductCommand ValidCommand(string name = "X Burger", decimal price = 5.00m,
        string category = "Sandwich") =>
        new(name, price, category, "pão + burger", "O original.", null);

    [Fact]
    public async Task HandleAsync_ShouldCreateProduct_WithValidData()
    {
        var result = await Handler().HandleAsync(ValidCommand());

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("X Burger");
        result.Value.Price.Should().Be(5.00m);
        result.Value.Category.Should().Be("Sandwich");
        await _repo.Received(1).AddAsync(Arg.Any<Product>());
    }

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenCategoryInvalid()
    {
        var result = await Handler().HandleAsync(ValidCommand(category: "Porcaria"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Porcaria");
        await _repo.DidNotReceive().AddAsync(Arg.Any<Product>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_ShouldFail_WhenNameBlank(string name)
    {
        var result = await Handler().HandleAsync(ValidCommand(name: name));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Nome");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public async Task HandleAsync_ShouldFail_WhenPriceNotPositive(decimal price)
    {
        var result = await Handler().HandleAsync(ValidCommand(price: price));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Preço");
    }

    [Theory]
    [InlineData("Sandwich")]
    [InlineData("sandwich")]
    [InlineData("Side")]
    [InlineData("Drink")]
    public async Task HandleAsync_ShouldAcceptAllCategories(string category)
    {
        var result = await Handler().HandleAsync(ValidCommand(category: category));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_ShouldTrimName()
    {
        var result = await Handler().HandleAsync(ValidCommand(name: "  X Burger  "));

        result.Value.Name.Should().Be("X Burger");
    }
}
