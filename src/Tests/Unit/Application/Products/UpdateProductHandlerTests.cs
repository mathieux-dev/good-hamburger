using FluentAssertions;
using GoodHamburger.Application.Products.Commands;
using GoodHamburger.Application.Products.Handlers;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;
using GoodHamburger.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace GoodHamburger.Tests.Unit.Application.Products;

public class UpdateProductHandlerTests
{
    private readonly IProductRepository _repo = Substitute.For<IProductRepository>();

    private UpdateProductHandler Handler() => new(_repo, NullLogger<UpdateProductHandler>.Instance);

    private static Product ExistingProduct() =>
        Product.Create("X Burger", 5.00m, ProductCategory.Sandwich, "sub", "desc");

    private static UpdateProductCommand ValidCommand(Product product, string name = "Novo Nome", decimal price = 9.99m) =>
        new(product.Id, name, price, "nova sub", "nova desc", null);

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenProductNotFound()
    {
        var id = Guid.NewGuid();
        _repo.GetByIdAsync(id).Returns((Product?)null);

        var result = await Handler().HandleAsync(new UpdateProductCommand(id, "Nome", 5m, "", "", null));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain(id.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_ShouldFail_WhenNameBlank(string name)
    {
        var product = ExistingProduct();
        _repo.GetByIdAsync(product.Id).Returns(product);

        var result = await Handler().HandleAsync(new UpdateProductCommand(product.Id, name, 5m, "", "", null));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Nome");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task HandleAsync_ShouldFail_WhenPriceNotPositive(decimal price)
    {
        var product = ExistingProduct();
        _repo.GetByIdAsync(product.Id).Returns(product);

        var result = await Handler().HandleAsync(new UpdateProductCommand(product.Id, "Nome", price, "", "", null));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Preço");
    }

    [Fact]
    public async Task HandleAsync_ShouldUpdate_AndPersist()
    {
        var product = ExistingProduct();
        _repo.GetByIdAsync(product.Id).Returns(product);

        var result = await Handler().HandleAsync(ValidCommand(product));

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Novo Nome");
        result.Value.Price.Should().Be(9.99m);
        await _repo.Received(1).UpdateAsync(product);
    }

    [Fact]
    public async Task HandleAsync_ShouldTrimName()
    {
        var product = ExistingProduct();
        _repo.GetByIdAsync(product.Id).Returns(product);

        var result = await Handler().HandleAsync(new UpdateProductCommand(product.Id, "  Trimmed  ", 5m, "", "", null));

        result.Value.Name.Should().Be("Trimmed");
    }
}
