using FluentValidation;

namespace B2BCommerce.Backend.Application.Features.Collections.Commands.CreateCollection;

/// <summary>
/// Validator for CreateCollectionCommand
/// </summary>
public class CreateCollectionCommandValidator : AbstractValidator<CreateCollectionCommand>
{
    public CreateCollectionCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("Image URL cannot exceed 500 characters")
            .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Image URL must be a valid URL");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid collection type");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be non-negative");

        RuleFor(x => x.EndDate)
            .Must((cmd, endDate) => !endDate.HasValue || !cmd.StartDate.HasValue || endDate >= cmd.StartDate)
            .WithMessage("End date must be after start date");
    }
}
