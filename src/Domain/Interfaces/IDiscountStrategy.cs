using GoodHamburger.Domain.Entities;

namespace GoodHamburger.Domain.Interfaces;

public interface IDiscountStrategy
{
    decimal Calculate(Order order);
}
