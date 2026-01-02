using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace B2BCommerce.Backend.Infrastructure.Data.Repositories;

/// <summary>
/// Currency repository implementation for currency-specific operations
/// </summary>
public class CurrencyRepository : GenericRepository<Currency>, ICurrencyRepository
{
    public CurrencyRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets a currency by its ISO code
    /// </summary>
    public async Task<Currency?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var upperCode = code.ToUpperInvariant();
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Code == upperCode && !c.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Checks if a currency exists by its ISO code
    /// </summary>
    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var upperCode = code.ToUpperInvariant();
        return await _dbSet.AnyAsync(c => c.Code == upperCode && !c.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets the default currency
    /// </summary>
    public async Task<Currency?> GetDefaultAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.IsDefault && !c.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets all active currencies ordered by display order
    /// </summary>
    public async Task<IEnumerable<Currency>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Code)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all currencies ordered by display order
    /// </summary>
    public async Task<IEnumerable<Currency>> GetAllOrderedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Code)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Clears the default flag from all currencies
    /// </summary>
    public async Task ClearAllDefaultsAsync(CancellationToken cancellationToken = default)
    {
        await _dbSet
            .Where(c => c.IsDefault && !c.IsDeleted)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsDefault, false), cancellationToken);
    }
}
