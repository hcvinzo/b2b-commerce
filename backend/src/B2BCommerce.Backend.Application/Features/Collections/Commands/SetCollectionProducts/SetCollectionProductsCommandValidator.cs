using B2BCommerce.Backend.Domain.Entities;
using FluentValidation;

namespace B2BCommerce.Backend.Application.Features.Collections.Commands.SetCollectionProducts;

/// <summary>
/// Validator for SetCollectionProductsCommand
/// </summary>
public class SetCollectionProductsCommandValidator : AbstractValidator<SetCollectionProductsCommand>
{
    public SetCollectionProductsCommandValidator()
    {
        RuleFor(x => x.CollectionId)
            .NotEmpty().WithMessage("Collection ID is required");

        RuleFor(x => x.Products)
            .NotNull().WithMessage("Products list is required");

        RuleFor(x => x.Products.Count)
            .LessThanOrEqualTo(Collection.MaxProductsPerManualCollection)
            .WithMessage($"Cannot add more than {Collection.MaxProductsPerManualCollection} products to a manual collection");

        RuleForEach(x => x.Products).ChildRules(p =>
        {
            p.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required");

            p.RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be non-negative");
        });

        // Check for duplicate product IDs
        RuleFor(x => x.Products)
            .Must(products => products.Select(p => p.ProductId).Distinct().Count() == products.Count)
            .WithMessage("Duplicate product IDs are not allowed");
    }
}
