using GoodHamburger.Application.DTOs;
using GoodHamburger.Application.Products.Commands;
using GoodHamburger.Domain.Common;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Application.Products.Handlers;

public class CreateProductHandler
{
    private readonly IProductRepository _repo;

    public CreateProductHandler(IProductRepository repo) => _repo = repo;

    public async Task<Result<ProductDto>> HandleAsync(CreateProductCommand command, CancellationToken ct = default)
    {
        if (!Enum.TryParse<ProductCategory>(command.Category, true, out var category))
            return Result<ProductDto>.Failure($"Categoria inválida: '{command.Category}'.");

        if (string.IsNullOrWhiteSpace(command.Name))
            return Result<ProductDto>.Failure("Nome do produto é obrigatório.");

        if (command.Price <= 0)
            return Result<ProductDto>.Failure("Preço deve ser maior que zero.");

        var product = Product.Create(command.Name.Trim(), command.Price, category,
            command.Subtitle, command.Description, command.ImageUrl);
        await _repo.AddAsync(product, ct);

        return Result<ProductDto>.Success(ToDto(product));
    }

    internal static ProductDto ToDto(Product p) =>
        new(p.Id.ToString(), p.Name, p.Price, p.Category.ToString(),
            p.Subtitle, p.Description, ImageUrl: p.ImageUrl, IsActive: p.IsActive);
}
