namespace GoodHamburger.Application.DTOs;

public record ProductDto(
    string Id,
    string Name,
    decimal Price,
    string Category,
    string Subtitle = "",
    string Description = "",
    string Placeholder = "burger",
    string? ImageUrl = null,
    bool IsActive = true);
