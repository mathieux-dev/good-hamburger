namespace GoodHamburger.Application.DTOs;

public record OrderDto(
    string Id,
    string Customer,
    string Status,
    string Note,
    IEnumerable<OrderItemDto> Items,
    decimal Subtotal,
    decimal DiscountRate,
    string? DiscountLabel,
    decimal DiscountAmount,
    decimal Total,
    string CreatedAt);
