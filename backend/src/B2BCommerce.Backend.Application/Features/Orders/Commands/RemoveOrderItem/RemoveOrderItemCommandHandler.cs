using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Orders;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Orders.Commands.RemoveOrderItem;

/// <summary>
/// Handler for RemoveOrderItemCommand
/// </summary>
public class RemoveOrderItemCommandHandler : ICommandHandler<RemoveOrderItemCommand, Result<OrderDto>>
{
    private readonly IOrderService _orderService;

    public RemoveOrderItemCommandHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<Result<OrderDto>> Handle(RemoveOrderItemCommand request, CancellationToken cancellationToken)
    {
        return await _orderService.RemoveOrderItemAsync(request.OrderId, request.OrderItemId, cancellationToken);
    }
}
