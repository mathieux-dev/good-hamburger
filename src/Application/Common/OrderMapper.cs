using GoodHamburger.Application.DTOs;
using GoodHamburger.Domain.Entities;

namespace GoodHamburger.Application.Common;

internal static class OrderMapper
{
    internal static OrderDto ToDto(Order order)
    {
        var discountLabel = order.DiscountRate switch
        {
            0.20m => "COMBO COMPLETO",
            0.15m => "SANDUÍCHE + BEBIDA",
            0.10m => "SANDUÍCHE + BATATA",
            _ => null
        };

        return new OrderDto(
            order.Id.ToString(),
            order.Customer,
            order.Status,
            order.Note,
            order.Items.Select(i => new OrderItemDto(
                i.ProductId.ToString(),
                i.Product.Name,
                i.UnitPrice,
                i.Product.Category.ToString())),
            order.Subtotal,
            order.DiscountRate,
            discountLabel,
            order.Discount,
            order.Total,
            order.CreatedAt.ToString("o"));
    }
}
