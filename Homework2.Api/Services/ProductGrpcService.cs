using FluentValidation;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Homework2.Application.Interfaces;
using Homework2.Products;
using DomainProduct = Homework2.Domain.Entities.Product;
using DomainProductType = Homework2.Domain.Entities.ProductType;

namespace Homework2.Api.Services;

public class ProductGrpcService : ProductService.ProductServiceBase
{
    private readonly IProductService _productService;
    private readonly IValidator<CreateProductRequest> _createValidator;
    private readonly IValidator<GetProductRequest> _getValidator;
    private readonly IValidator<GetProductsListRequest> _listValidator;
    private readonly IValidator<UpdateProductRequest> _updateValidator;

    public ProductGrpcService(
        IProductService productService,
        IValidator<CreateProductRequest> createValidator,
        IValidator<GetProductRequest> getValidator,
        IValidator<GetProductsListRequest> listValidator,
        IValidator<UpdateProductRequest> updateValidator
    )
    {
        _productService = productService;
        _createValidator = createValidator;
        _getValidator = getValidator;
        _listValidator = listValidator;
        _updateValidator = updateValidator;
    }

    public override async Task<CreateProductResponse> CreateProduct(
        CreateProductRequest request,
        ServerCallContext context
    )
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        ValidateRequest(_createValidator, request);

        var identifier = await _productService.CreateProductAsync(
            name: request.Name,
            priceInKopecks: request.PriceInKopecks,
            weightInKg: request.WeightInKg,
            productType: ToDomainProductType(request.ProductType),
            warehouseNumber: request.WarehouseNumber
        );

        return new CreateProductResponse
        {
            Identifier = identifier.ToString()
        };
    }

    public override async Task<Product> UpdateProduct(
        UpdateProductRequest request,
        ServerCallContext context
    )
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        ValidateRequest(_updateValidator, request);

        var product = await _productService.UpdateProductAsync(
            identifier: Guid.Parse(request.Identifier),
            name: request.HasName ? request.Name : null,
            priceInKopecks: request.HasPriceInKopecks ? request.PriceInKopecks : null,
            weightInKg: request.HasWeightInKg ? request.WeightInKg : null,
            productType: request.HasProductType ? ToDomainProductType(request.ProductType) : null,
            warehouseNumber: request.HasWarehouseNumber ? request.WarehouseNumber : null
        );

        if (product is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Product with this Id not found."));
        }

        return ToGrpcProduct(product);
    }

    public override async Task<Product> GetProductById(
        GetProductRequest request,
        ServerCallContext context
    )
    {
        ValidateRequest(_getValidator, request);
        var identifier = Guid.Parse(request.Identifier);

        var productToReturn = await _productService.GetProductByIdAsync(identifier);

        if (productToReturn == null)
        {
            var status = new Status(StatusCode.NotFound, "Product with this Id not found.");
            throw new RpcException(status);
        }

        return ToGrpcProduct(productToReturn);
    }

    public override async Task GetProductsList(
        GetProductsListRequest request,
        IServerStreamWriter<Product> responseStream,
        ServerCallContext context
    )
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        ValidateRequest(_listValidator, request);

        DateTime? createdFrom;
        DateTime? createdTo;
        try
        {
            createdFrom = request.CreatedFrom?.ToDateTime();
            createdTo = request.CreatedTo?.ToDateTime();
        }
        catch (InvalidOperationException)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid timestamp."));
        }

        DomainProductType? productType = null;
        if (request.HasProductType)
        {
            productType = ToDomainProductType(request.ProductType);
        }

        IEnumerable<DomainProduct> products;
        try
        {
            products = await _productService.GetProductListAsync(
                createdFrom: createdFrom,
                createdTo: createdTo,
                productType: productType,
                warehouseNumber: request.HasWarehouseNumber ? request.WarehouseNumber : null,
                page: request.HasPage ? request.Page : 1,
                pageSize: request.HasPageSize ? request.PageSize : 20
            );
        }
        catch (ArgumentException exception)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, exception.Message));
        }

        foreach (var product in products)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            await responseStream.WriteAsync(ToGrpcProduct(product));
        }
    }

    private static void ValidateRequest<T>(IValidator<T> validator, T request)
    {
        var result = validator.Validate(request);
        if (!result.IsValid)
        {
            var message = string.Join("; ", result.Errors.Select(error => $"{error.PropertyName}: {error.ErrorMessage}"));
            throw new RpcException(new Status(StatusCode.InvalidArgument, message));
        }
    }

    private static DomainProductType ToDomainProductType(ProductType productType)
    {
        return productType switch
        {
            ProductType.General => DomainProductType.General,
            ProductType.HouseholdChemicals => DomainProductType.HouseholdChemicals,
            ProductType.Appliance => DomainProductType.Appliance,
            ProductType.Groceries => DomainProductType.Groceries,
            _ => throw new RpcException(new Status(StatusCode.InvalidArgument, "Unknown product type."))
        };
    }

    private static Product ToGrpcProduct(DomainProduct product)
    {
        return new Product
        {
            Identifier = product.Id.ToString(),
            Name = product.Name,
            PriceInKopecks = product.PriceInKopecks,
            WeightInKg = product.WeightInKg,
            ProductType = product.ProductType switch
            {
                DomainProductType.General => ProductType.General,
                DomainProductType.HouseholdChemicals => ProductType.HouseholdChemicals,
                DomainProductType.Appliance => ProductType.Appliance,
                DomainProductType.Groceries => ProductType.Groceries,
                _ => throw new InvalidOperationException("Unknown product type.")
            },
            WarehouseNumber = product.WarehouseNumber,
            DateCreated = Timestamp.FromDateTime(product.DateCreated)
        };
    }
}
