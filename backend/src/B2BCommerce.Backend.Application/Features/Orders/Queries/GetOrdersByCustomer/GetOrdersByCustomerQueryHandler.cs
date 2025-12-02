using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Orders;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Orders.Queries.GetOrdersByCustomer;

/// <summary>
/// Handler for GetOrdersByCustomerQuery
/// </summary>
public class GetOrdersByCustomerQueryHandler : IQueryHandler<GetOrdersByCustomerQuery, Result<IEnumerable<OrderDto>>>
{
    private readonly IOrderService _orderService;

    public GetOrdersByCustomerQueryHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<Result<IEnumerable<OrderDto>>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        return await _orderService.GetByCustomerAsync(request.CustomerId, request.Status, cancellationToken);
    }
}
