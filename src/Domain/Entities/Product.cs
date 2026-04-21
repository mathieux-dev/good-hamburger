using GoodHamburger.Domain.Enums;

namespace GoodHamburger.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public ProductCategory Category { get; private set; }
    public string Subtitle { get; private set; }
    public string Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Product() { Name = null!; Subtitle = ""; Description = ""; }

    public Product(Guid id, string name, decimal price, ProductCategory category,
                   string subtitle = "", string description = "")
    {
        Id = id; Name = name; Price = price; Category = category;
        Subtitle = subtitle; Description = description; IsActive = true;
    }

    public static Product Create(string name, decimal price, ProductCategory category,
                                 string subtitle, string description, string? imageUrl = null) => new()
    {
        Id = Guid.NewGuid(), Name = name, Price = price, Category = category,
        Subtitle = subtitle, Description = description,
        ImageUrl = imageUrl, IsActive = true
    };

    public void Update(string name, decimal price, string subtitle, string description, string? imageUrl)
    {
        Name = name;
        Price = price;
        Subtitle = subtitle;
        Description = description;
        ImageUrl = imageUrl;
    }

    public void Deactivate() => IsActive = false;
    public void Activate()   => IsActive = true;
}
