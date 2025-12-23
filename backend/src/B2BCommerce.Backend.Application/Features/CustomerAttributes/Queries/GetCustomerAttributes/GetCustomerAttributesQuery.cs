using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.Features.CustomerAttributes.Queries.GetCustomerAttributes;

/// <summary>
/// Query to get customer attributes by customer ID, optionally filtered by type
/// </summary>
public record GetCustomerAttributesQuery(
    Guid CustomerId,
    CustomerAttributeType? AttributeType = null) : IQuery<Result<IEnumerable<CustomerAttributeDto>>>;
