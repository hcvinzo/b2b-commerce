using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Orders;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Orders.Queries.GetPendingOrders;

/// <summary>
/// Handler for GetPendingOrdersQuery
/// </summary>
public class GetPendingOrdersQueryHandler : IQueryHandler<GetPendingOrdersQuery, Result<IEnumerable<OrderDto>>>
{
    private readonly IOrderService _orderService;

    public GetPendingOrdersQueryHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<Result<IEnumerable<OrderDto>>> Handle(GetPendingOrdersQuery request, CancellationToken cancellationToken)
    {
        return await _orderService.GetPendingOrdersAsync(cancellationToken);
    }
}
