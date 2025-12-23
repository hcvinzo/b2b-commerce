using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.Features.CustomerAttributes.Commands.UpsertCustomerAttributes;

/// <summary>
/// Command to upsert customer attributes by type (replaces all existing attributes of the specified type)
/// </summary>
public record UpsertCustomerAttributesCommand(
    Guid CustomerId,
    CustomerAttributeType AttributeType,
    List<CustomerAttributeItemDto> Items) : ICommand<Result<IEnumerable<CustomerAttributeDto>>>;
