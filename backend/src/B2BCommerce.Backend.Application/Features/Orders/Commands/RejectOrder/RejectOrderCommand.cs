using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Orders;

namespace B2BCommerce.Backend.Application.Features.Orders.Commands.RejectOrder;

/// <summary>
/// Command to reject an order
/// </summary>
public record RejectOrderCommand(Guid Id, string RejectedBy, string Reason) : ICommand<Result<OrderDto>>;
