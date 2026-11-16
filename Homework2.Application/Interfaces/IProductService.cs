using Homework2.Domain.Entities;

namespace Homework2.Application.Interfaces;

public interface IProductService
{
    public Task<Product?> UpdateProductAsync(
        Guid identifier,
        string? name = null,
        long? priceInKopecks = null,
        float? weightInKg = null,
        ProductType? productType = null,
        int? warehouseNumber = null
    );

    public Task<Guid> CreateProductAsync(
        string name,
        long priceInKopecks,
        float weightInKg,
        ProductType productType,
        int warehouseNumber
    );

    public Task<IEnumerable<Product>> GetProductListAsync(
        DateTime? createdFrom = null,
        DateTime? createdTo = null,
        ProductType? productType = null,
        int? warehouseNumber = null,
        int page = 1,
        int pageSize = 20
    );

    public Task<Product?> GetProductByIdAsync(
        Guid identifier
    );
}
