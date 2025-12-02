using B2BCommerce.Backend.Application.DTOs.Customers;
using FluentValidation;

namespace B2BCommerce.Backend.Application.Validators.Customers;

public class RegisterCustomerValidator : AbstractValidator<RegisterCustomerDto>
{
    public RegisterCustomerValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required")
            .MaximumLength(200).WithMessage("Company name cannot exceed 200 characters");

        RuleFor(x => x.TaxNumber)
            .NotEmpty().WithMessage("Tax number is required")
            .Length(10, 20).WithMessage("Tax number must be between 10 and 20 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^[\d\s\-\+\(\)]{10,15}$").WithMessage("Invalid phone number format");

        RuleFor(x => x.ContactPersonName)
            .NotEmpty().WithMessage("Contact person name is required")
            .MaximumLength(100).WithMessage("Contact person name cannot exceed 100 characters");

        RuleFor(x => x.ContactPersonTitle)
            .MaximumLength(100).WithMessage("Contact person title cannot exceed 100 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit");

        // Billing address
        RuleFor(x => x.BillingStreet).NotEmpty().WithMessage("Billing street is required");
        RuleFor(x => x.BillingCity).NotEmpty().WithMessage("Billing city is required");
        RuleFor(x => x.BillingState).NotEmpty().WithMessage("Billing state is required");
        RuleFor(x => x.BillingCountry).NotEmpty().WithMessage("Billing country is required");
        RuleFor(x => x.BillingPostalCode).NotEmpty().WithMessage("Billing postal code is required");

        // Shipping address
        RuleFor(x => x.ShippingStreet).NotEmpty().WithMessage("Shipping street is required");
        RuleFor(x => x.ShippingCity).NotEmpty().WithMessage("Shipping city is required");
        RuleFor(x => x.ShippingState).NotEmpty().WithMessage("Shipping state is required");
        RuleFor(x => x.ShippingCountry).NotEmpty().WithMessage("Shipping country is required");
        RuleFor(x => x.ShippingPostalCode).NotEmpty().WithMessage("Shipping postal code is required");

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Credit limit must be non-negative");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code");
    }
}
