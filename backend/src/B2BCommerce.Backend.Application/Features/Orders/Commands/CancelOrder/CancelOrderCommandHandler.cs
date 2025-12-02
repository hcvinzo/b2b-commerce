using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Orders;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Orders.Commands.CancelOrder;

/// <summary>
/// Handler for CancelOrderCommand
/// </summary>
public class CancelOrderCommandHandler : ICommandHandler<CancelOrderCommand, Result<OrderDto>>
{
    private readonly IOrderService _orderService;

    public CancelOrderCommandHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<Result<OrderDto>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        return await _orderService.CancelAsync(request.Id, cancellationToken);
    }
}
