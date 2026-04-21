using GoodHamburger.Application.DTOs;
using GoodHamburger.Application.Products.Commands;
using GoodHamburger.Application.Products.Handlers;
using GoodHamburger.Domain.Common;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Application.Products.Handlers;

public class UpdateProductHandler
{
    private readonly IProductRepository _repo;

    public UpdateProductHandler(IProductRepository repo) => _repo = repo;

    public async Task<Result<ProductDto>> HandleAsync(UpdateProductCommand command, CancellationToken ct = default)
    {
        var product = await _repo.GetByIdAsync(command.ProductId, ct);
        if (product is null)
            return Result<ProductDto>.Failure($"Produto '{command.ProductId}' não encontrado.");

        if (string.IsNullOrWhiteSpace(command.Name))
            return Result<ProductDto>.Failure("Nome do produto é obrigatório.");

        if (command.Price <= 0)
            return Result<ProductDto>.Failure("Preço deve ser maior que zero.");

        product.Update(command.Name.Trim(), command.Price, command.ImageUrl);
        await _repo.UpdateAsync(product, ct);

        return Result<ProductDto>.Success(CreateProductHandler.ToDto(product));
    }
}
