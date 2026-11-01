using FluentValidation;
using Homework2.Api.Contracts;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty();
        RuleFor(request => request.WeightInKg).GreaterThan(0);
        RuleFor(request => request.WeightInKg).Must(float.IsFinite);
        RuleFor(request => request.PriceInKopecks).GreaterThanOrEqualTo(0);
        RuleFor(request => request.ProductType).IsInEnum();
    }
}
