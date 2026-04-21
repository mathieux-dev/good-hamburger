using GoodHamburger.Application.Products.Commands;
using GoodHamburger.Domain.Common;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Application.Products.Handlers;

public class DeleteProductHandler
{
    private readonly IProductRepository _repo;

    public DeleteProductHandler(IProductRepository repo) => _repo = repo;

    public async Task<Result> HandleAsync(DeleteProductCommand command, CancellationToken ct = default)
    {
        var product = await _repo.GetByIdAsync(command.ProductId, ct);
        if (product is null)
            return Result.Failure($"Produto '{command.ProductId}' não encontrado.");

        await _repo.DeleteAsync(product, ct);
        return Result.Success();
    }
}
