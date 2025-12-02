using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Orders;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Orders.Commands.AddOrderItem;

/// <summary>
/// Handler for AddOrderItemCommand
/// </summary>
public class AddOrderItemCommandHandler : ICommandHandler<AddOrderItemCommand, Result<OrderDto>>
{
    private readonly IOrderService _orderService;

    public AddOrderItemCommandHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<Result<OrderDto>> Handle(AddOrderItemCommand request, CancellationToken cancellationToken)
    {
        var itemDto = new CreateOrderItemDto
        {
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            DiscountAmount = request.DiscountAmount
        };

        return await _orderService.AddOrderItemAsync(request.OrderId, itemDto, cancellationToken);
    }
}
