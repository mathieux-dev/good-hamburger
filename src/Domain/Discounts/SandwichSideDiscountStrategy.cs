using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Domain.Discounts;

public class SandwichSideDiscountStrategy : IDiscountStrategy
{
    public decimal Calculate(Order order)
    {
        if (order.HasCategory(ProductCategory.Sandwich) &&
            order.HasCategory(ProductCategory.Side) &&
            !order.HasCategory(ProductCategory.Drink))
            return order.Subtotal * 0.10m;

        return 0;
    }
}
