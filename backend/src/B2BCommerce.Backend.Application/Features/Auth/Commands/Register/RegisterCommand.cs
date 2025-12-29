using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;

namespace B2BCommerce.Backend.Application.Features.Auth.Commands.Register;

/// <summary>
/// Command to register a new customer (dealer application)
/// </summary>
public record RegisterCommand(
    // Company info
    string Title,
    string? TaxOffice,
    string? TaxNo,
    int? EstablishmentYear,
    string? Website,
    // Primary contact info
    string ContactFirstName,
    string ContactLastName,
    string ContactEmail,
    string? ContactPosition,
    string? ContactPhone,
    string? ContactGsm,
    // Primary address info
    string? AddressTitle,
    string? Address,
    Guid? GeoLocationId,
    string? PostalCode,
    // User account
    string Email,
    string Password,
    string PasswordConfirmation,
    bool AcceptTerms,
    bool AcceptKvkk,
    // Optional customer attributes (collected during registration)
    List<UpsertCustomerAttributesByDefinitionDto>? Attributes = null
) : ICommand<Result<CustomerDto>>;
