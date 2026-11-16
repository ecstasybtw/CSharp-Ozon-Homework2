using FluentValidation;
using Homework2.Api.Contracts;

namespace Homework2.Api.Validators;

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(request => request)
            .Must(request => request.Name != null
                || request.PriceInKopecks.HasValue
                || request.WeightInKg.HasValue
                || request.ProductType.HasValue
                || request.WarehouseNumber.HasValue)
            .WithMessage("At least one field must be provided for update.");

        RuleFor(request => request.Name)
            .NotEmpty()
            .When(request => request.Name != null);

        RuleFor(request => request.PriceInKopecks)
            .GreaterThanOrEqualTo(0)
            .When(request => request.PriceInKopecks.HasValue);

        RuleFor(request => request.WeightInKg)
            .GreaterThan(0)
            .Must(weight => !weight.HasValue || float.IsFinite(weight.Value))
            .When(request => request.WeightInKg.HasValue);

        RuleFor(request => request.ProductType)
            .IsInEnum()
            .When(request => request.ProductType.HasValue);
    }
}
