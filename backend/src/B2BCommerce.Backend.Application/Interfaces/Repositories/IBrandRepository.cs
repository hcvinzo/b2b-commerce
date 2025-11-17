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
}
