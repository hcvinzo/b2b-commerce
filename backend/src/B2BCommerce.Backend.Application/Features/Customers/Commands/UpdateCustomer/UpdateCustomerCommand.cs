using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;

namespace B2BCommerce.Backend.Application.Features.Customers.Commands.UpdateCustomer;

/// <summary>
/// Command to update customer information
/// </summary>
public record UpdateCustomerCommand(
    Guid Id,
    string Title,
    string? TaxOffice,
    string? TaxNo,
    int? EstablishmentYear,
    string? Website) : ICommand<Result<CustomerDto>>;
