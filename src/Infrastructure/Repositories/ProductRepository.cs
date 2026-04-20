using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Interfaces;
using GoodHamburger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoodHamburger.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context) => _context = context;

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Products.FindAsync([id], ct);

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Products.ToListAsync(ct);
}
