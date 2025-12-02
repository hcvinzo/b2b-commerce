using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;

namespace B2BCommerce.Backend.Application.Features.Customers.Commands.ActivateCustomer;

/// <summary>
/// Command to activate a customer account
/// </summary>
public record ActivateCustomerCommand(Guid Id) : ICommand<Result>;
