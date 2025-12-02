using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Orders;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Orders.Queries.GetOrderById;

/// <summary>
/// Handler for GetOrderByIdQuery
/// </summary>
public class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    private readonly IOrderService _orderService;

    public GetOrderByIdQueryHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        return await _orderService.GetByIdAsync(request.Id, cancellationToken);
    }
}
