using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;

namespace B2BCommerce.Backend.Application.Features.Customers.Commands.DeactivateCustomer;

/// <summary>
/// Command to deactivate a customer account
/// </summary>
public record DeactivateCustomerCommand(Guid Id) : ICommand<Result>;
