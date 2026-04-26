using GoodHamburger.Application.Products.Commands;
using GoodHamburger.Domain.Common;
using GoodHamburger.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GoodHamburger.Application.Products.Handlers;

public class DeleteProductHandler
{
    private readonly IProductRepository _repo;
    private readonly ILogger<DeleteProductHandler> _logger;

    public DeleteProductHandler(IProductRepository repo, ILogger<DeleteProductHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(DeleteProductCommand command, CancellationToken ct = default)
    {
        var product = await _repo.GetByIdAsync(command.ProductId, ct);
        if (product is null)
        {
            _logger.LogWarning("Product {ProductId} not found for deletion", command.ProductId);
            return Result.Failure($"Produto '{command.ProductId}' não encontrado.");
        }

        await _repo.DeleteAsync(product, ct);
        _logger.LogInformation("Product {ProductId} '{Name}' deleted", product.Id, product.Name);

        return Result.Success();
    }
}
