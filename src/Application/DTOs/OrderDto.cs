namespace GoodHamburger.Application.DTOs;

public record OrderDto(
    Guid Id,
    IEnumerable<OrderItemDto> Items,
    decimal Subtotal,
    decimal Discount,
    decimal Total,
    DateTime CreatedAt);
