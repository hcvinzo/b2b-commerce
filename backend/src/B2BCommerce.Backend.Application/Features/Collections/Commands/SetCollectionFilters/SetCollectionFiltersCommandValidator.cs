using FluentValidation;

namespace B2BCommerce.Backend.Application.Features.Collections.Commands.SetCollectionFilters;

/// <summary>
/// Validator for SetCollectionFiltersCommand
/// </summary>
public class SetCollectionFiltersCommandValidator : AbstractValidator<SetCollectionFiltersCommand>
{
    public SetCollectionFiltersCommandValidator()
    {
        RuleFor(x => x.CollectionId)
            .NotEmpty().WithMessage("Collection ID is required");

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0).When(x => x.MinPrice.HasValue)
            .WithMessage("Minimum price must be non-negative");

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0).When(x => x.MaxPrice.HasValue)
            .WithMessage("Maximum price must be non-negative");

        RuleFor(x => x.MaxPrice)
            .Must((cmd, maxPrice) => !maxPrice.HasValue || !cmd.MinPrice.HasValue || maxPrice >= cmd.MinPrice)
            .WithMessage("Maximum price must be greater than or equal to minimum price");
    }
}
