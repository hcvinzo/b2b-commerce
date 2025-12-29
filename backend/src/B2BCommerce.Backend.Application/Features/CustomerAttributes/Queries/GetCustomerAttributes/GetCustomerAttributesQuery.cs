using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;

namespace B2BCommerce.Backend.Application.Features.CustomerAttributes.Queries.GetCustomerAttributes;

/// <summary>
/// Query to get customer attributes by customer ID, optionally filtered by definition
/// </summary>
public record GetCustomerAttributesQuery(
    Guid CustomerId,
    Guid? AttributeDefinitionId = null) : IQuery<Result<IEnumerable<CustomerAttributeDto>>>;
