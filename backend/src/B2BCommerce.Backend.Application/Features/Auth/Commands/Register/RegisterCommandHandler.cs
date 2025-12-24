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
            // Required fields
            CompanyName = request.CompanyName,
            TaxNumber = request.TaxNumber,
            TaxOffice = request.TaxOffice,
            Email = request.Email,
            Phone = request.Phone,
            ContactPersonName = request.ContactPersonName,
            ContactPersonTitle = request.ContactPersonTitle,
            // Optional company info
            TradeName = request.TradeName ?? string.Empty,
            MersisNo = request.MersisNo,
            IdentityNo = request.IdentityNo,
            TradeRegistryNo = request.TradeRegistryNo,
            MobilePhone = request.MobilePhone,
            Fax = request.Fax,
            Website = request.Website,
            // Financial info with defaults
            CreditLimit = request.CreditLimit ?? 0,
            Currency = request.Currency ?? "TRY",
            Type = request.Type
        };

        return await _authService.RegisterAsync(registerDto, cancellationToken);
    }
}
