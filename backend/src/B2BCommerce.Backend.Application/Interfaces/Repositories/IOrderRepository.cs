using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.Interfaces.Repositories;

/// <summary>
/// Order repository interface for order-specific operations
/// </summary>
public interface IOrderRepository : IGenericRepository<Order>
{
    /// <summary>
    /// Gets an order by its order number
    /// </summary>
    /// <param name="orderNumber">Order number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Order if found, null otherwise</returns>
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all orders for a specific customer
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of orders</returns>
    Task<IEnumerable<Order>> GetByCustomerAsync(
        Guid customerId,
        OrderStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all orders pending approval
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of pending orders</returns>
    Task<IEnumerable<Order>> GetPendingOrdersAsync(CancellationToken cancellationToken = default);
}
