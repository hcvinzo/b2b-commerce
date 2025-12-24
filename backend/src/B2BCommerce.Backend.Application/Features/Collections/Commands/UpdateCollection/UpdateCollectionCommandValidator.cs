using FluentValidation;

namespace B2BCommerce.Backend.Application.Features.Collections.Commands.UpdateCollection;

/// <summary>
/// Validator for UpdateCollectionCommand
/// </summary>
public class UpdateCollectionCommandValidator : AbstractValidator<UpdateCollectionCommand>
{
    public UpdateCollectionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Collection ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("Image URL cannot exceed 500 characters")
            .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Image URL must be a valid URL");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be non-negative");

        RuleFor(x => x.EndDate)
            .Must((cmd, endDate) => !endDate.HasValue || !cmd.StartDate.HasValue || endDate >= cmd.StartDate)
            .WithMessage("End date must be after start date");
    }
}
