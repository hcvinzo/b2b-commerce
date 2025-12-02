using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;

namespace B2BCommerce.Backend.Application.Features.Customers.Commands.DeleteCustomer;

/// <summary>
/// Command to delete a customer (soft delete)
/// </summary>
public record DeleteCustomerCommand(Guid Id) : ICommand<Result>;
