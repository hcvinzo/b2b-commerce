using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Orders;

namespace B2BCommerce.Backend.Application.Features.Orders.Commands.UpdateOrderItemQuantity;

/// <summary>
/// Command to update order item quantity
/// </summary>
public record UpdateOrderItemQuantityCommand(Guid OrderId, Guid OrderItemId, int NewQuantity) : ICommand<Result<OrderDto>>;
