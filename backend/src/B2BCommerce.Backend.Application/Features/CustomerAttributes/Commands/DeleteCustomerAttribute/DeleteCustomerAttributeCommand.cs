using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;

namespace B2BCommerce.Backend.Application.Features.CustomerAttributes.Commands.DeleteCustomerAttribute;

/// <summary>
/// Command to delete a customer attribute by ID
/// </summary>
public record DeleteCustomerAttributeCommand(Guid Id) : ICommand<Result>;
