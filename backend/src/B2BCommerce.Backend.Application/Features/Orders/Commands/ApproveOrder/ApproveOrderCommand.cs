using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Orders;

namespace B2BCommerce.Backend.Application.Features.Orders.Commands.ApproveOrder;

/// <summary>
/// Command to approve an order
/// </summary>
public record ApproveOrderCommand(Guid Id, string ApprovedBy, decimal? ExchangeRate = null) : ICommand<Result<OrderDto>>;
