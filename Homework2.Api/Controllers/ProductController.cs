using Homework2.Api.Contracts;
using Homework2.Application.Interfaces;
using Homework2.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _service;

    public ProductController(IProductService service)
    {
        _service = service;
    }

    [HttpPost]
    public ActionResult<CreateProductResponse> CreateProduct(CreateProductRequest request)
    {
        var identifier = _service.CreateProduct(
            name: request.Name,
            priceInKopecks: request.PriceInKopecks,
            weightInKg: request.WeightInKg,
            productType: request.ProductType,
            warehouseNumber: request.WarehouseNumber
        );

        return Created(
             $"/api/products/{identifier}",
            new CreateProductResponse(
            identifier: identifier
        ));
    }

    [HttpGet("{id}")]
    public ActionResult<Product> GetProductById(Guid id)
    {
        var product = _service.GetProductById(identifier: id);

        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpGet]
    public ActionResult<IEnumerable<Product>> GetProductList(
        [FromQuery] GetProductListRequest request
    )
    {
        var products = _service.GetProductList(
            createdFrom: request.CreatedFrom,
            createdTo: request.CreatedTo,
            productType: request.ProductType,
            warehouseNumber: request.WarehouseNumber,
            page: request.Page,
            pageSize: request.PageSize
        );

        return Ok(products);
    }
}
