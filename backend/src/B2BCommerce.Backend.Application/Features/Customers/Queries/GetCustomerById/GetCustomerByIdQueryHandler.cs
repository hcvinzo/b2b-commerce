using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Customers.Queries.GetCustomerById;

/// <summary>
/// Handler for GetCustomerByIdQuery
/// </summary>
public class GetCustomerByIdQueryHandler : IQueryHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    private readonly ICustomerService _customerService;

    public GetCustomerByIdQueryHandler(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        return await _customerService.GetByIdAsync(request.Id, cancellationToken);
    }
}
