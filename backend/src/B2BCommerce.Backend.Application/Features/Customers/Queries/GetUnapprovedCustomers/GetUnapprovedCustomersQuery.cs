using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;

namespace B2BCommerce.Backend.Application.Features.Customers.Queries.GetUnapprovedCustomers;

/// <summary>
/// Query to get customers pending approval
/// </summary>
public record GetUnapprovedCustomersQuery() : IQuery<Result<IEnumerable<CustomerDto>>>;
