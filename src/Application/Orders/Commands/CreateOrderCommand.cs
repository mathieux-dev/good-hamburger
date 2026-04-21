namespace GoodHamburger.Application.Orders.Commands;

public record CreateOrderCommand(string Customer, string Note, IEnumerable<Guid> Items, string ServiceType = "Salão");
