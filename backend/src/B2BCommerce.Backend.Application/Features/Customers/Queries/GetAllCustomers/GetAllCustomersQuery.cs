using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.Features.Customers.Queries.GetAllCustomers;

/// <summary>
/// Query to get all customers with pagination, search, and filtering
/// </summary>
public record GetAllCustomersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Search = null,
    bool? IsActive = null,
    CustomerStatus? Status = null,
    string? SortBy = null,
    string? SortDirection = null) : IQuery<Result<PagedResult<CustomerDto>>>;
