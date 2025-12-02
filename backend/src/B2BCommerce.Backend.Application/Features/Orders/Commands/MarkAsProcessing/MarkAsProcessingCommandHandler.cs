using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Orders;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Orders.Commands.MarkAsProcessing;

/// <summary>
/// Handler for MarkAsProcessingCommand
/// </summary>
public class MarkAsProcessingCommandHandler : ICommandHandler<MarkAsProcessingCommand, Result<OrderDto>>
{
    private readonly IOrderService _orderService;

    public MarkAsProcessingCommandHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<Result<OrderDto>> Handle(MarkAsProcessingCommand request, CancellationToken cancellationToken)
    {
        return await _orderService.MarkAsProcessingAsync(request.Id, cancellationToken);
    }
}
