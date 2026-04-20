using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Domain.Discounts;

public class FullComboDiscountStrategy : IDiscountStrategy
{
    public decimal Calculate(Order order)
    {
        if (order.HasCategory(ProductCategory.Sandwich) &&
            order.HasCategory(ProductCategory.Side) &&
            order.HasCategory(ProductCategory.Drink))
            return order.Subtotal * 0.20m;

        return 0;
    }
}
