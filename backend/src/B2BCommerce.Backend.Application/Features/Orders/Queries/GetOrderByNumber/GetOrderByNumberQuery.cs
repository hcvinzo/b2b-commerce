using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Orders;

namespace B2BCommerce.Backend.Application.Features.Orders.Queries.GetOrderByNumber;

/// <summary>
/// Query to get an order by order number
/// </summary>
public record GetOrderByNumberQuery(string OrderNumber) : IQuery<Result<OrderDto>>;
