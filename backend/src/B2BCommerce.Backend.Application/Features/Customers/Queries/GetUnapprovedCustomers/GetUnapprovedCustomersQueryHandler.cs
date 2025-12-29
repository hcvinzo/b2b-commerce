using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Customers.Queries.GetUnapprovedCustomers;

/// <summary>
/// Handler for GetUnapprovedCustomersQuery
/// </summary>
public class GetUnapprovedCustomersQueryHandler : IQueryHandler<GetUnapprovedCustomersQuery, Result<IEnumerable<CustomerDto>>>
{
    private readonly ICustomerService _customerService;

    public GetUnapprovedCustomersQueryHandler(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task<Result<IEnumerable<CustomerDto>>> Handle(GetUnapprovedCustomersQuery request, CancellationToken cancellationToken)
    {
        return await _customerService.GetPendingCustomersAsync(cancellationToken);
    }
}
