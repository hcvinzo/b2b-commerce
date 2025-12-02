using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;

namespace B2BCommerce.Backend.Application.Features.Auth.Commands.Register;

/// <summary>
/// Command to register a new customer
/// </summary>
public record RegisterCommand(
    string CompanyName,
    string TaxNumber,
    string Email,
    string Phone,
    string ContactPersonName,
    string ContactPersonTitle,
    string BillingStreet,
    string BillingCity,
    string BillingState,
    string BillingCountry,
    string BillingPostalCode,
    string ShippingStreet,
    string ShippingCity,
    string ShippingState,
    string ShippingCountry,
    string ShippingPostalCode,
    decimal CreditLimit,
    string Currency,
    string? Type,
    string Password) : ICommand<Result<CustomerDto>>;
