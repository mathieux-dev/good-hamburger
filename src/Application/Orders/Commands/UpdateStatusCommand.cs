namespace GoodHamburger.Application.Orders.Commands;

public record UpdateStatusCommand(Guid OrderId, string Status);
