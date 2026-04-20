namespace GoodHamburger.Web.Models;

public record DiscountRule(decimal Rate, string Label, string Combo);

public static class DiscountCalculator
{
    public static DiscountRule? Calculate(IEnumerable<MenuItemDto> items)
    {
        var cats = items.Select(i => i.Category).Distinct().ToHashSet();
        var hasSand = cats.Contains(Category.Sandwich);
        var hasSide = cats.Contains(Category.Side);
        var hasDrink = cats.Contains(Category.Drink);

        if (hasSand && hasSide && hasDrink) return new(0.20m, "COMBO COMPLETO", "full");
        if (hasSand && hasDrink)            return new(0.15m, "SANDUÍCHE + BEBIDA", "sand-ref");
        if (hasSand && hasSide)             return new(0.10m, "SANDUÍCHE + BATATA", "sand-bat");
        return null;
    }

    public record Summary(decimal Subtotal, DiscountRule? Rule, decimal Discount, decimal Total);

    public static Summary Summarize(IEnumerable<MenuItemDto> items)
    {
        var list = items.ToList();
        var subtotal = list.Sum(i => i.Price);
        var rule = Calculate(list);
        var discount = subtotal * (rule?.Rate ?? 0);
        return new Summary(subtotal, rule, discount, subtotal - discount);
    }
}
