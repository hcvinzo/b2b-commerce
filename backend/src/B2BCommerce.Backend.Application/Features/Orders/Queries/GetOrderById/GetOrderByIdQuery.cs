using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Orders;

namespace B2BCommerce.Backend.Application.Features.Orders.Queries.GetOrderById;

/// <summary>
/// Query to get an order by ID
/// </summary>
public record GetOrderByIdQuery(Guid Id) : IQuery<Result<OrderDto>>;
