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

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

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

app.UseHttpsRedirection();
app.MapControllers();
app.Run();

public partial class Program { }
