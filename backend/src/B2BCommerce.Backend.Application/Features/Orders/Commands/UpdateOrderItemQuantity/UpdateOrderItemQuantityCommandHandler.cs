using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Orders;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Orders.Commands.UpdateOrderItemQuantity;

/// <summary>
/// Handler for UpdateOrderItemQuantityCommand
/// </summary>
public class UpdateOrderItemQuantityCommandHandler : ICommandHandler<UpdateOrderItemQuantityCommand, Result<OrderDto>>
{
    private readonly IOrderService _orderService;

    public UpdateOrderItemQuantityCommandHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<Result<OrderDto>> Handle(UpdateOrderItemQuantityCommand request, CancellationToken cancellationToken)
    {
        return await _orderService.UpdateOrderItemQuantityAsync(request.OrderId, request.OrderItemId, request.NewQuantity, cancellationToken);
    }
}
