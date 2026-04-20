using GoodHamburger.Application.DTOs;
using GoodHamburger.Domain.Entities;

namespace GoodHamburger.Application.Common;

internal static class OrderMapper
{
    internal static OrderDto ToDto(Order order) => new(
        order.Id,
        order.Items.Select(i => new OrderItemDto(
            i.ProductId,
            i.Product.Name,
            i.UnitPrice,
            i.Product.Category.ToString())),
        order.Subtotal,
        order.Discount,
        order.Total,
        order.CreatedAt);
}
