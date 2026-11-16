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
    private readonly IValidator<UpdateProductRequest> _updateValidator;

    public ProductController(
        IProductService service,
        IValidator<CreateProductRequest> createValidator,
        IValidator<GetProductListRequest> listValidator,
        IValidator<UpdateProductRequest> updateValidator
    )
    {
        _service = service;
        _createValidator = createValidator;
        _listValidator = listValidator;
        _updateValidator = updateValidator;
    }

    [HttpPost]
    public async Task<ActionResult<CreateProductResponse>> CreateProductAsync(CreateProductRequest request)
    {
        var validationResult = _createValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }

        var identifier = await _service.CreateProductAsync(
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

    [HttpPatch("{id}")]
    public async Task<ActionResult<UpdateProductResponse>> UpdateProductAsync(
        Guid id,
        [FromBody] UpdateProductRequest request
    )
    {
        var validationResult = _updateValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }

        var product = await _service.UpdateProductAsync(
            identifier: id,
            name: request.Name,
            priceInKopecks: request.PriceInKopecks,
            weightInKg: request.WeightInKg,
            productType: request.ProductType,
            warehouseNumber: request.WarehouseNumber
        );

        if (product is null)
        {
            return NotFound();
        }

        return Ok(new UpdateProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            PriceInKopecks = product.PriceInKopecks,
            WeightInKg = product.WeightInKg,
            ProductType = product.ProductType,
            WarehouseNumber = product.WarehouseNumber,
            DateCreated = product.DateCreated
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProductByIdAsync(Guid id)
    {
        var product = await _service.GetProductByIdAsync(identifier: id);

        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProductListAsync(
        [FromQuery] GetProductListRequest request
    )
    {
        var validationResult = _listValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }

        var products = await _service.GetProductListAsync(
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
