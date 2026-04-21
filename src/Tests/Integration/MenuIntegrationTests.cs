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

public class MenuIntegrationTests : IAsyncLifetime
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
    public async Task GET_Menu_ShouldReturn200_WithSeedProducts()
    {
        var response = await _client.GetAsync("/api/menu");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var menu = await response.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>();
        menu!.Should().HaveCount(5);
    }

    [Fact]
    public async Task GET_Menu_ShouldContainAllCategories()
    {
        var menu = (await _client.GetFromJsonAsync<IEnumerable<ProductDto>>("/api/menu"))!.ToList();

        menu.Should().Contain(p => p.Category == "Sandwich");
        menu.Should().Contain(p => p.Category == "Side");
        menu.Should().Contain(p => p.Category == "Drink");
    }

    [Fact]
    public async Task GET_Menu_ShouldExcludeInactiveProducts()
    {
        var created = await _client.PostAsJsonAsync("/api/products", new
        {
            name = "Temp Product",
            price = 1.00m,
            category = "Drink",
            subtitle = "",
            description = ""
        });
        var product = await created.Content.ReadFromJsonAsync<ProductDto>();

        await _client.DeleteAsync($"/api/products/{product!.Id}");

        var menu = await _client.GetFromJsonAsync<IEnumerable<ProductDto>>("/api/menu");
        menu!.Should().NotContain(p => p.Id == product.Id);
    }

    [Fact]
    public async Task GET_Menu_ShouldIncludeNewlyCreatedProduct()
    {
        var created = await _client.PostAsJsonAsync("/api/products", new
        {
            name = "Chá Gelado",
            price = 2.00m,
            category = "Drink",
            subtitle = "gelado",
            description = "Refrescante."
        });
        var product = await created.Content.ReadFromJsonAsync<ProductDto>();

        var menu = await _client.GetFromJsonAsync<IEnumerable<ProductDto>>("/api/menu");
        menu!.Should().Contain(p => p.Id == product!.Id);
    }

    [Fact]
    public async Task GET_Menu_ShouldHaveCorrectPlaceholders()
    {
        var menu = (await _client.GetFromJsonAsync<IEnumerable<ProductDto>>("/api/menu"))!.ToList();

        menu.Where(p => p.Category == "Sandwich").Should().AllSatisfy(p => p.Placeholder.Should().Be("burger"));
        menu.Where(p => p.Category == "Side").Should().AllSatisfy(p => p.Placeholder.Should().Be("fries"));
        menu.Where(p => p.Category == "Drink").Should().AllSatisfy(p => p.Placeholder.Should().Be("soda"));
    }
}
