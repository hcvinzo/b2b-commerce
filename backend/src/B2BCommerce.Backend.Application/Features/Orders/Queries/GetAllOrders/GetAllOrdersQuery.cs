using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Orders;

namespace B2BCommerce.Backend.Application.Features.Orders.Queries.GetAllOrders;

/// <summary>
/// Query to get all orders with pagination
/// </summary>
public record GetAllOrdersQuery(int PageNumber = 1, int PageSize = 10) : IQuery<Result<PagedResult<OrderDto>>>;
