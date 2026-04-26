using FluentAssertions;
using GoodHamburger.Application.Products.Commands;
using GoodHamburger.Application.Products.Handlers;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;
using GoodHamburger.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace GoodHamburger.Tests.Unit.Application.Products;

public class DeleteProductHandlerTests
{
    private readonly IProductRepository _repo = Substitute.For<IProductRepository>();

    private DeleteProductHandler Handler() => new(_repo, NullLogger<DeleteProductHandler>.Instance);

    [Fact]
    public async Task HandleAsync_ShouldFail_WhenProductNotFound()
    {
        var id = Guid.NewGuid();
        _repo.GetByIdAsync(id).Returns((Product?)null);

        var result = await Handler().HandleAsync(new DeleteProductCommand(id));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain(id.ToString());
        await _repo.DidNotReceive().DeleteAsync(Arg.Any<Product>());
    }

    [Fact]
    public async Task HandleAsync_ShouldDelete_WhenProductExists()
    {
        var product = Product.Create("X Bacon", 7.00m, ProductCategory.Sandwich, "", "");
        _repo.GetByIdAsync(product.Id).Returns(product);

        var result = await Handler().HandleAsync(new DeleteProductCommand(product.Id));

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).DeleteAsync(product);
    }
}
