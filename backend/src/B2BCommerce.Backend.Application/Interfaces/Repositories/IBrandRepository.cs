using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Application.Interfaces.Repositories;

/// <summary>
/// Brand repository interface for brand-specific operations
/// </summary>
public interface IBrandRepository : IGenericRepository<Brand>
{
    /// <summary>
    /// Gets all active brands
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active brands</returns>
    Task<IEnumerable<Brand>> GetActiveBrandsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a brand by its external ID (primary key for LOGO ERP integration)
    /// </summary>
    /// <param name="externalId">External system ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Brand if found, null otherwise</returns>
    Task<Brand?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a brand by its name
    /// </summary>
    /// <param name="name">Brand name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Brand if found, null otherwise</returns>
    Task<Brand?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a brand exists by its external ID
    /// </summary>
    /// <param name="externalId">External system ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> ExistsByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);
}
