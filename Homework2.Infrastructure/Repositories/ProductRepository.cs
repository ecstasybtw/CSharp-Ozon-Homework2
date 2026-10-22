using System.Collections.Concurrent;
using Homework2.Application.Interfaces;
using Homework2.Domain.Entities;

namespace Homework2.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ConcurrentDictionary<Guid, Product> _storage = new();

    public Product? GetProductById(Guid identifier)
    {
        if (_storage.TryGetValue(identifier, out var result))
        {
            return result;
        }

        return null;
    }

    public IEnumerable<Product> GetProductList(
        DateTime? createdFrom = null,
        DateTime? createdTo = null,
        ProductType? productType = null,
        int? warehouseNumber = null,
        int page = 1,
        int pageSize = 20
    )
    {

        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        if (createdFrom.HasValue && createdTo.HasValue && createdFrom.Value >= createdTo.Value)
        {
            throw new ArgumentException("createdFrom must be before createdTo.", nameof(createdTo));
        }

        IEnumerable<Product> result = _storage.Values;

        if (createdFrom.HasValue)
        {
            result = result.Where(product => product.DateCreated >= createdFrom.Value);
        }

        if (createdTo.HasValue)
        {
            result = result.Where(product => product.DateCreated < createdTo.Value);
        }

        if (productType != null)
        {
            result = result.Where(product => product.ProductType == productType);
        }

        if (warehouseNumber != null)
        {
            result = result.Where(product => product.WarehouseNumber == warehouseNumber);
        }

        long skip = ((long)page - 1) * pageSize;

        if (skip > int.MaxValue)
        {
            return Array.Empty<Product>();
        }

        return result
            .OrderBy(product => product.DateCreated)
            .ThenBy(product => product.Id)
            .Skip((int)skip)
            .Take(pageSize)
            .ToArray();
    }


    public Guid CreateProduct(
        string name,
        long priceInKopecks,
        float weightInKg,
        ProductType productType,
        int warehouseNumber
    )
    {
        Product newProduct = new Product
        {
            Name = name,
            PriceInKopecks = priceInKopecks,
            WeightInKg = weightInKg,
            ProductType = productType,
            WarehouseNumber = warehouseNumber
        };



        if (_storage.TryAdd(newProduct.Id, newProduct))
        {
            return newProduct.Id;
        }

        throw new InvalidOperationException($"A product with Id '{newProduct.Id}' already exists.");
    }

}
