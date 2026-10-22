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

    public IEnumerable<Product> GetProductList(
        DateTime? createdFrom = null,
        DateTime? createdTo = null,
        ProductType? productType = null,
        int? warehouseNumber = null,
        int page = 1,
        int pageSize = 20
    )
    {
        return _repository.GetProductList(
            createdFrom,
            createdTo,
            productType,
            warehouseNumber,
            page,
            pageSize
        );
    }

    public Product? GetProductById(Guid identifier)
    {
        return _repository.GetProductById(identifier);
    }

    public Guid CreateProduct(
        string name,
        long priceInKopecks,
        float weightInKg,
        ProductType productType,
        int warehouseNumber
    )
    {
        return _repository.CreateProduct(
            name,
            priceInKopecks,
            weightInKg,
            productType,
            warehouseNumber
        );
    }
}
