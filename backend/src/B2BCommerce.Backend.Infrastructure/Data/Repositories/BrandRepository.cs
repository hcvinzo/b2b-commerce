using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace B2BCommerce.Backend.Infrastructure.Data.Repositories;

/// <summary>
/// Brand repository implementation for brand-specific operations
/// </summary>
public class BrandRepository : GenericRepository<Brand>, IBrandRepository
{
    public BrandRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets all active brands
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active brands</returns>
    public async Task<IEnumerable<Brand>> GetActiveBrandsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(b => b.IsActive && !b.IsDeleted)
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }
}
