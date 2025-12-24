using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;

namespace B2BCommerce.Backend.Application.Features.CustomerDocuments.Commands.UpdateCustomerDocument;

/// <summary>
/// Command to update/replace an existing customer document
/// </summary>
public record UpdateCustomerDocumentCommand(
    Guid CustomerId,
    Guid DocumentId,
    string FileName,
    string FileType,
    string ContentUrl,
    long FileSize) : ICommand<Result<CustomerDocumentDto>>;
