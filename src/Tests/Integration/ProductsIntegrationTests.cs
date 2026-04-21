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

public class ProductsIntegrationTests : IAsyncLifetime
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
    public async Task POST_Products_ShouldCreateProduct_AndReturn201()
    {
        var response = await _client.PostAsJsonAsync("/api/products", new
        {
            name = "Onion Ring",
            price = 3.00m,
            category = "Side",
            subtitle = "crocante",
            description = "Anéis dourados.",
            imageUrl = (string?)null
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        product!.Name.Should().Be("Onion Ring");
        product.Category.Should().Be("Side");
        product.Price.Should().Be(3.00m);
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task POST_Products_ShouldReturn400_WhenCategoryInvalid()
    {
        var response = await _client.PostAsJsonAsync("/api/products", new
        {
            name = "Algo",
            price = 5.00m,
            category = "Invalido",
            subtitle = "",
            description = ""
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_Products_ShouldReturn400_WhenPriceZero()
    {
        var response = await _client.PostAsJsonAsync("/api/products", new
        {
            name = "Algo",
            price = 0m,
            category = "Sandwich",
            subtitle = "",
            description = ""
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PUT_Products_ShouldUpdateProduct()
    {
        var created = await _client.PostAsJsonAsync("/api/products", new
        {
            name = "Old Name",
            price = 5.00m,
            category = "Sandwich",
            subtitle = "",
            description = ""
        });
        var product = await created.Content.ReadFromJsonAsync<ProductDto>();

        var response = await _client.PutAsJsonAsync($"/api/products/{product!.Id}", new
        {
            name = "New Name",
            price = 9.99m,
            category = "Sandwich",
            subtitle = "updated",
            description = "updated desc"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<ProductDto>();
        updated!.Name.Should().Be("New Name");
        updated.Price.Should().Be(9.99m);
    }

    [Fact]
    public async Task PUT_Products_ShouldReturn404_WhenNotFound()
    {
        var response = await _client.PutAsJsonAsync($"/api/products/{Guid.NewGuid()}", new
        {
            name = "X",
            price = 5.00m,
            category = "Sandwich",
            subtitle = "",
            description = ""
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DELETE_Products_ShouldReturn204_AndRemoveFromMenu()
    {
        var created = await _client.PostAsJsonAsync("/api/products", new
        {
            name = "To Delete",
            price = 2.00m,
            category = "Drink",
            subtitle = "",
            description = ""
        });
        var product = await created.Content.ReadFromJsonAsync<ProductDto>();

        var deleteResponse = await _client.DeleteAsync($"/api/products/{product!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var menu = await _client.GetFromJsonAsync<IEnumerable<ProductDto>>("/api/menu");
        menu!.Should().NotContain(p => p.Id == product.Id);
    }

    [Fact]
    public async Task GET_Products_ShouldReturnSeedData()
    {
        var products = await _client.GetFromJsonAsync<IEnumerable<ProductDto>>("/api/products");

        products!.Should().Contain(p => p.Name == "X Burger");
        products!.Should().Contain(p => p.Name == "Batata Frita");
        products!.Should().Contain(p => p.Name == "Refrigerante");
    }
}
