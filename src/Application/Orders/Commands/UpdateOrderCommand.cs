namespace GoodHamburger.Application.Orders.Commands;

public record UpdateOrderCommand(Guid OrderId, IEnumerable<Guid> ProductIds);
