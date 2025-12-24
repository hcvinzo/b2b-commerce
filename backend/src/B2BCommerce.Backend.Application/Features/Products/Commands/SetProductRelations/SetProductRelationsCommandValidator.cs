using B2BCommerce.Backend.Domain.Entities;
using FluentValidation;

namespace B2BCommerce.Backend.Application.Features.Products.Commands.SetProductRelations;

/// <summary>
/// Validator for SetProductRelationsCommand
/// </summary>
public class SetProductRelationsCommandValidator : AbstractValidator<SetProductRelationsCommand>
{
    public SetProductRelationsCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.RelationType)
            .IsInEnum().WithMessage("Invalid relation type");

        RuleFor(x => x.RelatedProducts)
            .NotNull().WithMessage("Related products list is required");

        RuleFor(x => x.RelatedProducts.Count)
            .LessThanOrEqualTo(ProductRelation.MaxRelatedProductsPerType)
            .WithMessage($"Cannot add more than {ProductRelation.MaxRelatedProductsPerType} related products per type");

        RuleForEach(x => x.RelatedProducts).ChildRules(rp =>
        {
            rp.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Related product ID is required");

            rp.RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be non-negative");
        });

        // Check for duplicate product IDs
        RuleFor(x => x.RelatedProducts)
            .Must(products => products.Select(p => p.ProductId).Distinct().Count() == products.Count)
            .WithMessage("Duplicate related product IDs are not allowed");
    }
}
