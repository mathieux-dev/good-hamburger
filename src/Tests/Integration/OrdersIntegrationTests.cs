using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GoodHamburger.Application.DTOs;
using GoodHamburger.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace GoodHamburger.Tests.Integration;

public class OrdersIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor is not null) services.Remove(descriptor);

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseNpgsql(_postgres.GetConnectionString()));
                });
            });

        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task POST_Orders_ShouldCreateOrder_WithFullComboDiscount()
    {
        var menu = await _client.GetFromJsonAsync<IEnumerable<ProductDto>>("/menu");
        var sandwich = menu!.First(p => p.Category == "Sandwich");
        var side     = menu!.First(p => p.Category == "Side");
        var drink    = menu!.First(p => p.Category == "Drink");

        var response = await _client.PostAsJsonAsync("/orders", new
        {
            productIds = new[] { sandwich.Id, side.Id, drink.Id }
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        order!.Items.Should().HaveCount(3);
        order.Discount.Should().Be(order.Subtotal * 0.20m);
        order.Total.Should().Be(order.Subtotal - order.Discount);
    }

    [Fact]
    public async Task POST_Orders_ShouldReturn400_WhenDuplicateCategory()
    {
        var menu = await _client.GetFromJsonAsync<IEnumerable<ProductDto>>("/menu");
        var sandwiches = menu!.Where(p => p.Category == "Sandwich").ToList();

        var response = await _client.PostAsJsonAsync("/orders", new
        {
            productIds = new[] { sandwiches[0].Id, sandwiches[1].Id }
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_Orders_ShouldReturnCreatedOrder()
    {
        var menu = await _client.GetFromJsonAsync<IEnumerable<ProductDto>>("/menu");
        var sandwich = menu!.First(p => p.Category == "Sandwich");

        var created = await _client.PostAsJsonAsync("/orders", new
        {
            productIds = new[] { sandwich.Id }
        });
        var order = await created.Content.ReadFromJsonAsync<OrderDto>();

        var response = await _client.GetAsync($"/orders/{order!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DELETE_Orders_ShouldReturn204()
    {
        var menu = await _client.GetFromJsonAsync<IEnumerable<ProductDto>>("/menu");
        var sandwich = menu!.First(p => p.Category == "Sandwich");

        var created = await _client.PostAsJsonAsync("/orders", new
        {
            productIds = new[] { sandwich.Id }
        });
        var order = await created.Content.ReadFromJsonAsync<OrderDto>();

        var response = await _client.DeleteAsync($"/orders/{order!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
