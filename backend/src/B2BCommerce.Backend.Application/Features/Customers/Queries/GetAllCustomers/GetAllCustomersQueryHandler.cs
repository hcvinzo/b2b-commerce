using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Customers.Queries.GetAllCustomers;

/// <summary>
/// Handler for GetAllCustomersQuery
/// </summary>
public class GetAllCustomersQueryHandler : IQueryHandler<GetAllCustomersQuery, Result<PagedResult<CustomerDto>>>
{
    private readonly ICustomerService _customerService;

    public GetAllCustomersQueryHandler(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task<Result<PagedResult<CustomerDto>>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        return await _customerService.GetAllAsync(
            request.PageNumber,
            request.PageSize,
            request.Search,
            request.IsActive,
            request.Status,
            request.SortBy,
            request.SortDirection,
            cancellationToken);
    }
}
