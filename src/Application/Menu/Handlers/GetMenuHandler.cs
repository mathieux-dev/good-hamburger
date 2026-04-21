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
        return products
            .Where(p => p.IsActive)
            .Select(p =>
            {
                var (subtitle, description, placeholder) = GetMeta(p.Name);
                return new ProductDto(p.Id.ToString(), p.Name, p.Price, p.Category.ToString(), subtitle, description, placeholder, p.ImageUrl);
            });
    }

    private static (string Subtitle, string Description, string Placeholder) GetMeta(string name) => name switch
    {
        "X Burger"     => ("pão + burger + queijo", "O original.", "burger"),
        "X Egg"        => ("pão + burger + ovo", "Gema escorrendo.", "egg"),
        "X Bacon"      => ("pão + burger + bacon", "Favorito da casa.", "bacon"),
        "Batata Frita" => ("porção crocante", "Corte rústico, sal grosso.", "fries"),
        "Refrigerante" => ("lata 350ml", "Gelado.", "soda"),
        _              => ("", "", "burger"),
    };
}
