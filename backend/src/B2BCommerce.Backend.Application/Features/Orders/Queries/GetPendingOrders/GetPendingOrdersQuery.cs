using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Orders;

namespace B2BCommerce.Backend.Application.Features.Orders.Queries.GetPendingOrders;

/// <summary>
/// Query to get orders pending approval
/// </summary>
public record GetPendingOrdersQuery() : IQuery<Result<IEnumerable<OrderDto>>>;
