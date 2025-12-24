using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;

namespace B2BCommerce.Backend.Application.Features.Auth.Commands.Register;

/// <summary>
/// Command to register a new customer (dealer application)
/// Password and addresses are not required at registration -
/// password is set after admin approval, addresses are added later
/// </summary>
public record RegisterCommand(
    // Required company info
    string CompanyName,
    string TaxNumber,
    string TaxOffice,
    string Email,
    string Phone,
    string ContactPersonName,
    string ContactPersonTitle,
    // Optional company info
    string? TradeName,
    string? MersisNo,
    string? IdentityNo,
    string? TradeRegistryNo,
    string? MobilePhone,
    string? Fax,
    string? Website,
    // Optional financial info (defaults will be applied)
    decimal? CreditLimit,
    string? Currency,
    string? Type
) : ICommand<Result<CustomerDto>>;
