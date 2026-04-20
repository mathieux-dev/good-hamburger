namespace GoodHamburger.Web.Models;

public record MenuItemDto(
    string Id,
    string Name,
    decimal Price,
    Category Category,
    string Subtitle = "",
    string Description = "",
    string Placeholder = "burger"
);
