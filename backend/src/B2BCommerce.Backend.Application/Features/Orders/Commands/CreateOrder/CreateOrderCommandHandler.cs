using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Orders;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Orders.Commands.CreateOrder;

/// <summary>
/// Handler for CreateOrderCommand
/// </summary>
public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Result<OrderDto>>
{
    private readonly IOrderService _orderService;

    public CreateOrderCommandHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var createDto = new CreateOrderDto
        {
            CustomerId = request.CustomerId,
            ShippingStreet = request.ShippingStreet,
            ShippingCity = request.ShippingCity,
            ShippingState = request.ShippingState,
            ShippingCountry = request.ShippingCountry,
            ShippingPostalCode = request.ShippingPostalCode,
            Currency = request.Currency,
            CustomerNote = request.CustomerNote,
            ShippingNote = request.ShippingNote,
            OrderItems = request.OrderItems,
            DiscountAmount = request.DiscountAmount,
            ShippingCost = request.ShippingCost
        };

        return await _orderService.CreateAsync(createDto, cancellationToken);
    }
}
