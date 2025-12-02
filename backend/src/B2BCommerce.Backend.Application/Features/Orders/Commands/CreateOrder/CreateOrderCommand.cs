using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Orders;

namespace B2BCommerce.Backend.Application.Features.Orders.Commands.CreateOrder;

/// <summary>
/// Command to create a new order
/// </summary>
public record CreateOrderCommand(
    Guid CustomerId,
    string ShippingStreet,
    string ShippingCity,
    string ShippingState,
    string ShippingCountry,
    string ShippingPostalCode,
    string Currency,
    string? CustomerNote,
    string? ShippingNote,
    List<CreateOrderItemDto> OrderItems,
    decimal? DiscountAmount,
    decimal? ShippingCost) : ICommand<Result<OrderDto>>;
