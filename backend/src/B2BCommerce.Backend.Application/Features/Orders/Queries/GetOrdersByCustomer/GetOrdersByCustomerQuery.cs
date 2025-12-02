using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Orders;

namespace B2BCommerce.Backend.Application.Features.Orders.Queries.GetOrdersByCustomer;

/// <summary>
/// Query to get orders for a specific customer
/// </summary>
public record GetOrdersByCustomerQuery(Guid CustomerId, string? Status = null) : IQuery<Result<IEnumerable<OrderDto>>>;
