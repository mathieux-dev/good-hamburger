using GoodHamburger.Domain.Enums;

namespace GoodHamburger.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public ProductCategory Category { get; private set; }

    private Product() { Name = null!; }

    public Product(Guid id, string name, decimal price, ProductCategory category)
    {
        Id = id;
        Name = name;
        Price = price;
        Category = category;
    }
}
