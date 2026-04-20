using GoodHamburger.Application.DTOs;
using GoodHamburger.Application.Menu.Queries;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Application.Menu.Handlers;

public class GetMenuHandler
{
    private readonly IProductRepository _productRepository;

    public GetMenuHandler(IProductRepository productRepository) =>
        _productRepository = productRepository;

    public async Task<IEnumerable<ProductDto>> HandleAsync(GetMenuQuery query, CancellationToken ct = default)
    {
        var products = await _productRepository.GetAllAsync(ct);
        return products.Select(p => new ProductDto(p.Id, p.Name, p.Price, p.Category.ToString()));
    }
}
