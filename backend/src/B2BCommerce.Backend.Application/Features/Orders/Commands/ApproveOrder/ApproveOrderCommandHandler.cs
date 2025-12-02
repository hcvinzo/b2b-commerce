using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Orders;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Orders.Commands.ApproveOrder;

/// <summary>
/// Handler for ApproveOrderCommand
/// </summary>
public class ApproveOrderCommandHandler : ICommandHandler<ApproveOrderCommand, Result<OrderDto>>
{
    private readonly IOrderService _orderService;

    public ApproveOrderCommandHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<Result<OrderDto>> Handle(ApproveOrderCommand request, CancellationToken cancellationToken)
    {
        return await _orderService.ApproveAsync(request.Id, request.ApprovedBy, request.ExchangeRate, cancellationToken);
    }
}
