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
    string PreferredLanguage) : ICommand<Result<CustomerDto>>;
