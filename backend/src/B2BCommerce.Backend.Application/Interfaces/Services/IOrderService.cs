using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Orders;

namespace B2BCommerce.Backend.Application.Interfaces.Services;

/// <summary>
/// Service interface for order operations
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Get order by ID
    /// </summary>
    Task<Result<OrderDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get order by order number
    /// </summary>
    Task<Result<OrderDto>> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all orders with pagination
    /// </summary>
    Task<Result<PagedResult<OrderDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get orders for a specific customer
    /// </summary>
    Task<Result<IEnumerable<OrderDto>>> GetByCustomerAsync(Guid customerId, string? status = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get orders pending approval
    /// </summary>
    Task<Result<IEnumerable<OrderDto>>> GetPendingOrdersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new order
    /// </summary>
    Task<Result<OrderDto>> CreateAsync(CreateOrderDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add item to existing order
    /// </summary>
    Task<Result<OrderDto>> AddOrderItemAsync(Guid orderId, CreateOrderItemDto item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove item from order
    /// </summary>
    Task<Result<OrderDto>> RemoveOrderItemAsync(Guid orderId, Guid orderItemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update order item quantity
    /// </summary>
    Task<Result<OrderDto>> UpdateOrderItemQuantityAsync(Guid orderId, Guid orderItemId, int newQuantity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approve an order
    /// </summary>
    Task<Result<OrderDto>> ApproveAsync(Guid id, string approvedBy, decimal? exchangeRate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reject an order
    /// </summary>
    Task<Result<OrderDto>> RejectAsync(Guid id, string rejectedBy, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel an order
    /// </summary>
    Task<Result<OrderDto>> CancelAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark order as processing
    /// </summary>
    Task<Result<OrderDto>> MarkAsProcessingAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark order as shipped
    /// </summary>
    Task<Result<OrderDto>> MarkAsShippedAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark order as delivered
    /// </summary>
    Task<Result<OrderDto>> MarkAsDeliveredAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set internal note on order
    /// </summary>
    Task<Result<OrderDto>> SetInternalNoteAsync(Guid id, string note, CancellationToken cancellationToken = default);
}
