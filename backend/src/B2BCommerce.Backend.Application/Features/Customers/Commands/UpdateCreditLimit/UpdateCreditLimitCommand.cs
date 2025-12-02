using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;

namespace B2BCommerce.Backend.Application.Features.Customers.Commands.UpdateCreditLimit;

/// <summary>
/// Command to update customer credit limit
/// </summary>
public record UpdateCreditLimitCommand(Guid Id, decimal NewCreditLimit) : ICommand<Result<CustomerDto>>;
