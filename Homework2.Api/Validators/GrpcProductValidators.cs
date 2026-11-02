using FluentValidation;
using Google.Protobuf.WellKnownTypes;
using Homework2.Products;

namespace Homework2.Api.Validators;

public class GrpcCreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public GrpcCreateProductRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty();
        RuleFor(request => request.WeightInKg).GreaterThan(0).Must(float.IsFinite);
        RuleFor(request => request.PriceInKopecks).GreaterThanOrEqualTo(0);
        RuleFor(request => request.ProductType).IsInEnum();
    }
}

public class GrpcGetProductRequestValidator : AbstractValidator<GetProductRequest>
{
    public GrpcGetProductRequestValidator()
    {
        RuleFor(request => request.Identifier)
            .Must(identifier => Guid.TryParse(identifier, out _))
            .WithMessage("Identifier must be a valid Guid.");
    }
}

public class GrpcGetProductsListRequestValidator : AbstractValidator<GetProductsListRequest>
{
    public GrpcGetProductsListRequestValidator()
    {
        RuleFor(request => request.Page).GreaterThanOrEqualTo(1).When(request => request.HasPage);
        RuleFor(request => request.PageSize).GreaterThanOrEqualTo(1).When(request => request.HasPageSize);
        RuleFor(request => request.ProductType).IsInEnum().When(request => request.HasProductType);
        RuleFor(request => request.CreatedFrom).Must(IsValidTimestamp).WithMessage("Invalid timestamp.");
        RuleFor(request => request.CreatedTo).Must(IsValidTimestamp).WithMessage("Invalid timestamp.");

        RuleFor(request => request.CreatedTo)
            .Must((request, createdTo) => createdTo.ToDateTime() > request.CreatedFrom.ToDateTime())
            .When(request => request.CreatedFrom != null && request.CreatedTo != null
                && IsValidTimestamp(request.CreatedFrom) && IsValidTimestamp(request.CreatedTo))
            .WithMessage("CreatedTo must be after CreatedFrom.");
    }

    private static bool IsValidTimestamp(Timestamp? timestamp)
    {
        if (timestamp == null)
        {
            return true;
        }

        try
        {
            timestamp.ToDateTime();
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}
