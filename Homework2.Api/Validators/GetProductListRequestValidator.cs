using FluentValidation;
using Homework2.Api.Contracts;

namespace Homework2.Api.Validators;

public class GetProductListRequestValidator : AbstractValidator<GetProductListRequest>
{
    public GetProductListRequestValidator()
    {
        RuleFor(request => request.Page).GreaterThanOrEqualTo(1);
        RuleFor(request => request.PageSize).GreaterThanOrEqualTo(1);

        RuleFor(request => request.ProductType)
            .IsInEnum()
            .When(request => request.ProductType.HasValue);

        RuleFor(request => request.CreatedTo)
            .Must((request, createdTo) => createdTo!.Value > request.CreatedFrom!.Value)
            .When(request => request.CreatedFrom.HasValue && request.CreatedTo.HasValue)
            .WithMessage("CreatedTo must be after CreatedFrom.");
    }
}
