namespace GoodHamburger.Web.Models;

public class OrderDto
{
    public string Id { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public string Status { get; set; } = "preparando";
    public string CreatedAt { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public List<MenuItemDto> Items { get; set; } = new();

    public decimal Subtotal { get; set; }
    public decimal DiscountRate { get; set; }
    public string? DiscountLabel { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
}

public class OrderRequest
{
    public string Customer { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public List<string> Items { get; set; } = new();
}
