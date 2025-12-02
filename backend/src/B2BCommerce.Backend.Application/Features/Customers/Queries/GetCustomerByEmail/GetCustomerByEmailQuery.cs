using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;

namespace B2BCommerce.Backend.Application.Features.Customers.Queries.GetCustomerByEmail;

/// <summary>
/// Query to get a customer by email
/// </summary>
public record GetCustomerByEmailQuery(string Email) : IQuery<Result<CustomerDto>>;
