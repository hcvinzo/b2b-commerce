using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace B2BCommerce.Backend.Infrastructure.Data.Repositories;

/// <summary>
/// Currency rate repository implementation for currency rate operations
/// </summary>
public class CurrencyRateRepository : GenericRepository<CurrencyRate>, ICurrencyRateRepository
{
    public CurrencyRateRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets the exchange rate between two currencies
    /// </summary>
    /// <param name="fromCurrency">Source currency code</param>
    /// <param name="toCurrency">Target currency code</param>
    /// <param name="effectiveDate">Optional date for historical rates (uses current date if not specified)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Currency rate if found, null otherwise</returns>
    public async Task<CurrencyRate?> GetRateAsync(
        string fromCurrency,
        string toCurrency,
        DateTime? effectiveDate = null,
        CancellationToken cancellationToken = default)
    {
        var targetDate = effectiveDate ?? DateTime.UtcNow;

        return await _dbSet
            .Where(cr => cr.FromCurrency == fromCurrency &&
                         cr.ToCurrency == toCurrency &&
                         cr.IsActive &&
                         !cr.IsDeleted &&
                         cr.EffectiveDate <= targetDate)
            .OrderByDescending(cr => cr.EffectiveDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all active currency rates
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active currency rates</returns>
    public async Task<IEnumerable<CurrencyRate>> GetActiveRatesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(cr => cr.IsActive && !cr.IsDeleted)
            .OrderBy(cr => cr.FromCurrency)
            .ThenBy(cr => cr.ToCurrency)
            .ThenByDescending(cr => cr.EffectiveDate)
            .ToListAsync(cancellationToken);
    }
}
