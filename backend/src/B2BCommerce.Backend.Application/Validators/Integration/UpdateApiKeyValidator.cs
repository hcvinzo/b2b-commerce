using B2BCommerce.Backend.Application.DTOs.Integration;
using FluentValidation;

namespace B2BCommerce.Backend.Application.Validators.Integration;

public class UpdateApiKeyValidator : AbstractValidator<UpdateApiKeyDto>
{
    public UpdateApiKeyValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Key name is required")
            .MaximumLength(100).WithMessage("Key name cannot exceed 100 characters");

        RuleFor(x => x.RateLimitPerMinute)
            .GreaterThan(0).WithMessage("Rate limit must be positive")
            .LessThanOrEqualTo(10000).WithMessage("Rate limit cannot exceed 10000 per minute");

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).When(x => x.ExpiresAt.HasValue)
            .WithMessage("Expiration date must be in the future");
    }
}
