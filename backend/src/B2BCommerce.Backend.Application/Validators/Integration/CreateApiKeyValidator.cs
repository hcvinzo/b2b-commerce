using B2BCommerce.Backend.Application.DTOs.Integration;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.Helpers;
using FluentValidation;

namespace B2BCommerce.Backend.Application.Validators.Integration;

public class CreateApiKeyValidator : AbstractValidator<CreateApiKeyDto>
{
    public CreateApiKeyValidator()
    {
        RuleFor(x => x.ApiClientId)
            .NotEmpty().WithMessage("API Client ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Key name is required")
            .MaximumLength(100).WithMessage("Key name cannot exceed 100 characters");

        RuleFor(x => x.RateLimitPerMinute)
            .GreaterThan(0).WithMessage("Rate limit must be positive")
            .LessThanOrEqualTo(10000).WithMessage("Rate limit cannot exceed 10000 per minute");

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).When(x => x.ExpiresAt.HasValue)
            .WithMessage("Expiration date must be in the future");

        RuleFor(x => x.Permissions)
            .NotEmpty().WithMessage("At least one permission is required");

        RuleForEach(x => x.Permissions)
            .Must(IntegrationPermissionScopes.IsValidScope)
            .WithMessage((_, scope) => $"Invalid permission scope: {scope}");

        RuleForEach(x => x.IpWhitelist)
            .Must(IpAddressHelper.IsValidIpOrCidr)
            .WithMessage((_, ip) => $"Invalid IP address or CIDR: {ip}");
    }
}
