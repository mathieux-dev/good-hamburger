namespace GoodHamburger.Application.Products.Commands;

public record CreateProductCommand(string Name, decimal Price, string Category, string? ImageUrl);
