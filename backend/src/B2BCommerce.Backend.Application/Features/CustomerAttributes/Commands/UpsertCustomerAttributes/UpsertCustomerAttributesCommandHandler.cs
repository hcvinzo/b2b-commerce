using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.CustomerAttributes.Commands.UpsertCustomerAttributes;

/// <summary>
/// Handler for UpsertCustomerAttributesCommand
/// </summary>
public class UpsertCustomerAttributesCommandHandler : ICommandHandler<UpsertCustomerAttributesCommand, Result<IEnumerable<CustomerAttributeDto>>>
{
    private readonly ICustomerAttributeService _customerAttributeService;

    public UpsertCustomerAttributesCommandHandler(ICustomerAttributeService customerAttributeService)
    {
        _customerAttributeService = customerAttributeService;
    }

    public async Task<Result<IEnumerable<CustomerAttributeDto>>> Handle(
        UpsertCustomerAttributesCommand request,
        CancellationToken cancellationToken)
    {
        var dto = new UpsertCustomerAttributesByTypeDto
        {
            AttributeType = request.AttributeType,
            Items = request.Items
        };

        return await _customerAttributeService.UpsertByTypeAsync(
            request.CustomerId,
            dto,
            cancellationToken);
    }
}
