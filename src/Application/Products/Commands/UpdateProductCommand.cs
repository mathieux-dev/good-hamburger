namespace GoodHamburger.Application.Products.Commands;

public record UpdateProductCommand(Guid ProductId, string Name, decimal Price, string? ImageUrl);
