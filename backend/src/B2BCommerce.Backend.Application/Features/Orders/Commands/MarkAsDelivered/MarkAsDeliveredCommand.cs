using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Orders;

namespace B2BCommerce.Backend.Application.Features.Orders.Commands.MarkAsDelivered;

/// <summary>
/// Command to mark order as delivered
/// </summary>
public record MarkAsDeliveredCommand(Guid Id) : ICommand<Result<OrderDto>>;
