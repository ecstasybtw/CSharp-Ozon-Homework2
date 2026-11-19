using Dapper;
using Homework2.Domain.Entities;
using Homework2.Infrastructure.Helpers;
using Homework2.Infrastructure.Repositories;
using Homework2.Infrastructure.Tests.Fixtures;
using Xunit;

namespace Homework2.Infrastructure.Tests.Repositories;

public class ProductRepositoryTests : IClassFixture<PostgresFixture>, IAsyncLifetime
{
    private readonly PostgresConnectionFactory _factory;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests(PostgresFixture fixture)
    {
        _factory = new PostgresConnectionFactory(fixture.ConnectionString);
        _repository = new ProductRepository(_factory);
    }

    public async Task InitializeAsync()
    {
        // Only the temporary test database is cleared.
        await using var connection = _factory.GetConnection();
        await connection.ExecuteAsync("TRUNCATE TABLE product");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateAndGetProduct()
    {
        var product = new Product
        {
            Name = "Milk",
            PriceInKopecks = 10000,
            WeightInKg = 1.5f,
            ProductType = ProductType.Groceries,
            WarehouseNumber = 2,
            DateCreated = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        var id = await _repository.CreateProductAsync(product);
        var result = await _repository.GetProductByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal(product.Id, id);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal(product.Name, result.Name);
        Assert.Equal(product.PriceInKopecks, result.PriceInKopecks);
        Assert.Equal(product.WeightInKg, result.WeightInKg);
        Assert.Equal(product.ProductType, result.ProductType);
        Assert.Equal(product.WarehouseNumber, result.WarehouseNumber);
        Assert.Equal(product.DateCreated, result.DateCreated);
    }

    [Fact]
    public async Task UnknownProductReturnsNull()
    {
        var id = Guid.NewGuid();

        Assert.Null(await _repository.GetProductByIdAsync(id));
        Assert.Null(await _repository.UpdateProductAsync(id, name: "New name"));
    }

    [Fact]
    public async Task GetProductsWithFilters()
    {
        var date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var products = new[]
        {
            new Product { Name = "Milk", ProductType = ProductType.Groceries, WarehouseNumber = 1, DateCreated = date },
            new Product { Name = "TV", ProductType = ProductType.Appliance, WarehouseNumber = 1, DateCreated = date },
            new Product { Name = "Bread", ProductType = ProductType.Groceries, WarehouseNumber = 2, DateCreated = date },
            new Product { Name = "Old milk", ProductType = ProductType.Groceries, WarehouseNumber = 1, DateCreated = date.AddDays(-1) },
            new Product { Name = "New milk", ProductType = ProductType.Groceries, WarehouseNumber = 1, DateCreated = date.AddDays(1) }
        };
        foreach (var product in products)
        {
            await _repository.CreateProductAsync(product);
        }

        var result = await _repository.GetProductListAsync(
            createdFrom: date, createdTo: date.AddDays(1),
            productType: ProductType.Groceries, warehouseNumber: 1);

        var found = Assert.Single(result);
        Assert.Equal(products[0].Id, found.Id);
    }

    [Fact]
    public async Task GetSecondPage()
    {
        var date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var first = new Product { Name = "First", DateCreated = date };
        var second = new Product { Name = "Second", DateCreated = date.AddDays(1) };
        var third = new Product { Name = "Third", DateCreated = date.AddDays(2) };
        await _repository.CreateProductAsync(third);
        await _repository.CreateProductAsync(first);
        await _repository.CreateProductAsync(second);

        var result = await _repository.GetProductListAsync(page: 2, pageSize: 1);

        Assert.Equal(second.Id, Assert.Single(result).Id);
        Assert.Empty(await _repository.GetProductListAsync(page: 4, pageSize: 1));
    }

    [Fact]
    public async Task UpdateOnlyName()
    {
        var product = new Product
        {
            Name = "Old name", PriceInKopecks = 100, WeightInKg = 2,
            ProductType = ProductType.Appliance, WarehouseNumber = 3,
            DateCreated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        await _repository.CreateProductAsync(product);

        var result = await _repository.UpdateProductAsync(product.Id, name: "New name");

        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal("New name", result.Name);
        Assert.Equal(product.PriceInKopecks, result.PriceInKopecks);
        Assert.Equal(product.WeightInKg, result.WeightInKg);
        Assert.Equal(product.ProductType, result.ProductType);
        Assert.Equal(product.WarehouseNumber, result.WarehouseNumber);
        Assert.Equal(product.DateCreated, result.DateCreated);
        var saved = await _repository.GetProductByIdAsync(product.Id);
        Assert.NotNull(saved);
        Assert.Equal("New name", saved.Name);
    }

    [Fact]
    public async Task UpdateOtherFieldsAndZeroValues()
    {
        var product = new Product
        {
            Name = "Product", PriceInKopecks = 100, WeightInKg = 1,
            ProductType = ProductType.Appliance, WarehouseNumber = 1
        };
        await _repository.CreateProductAsync(product);

        var result = await _repository.UpdateProductAsync(product.Id,
            priceInKopecks: 0, weightInKg: 2.5f,
            productType: ProductType.General, warehouseNumber: 2);

        Assert.NotNull(result);
        Assert.Equal("Product", result.Name);
        Assert.Equal(0L, result.PriceInKopecks);
        Assert.Equal(2.5f, result.WeightInKg);
        Assert.Equal(ProductType.General, result.ProductType);
        Assert.Equal(2, result.WarehouseNumber);
    }
}
