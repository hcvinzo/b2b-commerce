using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Customers.Commands.UpdateCustomer;

/// <summary>
/// Handler for UpdateCustomerCommand
/// </summary>
public class UpdateCustomerCommandHandler : ICommandHandler<UpdateCustomerCommand, Result<CustomerDto>>
{
    private readonly ICustomerService _customerService;

    public UpdateCustomerCommandHandler(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task<Result<CustomerDto>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var updateDto = new UpdateCustomerDto
        {
            CompanyName = request.CompanyName,
            TradeName = request.TradeName,
            TaxOffice = request.TaxOffice,
            MersisNo = request.MersisNo,
            IdentityNo = request.IdentityNo,
            TradeRegistryNo = request.TradeRegistryNo,
            Phone = request.Phone,
            MobilePhone = request.MobilePhone,
            Fax = request.Fax,
            Website = request.Website,
            ContactPersonName = request.ContactPersonName,
            ContactPersonTitle = request.ContactPersonTitle,
            PreferredLanguage = request.PreferredLanguage
        };

        return await _customerService.UpdateAsync(request.Id, updateDto, cancellationToken);
    }
}
