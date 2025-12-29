using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.Interfaces.Repositories;

/// <summary>
/// Customer repository interface for customer-specific operations
/// </summary>
public interface ICustomerRepository : IGenericRepository<Customer>
{
    /// <summary>
    /// Gets a customer by external ID (primary upsert key)
    /// </summary>
    Task<Customer?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a customer exists by external ID
    /// </summary>
    Task<bool> ExistsByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a customer by tax number
    /// </summary>
    Task<Customer?> GetByTaxNoAsync(string taxNo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a customer exists by tax number
    /// </summary>
    Task<bool> ExistsByTaxNoAsync(string taxNo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets customers by status
    /// </summary>
    Task<IEnumerable<Customer>> GetByStatusAsync(CustomerStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a customer by user ID
    /// </summary>
    Task<Customer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all pending customers (awaiting approval)
    /// </summary>
    Task<IEnumerable<Customer>> GetPendingCustomersAsync(CancellationToken cancellationToken = default);
}
