using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Orders;

namespace B2BCommerce.Backend.Application.Features.Orders.Commands.AddOrderItem;

/// <summary>
/// Command to add an item to an existing order
/// </summary>
public record AddOrderItemCommand(
    Guid OrderId,
    Guid ProductId,
    int Quantity,
    decimal? DiscountAmount) : ICommand<Result<OrderDto>>;
