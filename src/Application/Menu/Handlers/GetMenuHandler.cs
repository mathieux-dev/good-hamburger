using GoodHamburger.Application.DTOs;
using GoodHamburger.Application.Menu.Queries;
using GoodHamburger.Domain.Enums;
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
        return products
            .Where(p => p.IsActive)
            .Select(p => new ProductDto(
                p.Id.ToString(), p.Name, p.Price, p.Category.ToString(),
                p.Subtitle, p.Description, PlaceholderFor(p.Category), p.ImageUrl));
    }

    private static string PlaceholderFor(ProductCategory cat) => cat switch
    {
        ProductCategory.Sandwich => "burger",
        ProductCategory.Side     => "fries",
        ProductCategory.Drink    => "soda",
        _                        => "burger",
    };
}
