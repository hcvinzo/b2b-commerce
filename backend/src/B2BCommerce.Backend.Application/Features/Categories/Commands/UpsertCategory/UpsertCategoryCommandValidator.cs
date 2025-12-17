using FluentValidation;

namespace B2BCommerce.Backend.Application.Features.Categories.Commands.UpsertCategory;

/// <summary>
/// Validator for UpsertCategoryCommand
/// </summary>
public class UpsertCategoryCommandValidator : AbstractValidator<UpsertCategoryCommand>
{
    public UpsertCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(200).WithMessage("Category name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.ExternalCode)
            .MaximumLength(100).WithMessage("External code must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ExternalCode));

        RuleFor(x => x.ExternalId)
            .MaximumLength(100).WithMessage("External ID must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ExternalId));

        RuleFor(x => x.ParentExternalCode)
            .MaximumLength(100).WithMessage("Parent external code must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ParentExternalCode));

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be non-negative");

    }
}
