using Homework2.Application.Interfaces;
using Homework2.Domain.Entities;

namespace Homework2.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Product>> GetProductListAsync(
        DateTime? createdFrom = null,
        DateTime? createdTo = null,
        ProductType? productType = null,
        int? warehouseNumber = null,
        int page = 1,
        int pageSize = 20
    )
    {
        return _repository.GetProductListAsync(
            createdFrom,
            createdTo,
            productType,
            warehouseNumber,
            page,
            pageSize
        );
    }

    public Task<Product?> GetProductByIdAsync(Guid identifier)
    {
        return _repository.GetProductByIdAsync(identifier);
    }

    public Task<Guid> CreateProductAsync(
        string name,
        long priceInKopecks,
        float weightInKg,
        ProductType productType,
        int warehouseNumber
    )
    {
        var product = new Product
        {
            Name = name,
            PriceInKopecks = priceInKopecks,
            WeightInKg = weightInKg,
            ProductType = productType,
            WarehouseNumber = warehouseNumber
        };

        return _repository.CreateProductAsync(product);
    }
}
