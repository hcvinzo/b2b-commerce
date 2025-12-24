using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;

namespace B2BCommerce.Backend.Application.Features.CustomerDocuments.Queries.GetCustomerDocuments;

/// <summary>
/// Query to get all documents for a customer
/// </summary>
public record GetCustomerDocumentsQuery(Guid CustomerId) : IQuery<Result<IEnumerable<CustomerDocumentDto>>>;
