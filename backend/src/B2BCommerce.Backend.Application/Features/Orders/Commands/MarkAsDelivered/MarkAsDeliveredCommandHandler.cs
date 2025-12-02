using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Orders;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Orders.Commands.MarkAsDelivered;

/// <summary>
/// Handler for MarkAsDeliveredCommand
/// </summary>
public class MarkAsDeliveredCommandHandler : ICommandHandler<MarkAsDeliveredCommand, Result<OrderDto>>
{
    private readonly IOrderService _orderService;

    public MarkAsDeliveredCommandHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<Result<OrderDto>> Handle(MarkAsDeliveredCommand request, CancellationToken cancellationToken)
    {
        return await _orderService.MarkAsDeliveredAsync(request.Id, cancellationToken);
    }
}
