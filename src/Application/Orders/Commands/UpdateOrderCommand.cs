namespace GoodHamburger.Application.Orders.Commands;

public record UpdateOrderCommand(Guid OrderId, string Customer, string Note, IEnumerable<Guid> Items);
