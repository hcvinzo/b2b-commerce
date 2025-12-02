using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Orders;

namespace B2BCommerce.Backend.Application.Features.Orders.Commands.RemoveOrderItem;

/// <summary>
/// Command to remove an item from an order
/// </summary>
public record RemoveOrderItemCommand(Guid OrderId, Guid OrderItemId) : ICommand<Result<OrderDto>>;
