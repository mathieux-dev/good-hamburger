using GoodHamburger.Application.DTOs;
using GoodHamburger.Application.Products.Commands;
using GoodHamburger.Application.Products.Handlers;
using GoodHamburger.Domain.Common;
using GoodHamburger.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GoodHamburger.Application.Products.Handlers;

public class UpdateProductHandler
{
    private readonly IProductRepository _repo;
    private readonly ILogger<UpdateProductHandler> _logger;

    public UpdateProductHandler(IProductRepository repo, ILogger<UpdateProductHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<Result<ProductDto>> HandleAsync(UpdateProductCommand command, CancellationToken ct = default)
    {
        var product = await _repo.GetByIdAsync(command.ProductId, ct);
        if (product is null)
        {
            _logger.LogWarning("Product {ProductId} not found for update", command.ProductId);
            return Result<ProductDto>.Failure($"Produto '{command.ProductId}' não encontrado.");
        }

        if (string.IsNullOrWhiteSpace(command.Name))
            return Result<ProductDto>.Failure("Nome do produto é obrigatório.");

        if (command.Price <= 0)
            return Result<ProductDto>.Failure("Preço deve ser maior que zero.");

        product.Update(command.Name.Trim(), command.Price, command.Subtitle, command.Description, command.ImageUrl);
        await _repo.UpdateAsync(product, ct);

        _logger.LogInformation("Product {ProductId} '{Name}' updated", product.Id, product.Name);

        return Result<ProductDto>.Success(CreateProductHandler.ToDto(product));
    }
}
