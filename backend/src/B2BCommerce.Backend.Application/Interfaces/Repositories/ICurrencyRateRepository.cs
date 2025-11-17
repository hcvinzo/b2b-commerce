using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Application.Interfaces.Repositories;

/// <summary>
/// Currency rate repository interface for currency rate operations
/// </summary>
public interface ICurrencyRateRepository : IGenericRepository<CurrencyRate>
{
    /// <summary>
    /// Gets the exchange rate between two currencies
    /// </summary>
    /// <param name="fromCurrency">Source currency code</param>
    /// <param name="toCurrency">Target currency code</param>
    /// <param name="effectiveDate">Optional date for historical rates (uses current date if not specified)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Currency rate if found, null otherwise</returns>
    Task<CurrencyRate?> GetRateAsync(
        string fromCurrency,
        string toCurrency,
        DateTime? effectiveDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active currency rates
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active currency rates</returns>
    Task<IEnumerable<CurrencyRate>> GetActiveRatesAsync(CancellationToken cancellationToken = default);
}
