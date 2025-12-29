using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;

namespace B2BCommerce.Backend.Application.Features.CustomerAttributes.Commands.UpsertCustomerAttributes;

/// <summary>
/// Command to upsert customer attributes by definition (replaces all existing attributes of the specified definition)
/// </summary>
public record UpsertCustomerAttributesCommand(
    Guid CustomerId,
    Guid AttributeDefinitionId,
    List<CustomerAttributeItemDto> Items) : ICommand<Result<IEnumerable<CustomerAttributeDto>>>;
