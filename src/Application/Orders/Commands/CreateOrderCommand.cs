namespace GoodHamburger.Application.Orders.Commands;

public record CreateOrderCommand(IEnumerable<Guid> ProductIds);
