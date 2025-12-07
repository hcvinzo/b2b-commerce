using B2BCommerce.Backend.Application.DTOs.Integration;
using FluentValidation;

namespace B2BCommerce.Backend.Application.Validators.Integration;

public class CreateApiClientValidator : AbstractValidator<CreateApiClientDto>
{
    public CreateApiClientValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Client name is required")
            .MaximumLength(100).WithMessage("Client name cannot exceed 100 characters");

        RuleFor(x => x.ContactEmail)
            .NotEmpty().WithMessage("Contact email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.ContactPhone)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .Matches(@"^[\d\s\+\-\(\)]*$").When(x => !string.IsNullOrEmpty(x.ContactPhone))
            .WithMessage("Invalid phone number format");
    }
}
