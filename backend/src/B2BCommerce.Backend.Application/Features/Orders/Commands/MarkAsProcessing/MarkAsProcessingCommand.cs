using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Orders;

namespace B2BCommerce.Backend.Application.Features.Orders.Commands.MarkAsProcessing;

/// <summary>
/// Command to mark order as processing
/// </summary>
public record MarkAsProcessingCommand(Guid Id) : ICommand<Result<OrderDto>>;
