namespace GoodHamburger.Web.Models;

public enum Category
{
    Sandwich,
    Side,
    Drink
}

public static class CategoryExtensions
{
    public static string Label(this Category c) => c switch
    {
        Category.Sandwich => "Sanduíche",
        Category.Side => "Acompanhamento",
        Category.Drink => "Bebida",
        _ => c.ToString()
    };
}
