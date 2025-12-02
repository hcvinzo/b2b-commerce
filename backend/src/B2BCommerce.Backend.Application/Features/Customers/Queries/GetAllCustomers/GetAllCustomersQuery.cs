using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;

namespace B2BCommerce.Backend.Application.Features.Customers.Queries.GetAllCustomers;

/// <summary>
/// Query to get all customers with pagination
/// </summary>
public record GetAllCustomersQuery(int PageNumber = 1, int PageSize = 10) : IQuery<Result<PagedResult<CustomerDto>>>;
