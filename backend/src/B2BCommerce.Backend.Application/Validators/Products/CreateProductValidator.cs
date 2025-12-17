using B2BCommerce.Backend.Application.DTOs.Products;
using FluentValidation;

namespace B2BCommerce.Backend.Application.Validators.Products;

public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU is required")
            .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters");

        RuleFor(x => x.CategoryIds)
            .NotEmpty().WithMessage("At least one category is required")
            .Must(c => c.Count > 0).WithMessage("At least one category is required");

        RuleFor(x => x.ListPrice)
            .GreaterThan(0).WithMessage("List price must be greater than zero");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative");

        RuleFor(x => x.MinimumOrderQuantity)
            .GreaterThanOrEqualTo(1).WithMessage("Minimum order quantity must be at least 1");

        RuleFor(x => x.TaxRate)
            .InclusiveBetween(0, 1).WithMessage("Tax rate must be between 0 and 1");

        // Tier prices should be less than or equal to list price
        RuleFor(x => x.Tier1Price)
            .LessThanOrEqualTo(x => x.ListPrice)
            .When(x => x.Tier1Price.HasValue)
            .WithMessage("Tier 1 price cannot exceed list price");

        RuleFor(x => x.Tier2Price)
            .LessThanOrEqualTo(x => x.ListPrice)
            .When(x => x.Tier2Price.HasValue)
            .WithMessage("Tier 2 price cannot exceed list price");

        RuleFor(x => x.Tier3Price)
            .LessThanOrEqualTo(x => x.ListPrice)
            .When(x => x.Tier3Price.HasValue)
            .WithMessage("Tier 3 price cannot exceed list price");

        RuleFor(x => x.Tier4Price)
            .LessThanOrEqualTo(x => x.ListPrice)
            .When(x => x.Tier4Price.HasValue)
            .WithMessage("Tier 4 price cannot exceed list price");

        RuleFor(x => x.Tier5Price)
            .LessThanOrEqualTo(x => x.ListPrice)
            .When(x => x.Tier5Price.HasValue)
            .WithMessage("Tier 5 price cannot exceed list price");

        RuleFor(x => x.Weight)
            .GreaterThan(0).When(x => x.Weight.HasValue)
            .WithMessage("Weight must be positive");

        RuleFor(x => x.Length)
            .GreaterThan(0).When(x => x.Length.HasValue)
            .WithMessage("Length must be positive");

        RuleFor(x => x.Width)
            .GreaterThan(0).When(x => x.Width.HasValue)
            .WithMessage("Width must be positive");

        RuleFor(x => x.Height)
            .GreaterThan(0).When(x => x.Height.HasValue)
            .WithMessage("Height must be positive");
    }
}
