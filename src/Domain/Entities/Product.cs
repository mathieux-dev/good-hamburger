using GoodHamburger.Domain.Enums;

namespace GoodHamburger.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public ProductCategory Category { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Product() { Name = null!; }

    public Product(Guid id, string name, decimal price, ProductCategory category)
    {
        Id = id; Name = name; Price = price; Category = category; IsActive = true;
    }

    public static Product Create(string name, decimal price, ProductCategory category, string? imageUrl = null) => new()
    {
        Id = Guid.NewGuid(), Name = name, Price = price, Category = category,
        ImageUrl = imageUrl, IsActive = true
    };

    public void Update(string name, decimal price, string? imageUrl)
    {
        Name = name;
        Price = price;
        ImageUrl = imageUrl;
    }

    public void Deactivate() => IsActive = false;
    public void Activate()   => IsActive = true;
}
