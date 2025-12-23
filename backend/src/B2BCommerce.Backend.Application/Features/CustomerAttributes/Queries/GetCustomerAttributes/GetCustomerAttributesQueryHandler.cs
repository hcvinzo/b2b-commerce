using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.CustomerAttributes.Queries.GetCustomerAttributes;

/// <summary>
/// Handler for GetCustomerAttributesQuery
/// </summary>
public class GetCustomerAttributesQueryHandler : IQueryHandler<GetCustomerAttributesQuery, Result<IEnumerable<CustomerAttributeDto>>>
{
    private readonly ICustomerAttributeService _customerAttributeService;

    public GetCustomerAttributesQueryHandler(ICustomerAttributeService customerAttributeService)
    {
        _customerAttributeService = customerAttributeService;
    }

    public async Task<Result<IEnumerable<CustomerAttributeDto>>> Handle(
        GetCustomerAttributesQuery request,
        CancellationToken cancellationToken)
    {
        if (request.AttributeType.HasValue)
        {
            return await _customerAttributeService.GetByCustomerIdAndTypeAsync(
                request.CustomerId,
                request.AttributeType.Value,
                cancellationToken);
        }

        return await _customerAttributeService.GetByCustomerIdAsync(
            request.CustomerId,
            cancellationToken);
    }
}
