using B2BCommerce.Backend.Application.DTOs.Integration;
using B2BCommerce.Backend.Domain.Helpers;
using FluentValidation;

namespace B2BCommerce.Backend.Application.Validators.Integration;

public class AddIpWhitelistValidator : AbstractValidator<AddIpWhitelistDto>
{
    public AddIpWhitelistValidator()
    {
        RuleFor(x => x.IpAddress)
            .NotEmpty().WithMessage("IP address is required")
            .Must(IpAddressHelper.IsValidIpOrCidr).WithMessage("Invalid IP address or CIDR notation")
            .MaximumLength(45).WithMessage("IP address cannot exceed 45 characters");

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description cannot exceed 200 characters");
    }
}
