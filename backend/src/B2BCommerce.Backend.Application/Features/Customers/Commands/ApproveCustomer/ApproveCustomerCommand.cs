using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;

namespace B2BCommerce.Backend.Application.Features.Customers.Commands.ApproveCustomer;

/// <summary>
/// Command to approve a customer account
/// </summary>
public record ApproveCustomerCommand(Guid Id) : ICommand<Result<CustomerDto>>;
