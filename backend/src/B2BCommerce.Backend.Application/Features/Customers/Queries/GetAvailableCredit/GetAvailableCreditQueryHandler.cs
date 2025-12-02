using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Customers.Queries.GetAvailableCredit;

/// <summary>
/// Handler for GetAvailableCreditQuery
/// </summary>
public class GetAvailableCreditQueryHandler : IQueryHandler<GetAvailableCreditQuery, Result<decimal>>
{
    private readonly ICustomerService _customerService;

    public GetAvailableCreditQueryHandler(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task<Result<decimal>> Handle(GetAvailableCreditQuery request, CancellationToken cancellationToken)
    {
        return await _customerService.GetAvailableCreditAsync(request.CustomerId, cancellationToken);
    }
}
