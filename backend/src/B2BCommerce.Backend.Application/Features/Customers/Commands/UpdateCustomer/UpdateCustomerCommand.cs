using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;

namespace B2BCommerce.Backend.Application.Features.Customers.Commands.UpdateCustomer;

/// <summary>
/// Command to update customer information
/// </summary>
public record UpdateCustomerCommand(
    Guid Id,
    string CompanyName,
    string TradeName,
    string TaxOffice,
    string? MersisNo,
    string? IdentityNo,
    string? TradeRegistryNo,
    string Phone,
    string? MobilePhone,
    string? Fax,
    string? Website,
    string ContactPersonName,
    string ContactPersonTitle,
    string PreferredLanguage) : ICommand<Result<CustomerDto>>;
