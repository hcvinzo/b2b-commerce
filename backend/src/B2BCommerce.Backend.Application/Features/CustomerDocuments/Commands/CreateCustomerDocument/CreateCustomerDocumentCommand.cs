using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;

namespace B2BCommerce.Backend.Application.Features.CustomerDocuments.Commands.CreateCustomerDocument;

/// <summary>
/// Command to create a new customer document
/// </summary>
public record CreateCustomerDocumentCommand(
    Guid CustomerId,
    string DocumentType,
    string FileName,
    string FileType,
    string ContentUrl,
    long FileSize) : ICommand<Result<CustomerDocumentDto>>;
