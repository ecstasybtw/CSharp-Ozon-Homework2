using Homework2.Domain.Entities;

namespace Homework2.Application.Interfaces;

public interface IProductRepository
{

    public IEnumerable<Product> GetProductList(
        DateTime? createdFrom = null,
        DateTime? createdTo = null,
        ProductType? productType = null,
        int? warehouseNumber = null,
        int page = 1,
        int pageSize = 20
    );

    public Product? GetProductById(Guid identifier);

    public Guid CreateProduct(
        string name,
        long priceInKopecks,
        float weightInKg,
        ProductType productType,
        int warehouseNumber
    );
}
