using FluentValidation;
using Homework2.Api.Contracts;
using Homework2.Application.Interfaces;
using Homework2.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _service;
    private readonly IValidator<CreateProductRequest> _createValidator;
    private readonly IValidator<GetProductListRequest> _listValidator;

    public ProductController(
        IProductService service,
        IValidator<CreateProductRequest> createValidator,
        IValidator<GetProductListRequest> listValidator
    )
    {
        _service = service;
        _createValidator = createValidator;
        _listValidator = listValidator;
    }

    [HttpPost]
    public ActionResult<CreateProductResponse> CreateProduct(CreateProductRequest request)
    {
        var validationResult = _createValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }

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
        var validationResult = _listValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }

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
