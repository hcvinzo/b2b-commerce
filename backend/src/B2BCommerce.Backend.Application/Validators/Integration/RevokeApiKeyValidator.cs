using B2BCommerce.Backend.Application.DTOs.Integration;
using FluentValidation;

namespace B2BCommerce.Backend.Application.Validators.Integration;

public class RevokeApiKeyValidator : AbstractValidator<RevokeApiKeyDto>
{
    public RevokeApiKeyValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Revocation reason is required")
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters");
    }
}
