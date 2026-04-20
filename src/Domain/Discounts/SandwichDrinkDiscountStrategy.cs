using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Domain.Discounts;

public class SandwichDrinkDiscountStrategy : IDiscountStrategy
{
    public decimal Calculate(Order order)
    {
        if (order.HasCategory(ProductCategory.Sandwich) &&
            order.HasCategory(ProductCategory.Drink) &&
            !order.HasCategory(ProductCategory.Side))
            return order.Subtotal * 0.15m;

        return 0;
    }
}
