using B2BCommerce.Backend.Application.DTOs.Customers;
using FluentValidation;

namespace B2BCommerce.Backend.Application.Validators.Customers;

public class RegisterCustomerValidator : AbstractValidator<RegisterCustomerDto>
{
    public RegisterCustomerValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Company title is required")
            .MaximumLength(300).WithMessage("Company title cannot exceed 300 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.ContactFirstName)
            .NotEmpty().WithMessage("Contact first name is required")
            .MaximumLength(100).WithMessage("Contact first name cannot exceed 100 characters");

        RuleFor(x => x.ContactLastName)
            .NotEmpty().WithMessage("Contact last name is required")
            .MaximumLength(100).WithMessage("Contact last name cannot exceed 100 characters");

        RuleFor(x => x.ContactEmail)
            .NotEmpty().WithMessage("Contact email is required")
            .EmailAddress().WithMessage("Invalid contact email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit");

        RuleFor(x => x.PasswordConfirmation)
            .Equal(x => x.Password).WithMessage("Password confirmation must match password");

        RuleFor(x => x.AcceptTerms)
            .Equal(true).WithMessage("You must accept the terms and conditions");

        RuleFor(x => x.AcceptKvkk)
            .Equal(true).WithMessage("You must accept the KVKK consent");

        // Optional validations
        RuleFor(x => x.TaxNo)
            .MaximumLength(20).WithMessage("Tax number cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.TaxNo));

        RuleFor(x => x.TaxOffice)
            .MaximumLength(200).WithMessage("Tax office cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.TaxOffice));

        RuleFor(x => x.Website)
            .MaximumLength(500).WithMessage("Website URL cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Website));

        RuleFor(x => x.EstablishmentYear)
            .InclusiveBetween(1800, DateTime.Now.Year)
            .WithMessage($"Establishment year must be between 1800 and {DateTime.Now.Year}")
            .When(x => x.EstablishmentYear.HasValue);
    }
}
