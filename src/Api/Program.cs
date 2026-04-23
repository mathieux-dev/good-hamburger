using GoodHamburger.Application.Menu.Handlers;
using GoodHamburger.Application.Orders.Handlers;
using GoodHamburger.Application.Products.Handlers;
using GoodHamburger.Domain.Discounts;
using GoodHamburger.Domain.Interfaces;
using GoodHamburger.Infrastructure.Persistence;
using GoodHamburger.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var connStr = builder.Configuration.GetConnectionString("Default")!;
// Render provides the connection string as a postgres:// URL; convert to Npgsql key-value format
if (connStr.StartsWith("postgres://") || connStr.StartsWith("postgresql://"))
{
    var uri = new Uri(connStr);
    var userInfo = uri.UserInfo.Split(':', 2);
    connStr = $"Host={uri.Host};Port={(uri.Port > 0 ? uri.Port : 5432)};" +
              $"Database={uri.AbsolutePath.TrimStart('/')};" +
              $"Username={userInfo[0]};Password={Uri.UnescapeDataString(userInfo[1])};" +
              "SSL Mode=Require;Trust Server Certificate=true";
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connStr));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddSingleton<IDiscountStrategy, FullComboDiscountStrategy>();
builder.Services.AddSingleton<IDiscountStrategy, SandwichDrinkDiscountStrategy>();
builder.Services.AddSingleton<IDiscountStrategy, SandwichSideDiscountStrategy>();

builder.Services.AddScoped<CreateOrderHandler>();
builder.Services.AddScoped<UpdateOrderHandler>();
builder.Services.AddScoped<UpdateStatusHandler>();
builder.Services.AddScoped<DeleteOrderHandler>();
builder.Services.AddScoped<GetOrderHandler>();
builder.Services.AddScoped<GetAllOrdersHandler>();
builder.Services.AddScoped<GetMenuHandler>();
builder.Services.AddScoped<CreateProductHandler>();
builder.Services.AddScoped<UpdateProductHandler>();
builder.Services.AddScoped<DeleteProductHandler>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.MapScalarApiReference(options =>
        options.WithOpenApiRoutePattern("/swagger/v1/swagger.json"));
}

if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.Run();

public partial class Program { }
