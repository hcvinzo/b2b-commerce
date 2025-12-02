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
            PreferredLanguage = request.PreferredLanguage
        };

        return await _customerService.UpdateAsync(request.Id, updateDto, cancellationToken);
    }
}
