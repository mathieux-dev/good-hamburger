using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;
using GoodHamburger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoodHamburger.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context) => _context = context;

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .ToListAsync(ct);

    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        await _context.Orders.AddAsync(order, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Order order, CancellationToken ct = default)
    {
        var currentIds = order.Items.Select(i => i.Id).ToHashSet();

        var existingItems = await _context.OrderItems
            .Where(i => i.OrderId == order.Id)
            .ToListAsync(ct);

        var toRemove = existingItems.Where(i => !currentIds.Contains(i.Id)).ToList();
        _context.OrderItems.RemoveRange(toRemove);

        var existingIds = existingItems.Select(i => i.Id).ToHashSet();
        var toAdd = order.Items.Where(i => !existingIds.Contains(i.Id)).ToList();
        await _context.OrderItems.AddRangeAsync(toAdd, ct);

        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Order order, CancellationToken ct = default)
    {
        order.SoftDelete();
        _context.Orders.Update(order);
        await _context.SaveChangesAsync(ct);
    }
}
