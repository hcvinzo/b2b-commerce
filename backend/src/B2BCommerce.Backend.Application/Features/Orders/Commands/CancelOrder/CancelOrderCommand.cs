using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Orders;

namespace B2BCommerce.Backend.Application.Features.Orders.Commands.CancelOrder;

/// <summary>
/// Command to cancel an order
/// </summary>
public record CancelOrderCommand(Guid Id) : ICommand<Result<OrderDto>>;
