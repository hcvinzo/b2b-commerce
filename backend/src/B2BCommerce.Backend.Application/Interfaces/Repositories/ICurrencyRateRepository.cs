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

    /// <summary>
    /// Gets all currency rates with optional filtering
    /// </summary>
    /// <param name="fromCurrency">Optional source currency filter</param>
    /// <param name="toCurrency">Optional target currency filter</param>
    /// <param name="activeOnly">Whether to return only active rates</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of currency rates</returns>
    Task<IEnumerable<CurrencyRate>> GetRatesAsync(
        string? fromCurrency = null,
        string? toCurrency = null,
        bool? activeOnly = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a currency rate by ID for update (with tracking)
    /// </summary>
    /// <param name="id">Currency rate ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Currency rate if found, null otherwise</returns>
    Task<CurrencyRate?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a rate already exists for the given currency pair
    /// </summary>
    /// <param name="fromCurrency">Source currency code</param>
    /// <param name="toCurrency">Target currency code</param>
    /// <param name="excludeId">Optional ID to exclude from the check (for updates)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if rate exists, false otherwise</returns>
    Task<bool> ExistsAsync(
        string fromCurrency,
        string toCurrency,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);
}
