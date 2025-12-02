using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Orders;

namespace B2BCommerce.Backend.Application.Features.Orders.Commands.MarkAsShipped;

/// <summary>
/// Command to mark order as shipped
/// </summary>
public record MarkAsShippedCommand(Guid Id) : ICommand<Result<OrderDto>>;
