using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;

namespace B2BCommerce.Backend.Application.Features.CustomerDocuments.Queries.GetCustomerDocument;

/// <summary>
/// Query to get a single customer document by ID
/// </summary>
public record GetCustomerDocumentQuery(Guid CustomerId, Guid DocumentId) : IQuery<Result<CustomerDocumentDto>>;
