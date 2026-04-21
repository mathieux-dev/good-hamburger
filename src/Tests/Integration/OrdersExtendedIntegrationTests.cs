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

public class OrdersExtendedIntegrationTests : IAsyncLifetime
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

    private async Task<(string sandwichId, string sideId, string drinkId)> GetMenuIdsAsync()
    {
        var menu = await _client.GetFromJsonAsync<IEnumerable<ProductDto>>("/api/menu");
        var items = menu!.ToList();
        return (
            items.First(p => p.Category == "Sandwich").Id,
            items.First(p => p.Category == "Side").Id,
            items.First(p => p.Category == "Drink").Id
        );
    }

    private async Task<OrderDto> CreateOrderAsync(params string[] productIds)
    {
        var response = await _client.PostAsJsonAsync("/api/orders", new
        {
            customer = "Test",
            note = "",
            serviceType = "Salão",
            items = productIds
        });
        return (await response.Content.ReadFromJsonAsync<OrderDto>())!;
    }

    [Fact]
    public async Task GET_Orders_ById_ShouldReturn404_WhenNotFound()
    {
        var response = await _client.GetAsync($"/api/orders/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PATCH_Orders_Status_ShouldReturn204_AndChangeStatus()
    {
        var (sandwichId, _, _) = await GetMenuIdsAsync();
        var order = await CreateOrderAsync(sandwichId);

        var response = await _client.PatchAsJsonAsync($"/api/orders/{order.Id}/status", new { status = "pronto" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var fetched = await _client.GetFromJsonAsync<OrderDto>($"/api/orders/{order.Id}");
        fetched!.Status.Should().Be("pronto");
    }

    [Fact]
    public async Task PATCH_Orders_Status_ShouldReturn404_WhenNotFound()
    {
        var response = await _client.PatchAsJsonAsync($"/api/orders/{Guid.NewGuid()}/status", new { status = "pronto" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PUT_Orders_ShouldUpdateItems_AndRecalculateDiscount()
    {
        var (sandwichId, sideId, drinkId) = await GetMenuIdsAsync();
        var order = await CreateOrderAsync(sandwichId);

        var response = await _client.PutAsJsonAsync($"/api/orders/{order.Id}", new
        {
            customer = "Mesa 2",
            note = "sem sal",
            serviceType = "Delivery",
            items = new[] { sandwichId, sideId, drinkId }
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<OrderDto>();
        updated!.Items.Should().HaveCount(3);
        updated.Customer.Should().Be("Mesa 2");
        updated.ServiceType.Should().Be("Delivery");
        updated.DiscountRate.Should().Be(0.20m);
    }

    [Fact]
    public async Task PUT_Orders_ShouldReturn404_WhenNotFound()
    {
        var (sandwichId, _, _) = await GetMenuIdsAsync();

        var response = await _client.PutAsJsonAsync($"/api/orders/{Guid.NewGuid()}", new
        {
            customer = "Test",
            note = "",
            serviceType = "Salão",
            items = new[] { sandwichId }
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PUT_Orders_ShouldReturn400_WhenDuplicateCategory()
    {
        var (sandwichId, _, _) = await GetMenuIdsAsync();
        var order = await CreateOrderAsync(sandwichId);

        var response = await _client.PutAsJsonAsync($"/api/orders/{order.Id}", new
        {
            customer = "Test",
            note = "",
            serviceType = "Salão",
            items = new[] { sandwichId, sandwichId }
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_Orders_ShouldPersistServiceType()
    {
        var (_, _, drinkId) = await GetMenuIdsAsync();

        var response = await _client.PostAsJsonAsync("/api/orders", new
        {
            customer = "Drive",
            note = "",
            serviceType = "Delivery",
            items = new[] { drinkId }
        });

        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        order!.ServiceType.Should().Be("Delivery");

        var fetched = await _client.GetFromJsonAsync<OrderDto>($"/api/orders/{order.Id}");
        fetched!.ServiceType.Should().Be("Delivery");
    }

    [Fact]
    public async Task DELETE_Orders_ShouldNotAppearInList_AfterDelete()
    {
        var (sandwichId, _, _) = await GetMenuIdsAsync();
        var order = await CreateOrderAsync(sandwichId);

        await _client.DeleteAsync($"/api/orders/{order.Id}");

        var all = await _client.GetFromJsonAsync<IEnumerable<OrderDto>>("/api/orders");
        all!.Should().NotContain(o => o.Id == order.Id);
    }

    [Fact]
    public async Task POST_Orders_ShouldApply15PercentDiscount_ForSandwichDrink()
    {
        var (sandwichId, _, drinkId) = await GetMenuIdsAsync();
        var order = await CreateOrderAsync(sandwichId, drinkId);

        order.DiscountRate.Should().Be(0.15m);
        order.DiscountLabel.Should().Be("SANDUÍCHE + BEBIDA");
    }

    [Fact]
    public async Task POST_Orders_ShouldApply10PercentDiscount_ForSandwichSide()
    {
        var (sandwichId, sideId, _) = await GetMenuIdsAsync();
        var order = await CreateOrderAsync(sandwichId, sideId);

        order.DiscountRate.Should().Be(0.10m);
        order.DiscountLabel.Should().Be("SANDUÍCHE + BATATA");
    }
}
