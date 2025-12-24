using B2BCommerce.Backend.Domain.Enums;
using FluentValidation;

namespace B2BCommerce.Backend.Application.Features.CustomerDocuments.Commands.CreateCustomerDocument;

/// <summary>
/// Validator for CreateCustomerDocumentCommand
/// </summary>
public class CreateCustomerDocumentCommandValidator : AbstractValidator<CreateCustomerDocumentCommand>
{
    public CreateCustomerDocumentCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required");

        RuleFor(x => x.DocumentType)
            .NotEmpty()
            .WithMessage("Document type is required")
            .Must(BeValidDocumentType)
            .WithMessage("Invalid document type. Valid types: TaxCertificate, SignatureCircular, TradeRegistry, PartnershipAgreement, AuthorizedIdCopy, AuthorizedResidenceDocument");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required")
            .MaximumLength(255)
            .WithMessage("File name cannot exceed 255 characters");

        RuleFor(x => x.FileType)
            .NotEmpty()
            .WithMessage("File type is required")
            .MaximumLength(100)
            .WithMessage("File type cannot exceed 100 characters")
            .Must(BeValidFileType)
            .WithMessage("Invalid file type. Allowed types: application/pdf, image/jpeg, image/png");

        RuleFor(x => x.ContentUrl)
            .NotEmpty()
            .WithMessage("Content URL is required")
            .MaximumLength(500)
            .WithMessage("Content URL cannot exceed 500 characters");

        RuleFor(x => x.FileSize)
            .GreaterThan(0)
            .WithMessage("File size must be greater than 0")
            .LessThanOrEqualTo(10 * 1024 * 1024) // 10MB
            .WithMessage("File size cannot exceed 10MB");
    }

    private static bool BeValidDocumentType(string documentType)
    {
        return Enum.TryParse<CustomerDocumentType>(documentType, out _);
    }

    private static bool BeValidFileType(string fileType)
    {
        var allowedTypes = new[]
        {
            "application/pdf",
            "image/jpeg",
            "image/jpg",
            "image/png"
        };

        return allowedTypes.Contains(fileType.ToLowerInvariant());
    }
}
