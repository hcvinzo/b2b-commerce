using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Auth.Commands.Register;

/// <summary>
/// Handler for RegisterCommand
/// </summary>
public class RegisterCommandHandler : ICommandHandler<RegisterCommand, Result<CustomerDto>>
{
    private readonly IAuthService _authService;

    public RegisterCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<CustomerDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var registerDto = new RegisterCustomerDto
        {
            // Company info
            Title = request.Title,
            TaxOffice = request.TaxOffice,
            TaxNo = request.TaxNo,
            EstablishmentYear = request.EstablishmentYear,
            Website = request.Website,
            // Primary contact info
            ContactFirstName = request.ContactFirstName,
            ContactLastName = request.ContactLastName,
            ContactEmail = request.ContactEmail,
            ContactPosition = request.ContactPosition,
            ContactPhone = request.ContactPhone,
            ContactGsm = request.ContactGsm,
            // Primary address info
            AddressTitle = request.AddressTitle ?? string.Empty,
            Address = request.Address ?? string.Empty,
            GeoLocationId = request.GeoLocationId,
            PostalCode = request.PostalCode,
            // User account
            Email = request.Email,
            Password = request.Password,
            PasswordConfirmation = request.PasswordConfirmation,
            AcceptTerms = request.AcceptTerms,
            AcceptKvkk = request.AcceptKvkk,
            // Customer attributes
            Attributes = request.Attributes
        };

        return await _authService.RegisterAsync(registerDto, cancellationToken);
    }
}
