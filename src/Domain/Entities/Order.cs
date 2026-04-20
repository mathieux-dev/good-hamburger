using GoodHamburger.Domain.Common;
using GoodHamburger.Domain.Enums;
using GoodHamburger.Domain.Interfaces;

namespace GoodHamburger.Domain.Entities;

public class Order
{
    private readonly List<OrderItem> _items = new();

    public Guid Id { get; private set; }
    public string Customer { get; private set; } = "Balcão";
    public string Note { get; private set; } = string.Empty;
    public string Status { get; private set; } = "preparando";
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public decimal Subtotal { get; private set; }
    public decimal Discount { get; private set; }
    public decimal DiscountRate { get; private set; }
    public decimal Total { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Order() { }

    public static Order Create(string customer = "Balcão", string note = "") => new()
    {
        Id = Guid.NewGuid(),
        CreatedAt = DateTime.UtcNow,
        Customer = string.IsNullOrWhiteSpace(customer) ? "Balcão" : customer,
        Note = note,
        Status = "preparando"
    };

    public void Update(string customer, string note)
    {
        Customer = string.IsNullOrWhiteSpace(customer) ? "Balcão" : customer;
        Note = note;
    }

    public Result AddItem(Product product, IEnumerable<IDiscountStrategy> strategies)
    {
        if (_items.Any(i => i.Product.Category == product.Category))
            return Result.Failure($"Order already contains an item of category '{product.Category}'.");

        _items.Add(new OrderItem(Id, product));
        RecalculateTotals(strategies);
        return Result.Success();
    }

    public Result RemoveItem(Guid productId, IEnumerable<IDiscountStrategy> strategies)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
            return Result.Failure("Item not found in order.");

        _items.Remove(item);
        RecalculateTotals(strategies);
        return Result.Success();
    }

    public Result SetItems(IEnumerable<Product> products, IEnumerable<IDiscountStrategy> strategies)
    {
        var newItems = new List<OrderItem>();

        foreach (var product in products)
        {
            if (newItems.Any(i => i.Product.Category == product.Category))
                return Result.Failure($"Order already contains an item of category '{product.Category}'.");

            newItems.Add(new OrderItem(Id, product));
        }

        _items.Clear();
        _items.AddRange(newItems);
        RecalculateTotals(strategies);
        return Result.Success();
    }

    private void RecalculateTotals(IEnumerable<IDiscountStrategy> strategies)
    {
        Subtotal = _items.Sum(i => i.UnitPrice);
        Discount = strategies
            .Select(s => s.Calculate(this))
            .FirstOrDefault(d => d > 0);
        DiscountRate = Subtotal > 0 ? Discount / Subtotal : 0;
        Total = Subtotal - Discount;
    }

    public bool HasCategory(ProductCategory category) =>
        _items.Any(i => i.Product.Category == category);
}
