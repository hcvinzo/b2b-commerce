using FluentValidation;

namespace B2BCommerce.Backend.Application.Features.CustomerDocuments.Commands.UpdateCustomerDocument;

/// <summary>
/// Validator for UpdateCustomerDocumentCommand
/// </summary>
public class UpdateCustomerDocumentCommandValidator : AbstractValidator<UpdateCustomerDocumentCommand>
{
    private static readonly string[] AllowedFileTypes = { "application/pdf", "image/jpeg", "image/jpg", "image/png" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public UpdateCustomerDocumentCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required");

        RuleFor(x => x.DocumentId)
            .NotEmpty()
            .WithMessage("Document ID is required");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required")
            .MaximumLength(255)
            .WithMessage("File name cannot exceed 255 characters");

        RuleFor(x => x.FileType)
            .NotEmpty()
            .WithMessage("File type is required")
            .Must(type => AllowedFileTypes.Contains(type.ToLowerInvariant()))
            .WithMessage("Only PDF, JPEG, and PNG files are allowed");

        RuleFor(x => x.ContentUrl)
            .NotEmpty()
            .WithMessage("Content URL is required")
            .MaximumLength(500)
            .WithMessage("Content URL cannot exceed 500 characters");

        RuleFor(x => x.FileSize)
            .GreaterThan(0)
            .WithMessage("File size must be greater than 0")
            .LessThanOrEqualTo(MaxFileSize)
            .WithMessage("File size cannot exceed 10MB");
    }
}
