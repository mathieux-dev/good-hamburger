using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoodHamburger.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).IsRequired().HasMaxLength(100);
            e.Property(p => p.Price).HasPrecision(10, 2);
            e.Property(p => p.Category).HasConversion<string>();
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.Customer).IsRequired().HasMaxLength(100);
            e.Property(o => o.Note).HasMaxLength(500);
            e.Property(o => o.Status).IsRequired().HasMaxLength(20);
            e.Property(o => o.Subtotal).HasPrecision(10, 2);
            e.Property(o => o.Discount).HasPrecision(10, 2);
            e.Property(o => o.DiscountRate).HasPrecision(5, 4);
            e.Property(o => o.Total).HasPrecision(10, 2);
            e.HasMany(o => o.Items)
             .WithOne()
             .HasForeignKey(i => i.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.UnitPrice).HasPrecision(10, 2);
            e.HasOne(i => i.Product)
             .WithMany()
             .HasForeignKey(i => i.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>().HasData(
            new Product(Guid.Parse("11111111-0000-0000-0000-000000000001"), "X Burger",    5.00m, ProductCategory.Sandwich),
            new Product(Guid.Parse("11111111-0000-0000-0000-000000000002"), "X Egg",       4.50m, ProductCategory.Sandwich),
            new Product(Guid.Parse("11111111-0000-0000-0000-000000000003"), "X Bacon",     7.00m, ProductCategory.Sandwich),
            new Product(Guid.Parse("11111111-0000-0000-0000-000000000004"), "Batata Frita",2.00m, ProductCategory.Side),
            new Product(Guid.Parse("11111111-0000-0000-0000-000000000005"), "Refrigerante",2.50m, ProductCategory.Drink)
        );
    }
}
