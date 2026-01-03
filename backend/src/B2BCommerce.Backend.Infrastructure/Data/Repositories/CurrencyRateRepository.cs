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
    public async Task<IEnumerable<CurrencyRate>> GetActiveRatesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(cr => cr.IsActive && !cr.IsDeleted)
            .OrderBy(cr => cr.FromCurrency)
            .ThenBy(cr => cr.ToCurrency)
            .ThenByDescending(cr => cr.EffectiveDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all currency rates with optional filtering
    /// </summary>
    public async Task<IEnumerable<CurrencyRate>> GetRatesAsync(
        string? fromCurrency = null,
        string? toCurrency = null,
        bool? activeOnly = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().Where(cr => !cr.IsDeleted);

        if (!string.IsNullOrWhiteSpace(fromCurrency))
        {
            query = query.Where(cr => cr.FromCurrency == fromCurrency.ToUpperInvariant());
        }

        if (!string.IsNullOrWhiteSpace(toCurrency))
        {
            query = query.Where(cr => cr.ToCurrency == toCurrency.ToUpperInvariant());
        }

        if (activeOnly.HasValue)
        {
            query = query.Where(cr => cr.IsActive == activeOnly.Value);
        }

        return await query
            .OrderBy(cr => cr.FromCurrency)
            .ThenBy(cr => cr.ToCurrency)
            .ThenByDescending(cr => cr.EffectiveDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a currency rate by ID for update (with tracking)
    /// </summary>
    public async Task<CurrencyRate?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(cr => cr.Id == id && !cr.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Checks if a rate already exists for the given currency pair
    /// </summary>
    public async Task<bool> ExistsAsync(
        string fromCurrency,
        string toCurrency,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(cr =>
            cr.FromCurrency == fromCurrency.ToUpperInvariant() &&
            cr.ToCurrency == toCurrency.ToUpperInvariant() &&
            !cr.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(cr => cr.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
