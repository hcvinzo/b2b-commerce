using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Customers.Queries.GetCustomerByEmail;

/// <summary>
/// Handler for GetCustomerByEmailQuery
/// </summary>
public class GetCustomerByEmailQueryHandler : IQueryHandler<GetCustomerByEmailQuery, Result<CustomerDto>>
{
    private readonly ICustomerService _customerService;

    public GetCustomerByEmailQueryHandler(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task<Result<CustomerDto>> Handle(GetCustomerByEmailQuery request, CancellationToken cancellationToken)
    {
        return await _customerService.GetByEmailAsync(request.Email, cancellationToken);
    }
}
