using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Orders;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Orders.Commands.RejectOrder;

/// <summary>
/// Handler for RejectOrderCommand
/// </summary>
public class RejectOrderCommandHandler : ICommandHandler<RejectOrderCommand, Result<OrderDto>>
{
    private readonly IOrderService _orderService;

    public RejectOrderCommandHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<Result<OrderDto>> Handle(RejectOrderCommand request, CancellationToken cancellationToken)
    {
        return await _orderService.RejectAsync(request.Id, request.RejectedBy, request.Reason, cancellationToken);
    }
}
