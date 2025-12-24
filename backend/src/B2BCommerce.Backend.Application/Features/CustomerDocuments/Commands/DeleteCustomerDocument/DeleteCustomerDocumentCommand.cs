using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;

namespace B2BCommerce.Backend.Application.Features.CustomerDocuments.Commands.DeleteCustomerDocument;

/// <summary>
/// Command to delete a customer document
/// </summary>
public record DeleteCustomerDocumentCommand(
    Guid CustomerId,
    Guid DocumentId) : ICommand<Result<bool>>;
