namespace GoodHamburger.Application.DTOs;

public record OrderItemDto(Guid ProductId, string ProductName, decimal UnitPrice, string Category);
