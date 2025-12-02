using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Orders;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Orders.Commands.MarkAsShipped;

/// <summary>
/// Handler for MarkAsShippedCommand
/// </summary>
public class MarkAsShippedCommandHandler : ICommandHandler<MarkAsShippedCommand, Result<OrderDto>>
{
    private readonly IOrderService _orderService;

    public MarkAsShippedCommandHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<Result<OrderDto>> Handle(MarkAsShippedCommand request, CancellationToken cancellationToken)
    {
        return await _orderService.MarkAsShippedAsync(request.Id, cancellationToken);
    }
}
