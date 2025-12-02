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
            CompanyName = request.CompanyName,
            TaxNumber = request.TaxNumber,
            Email = request.Email,
            Phone = request.Phone,
            ContactPersonName = request.ContactPersonName,
            ContactPersonTitle = request.ContactPersonTitle,
            BillingStreet = request.BillingStreet,
            BillingCity = request.BillingCity,
            BillingState = request.BillingState,
            BillingCountry = request.BillingCountry,
            BillingPostalCode = request.BillingPostalCode,
            ShippingStreet = request.ShippingStreet,
            ShippingCity = request.ShippingCity,
            ShippingState = request.ShippingState,
            ShippingCountry = request.ShippingCountry,
            ShippingPostalCode = request.ShippingPostalCode,
            CreditLimit = request.CreditLimit,
            Currency = request.Currency,
            Type = request.Type,
            Password = request.Password
        };

        return await _authService.RegisterAsync(registerDto, cancellationToken);
    }
}
