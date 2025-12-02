using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace B2BCommerce.Backend.Infrastructure.Data.Repositories;

/// <summary>
/// Order repository implementation for order-specific operations
/// </summary>
public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets an order by its order number
    /// </summary>
    /// <param name="orderNumber">Order number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Order if found, null otherwise</returns>
    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Payment)
            .Include(o => o.Shipment)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && !o.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets all orders for a specific customer
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of orders</returns>
    public async Task<IEnumerable<Order>> GetByCustomerAsync(
        Guid customerId,
        OrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.CustomerId == customerId && !o.IsDeleted);

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        return await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all orders pending approval
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of pending orders</returns>
    public async Task<IEnumerable<Order>> GetPendingOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.ApprovalStatus == OrderApprovalStatus.PendingApproval && !o.IsDeleted)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
