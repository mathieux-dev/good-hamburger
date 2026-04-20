namespace GoodHamburger.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; }
    public decimal UnitPrice { get; private set; }

    private OrderItem() { Product = null!; }

    public OrderItem(Guid orderId, Product product)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        ProductId = product.Id;
        Product = product;
        UnitPrice = product.Price;
    }
}
