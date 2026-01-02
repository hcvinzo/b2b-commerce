using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Application.Interfaces.Repositories;

/// <summary>
/// Currency repository interface for currency-specific operations
/// </summary>
public interface ICurrencyRepository : IGenericRepository<Currency>
{
    /// <summary>
    /// Gets a currency by its ISO code
    /// </summary>
    /// <param name="code">ISO 4217 currency code (e.g., USD, EUR, TRY)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Currency if found, null otherwise</returns>
    Task<Currency?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a currency exists by its ISO code
    /// </summary>
    /// <param name="code">ISO 4217 currency code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the default currency
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Default currency if set, null otherwise</returns>
    Task<Currency?> GetDefaultAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active currencies
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active currencies ordered by display order</returns>
    Task<IEnumerable<Currency>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all currencies ordered by display order
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all currencies</returns>
    Task<IEnumerable<Currency>> GetAllOrderedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears the default flag from all currencies
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ClearAllDefaultsAsync(CancellationToken cancellationToken = default);
}
