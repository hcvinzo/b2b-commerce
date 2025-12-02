using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Customers;

namespace B2BCommerce.Backend.Application.Features.Customers.Queries.GetCustomerById;

/// <summary>
/// Query to get a customer by ID
/// </summary>
public record GetCustomerByIdQuery(Guid Id) : IQuery<Result<CustomerDto>>;
