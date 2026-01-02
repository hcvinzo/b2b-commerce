using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Currencies;

namespace B2BCommerce.Backend.Application.Interfaces.Services;

/// <summary>
/// Currency service interface for currency operations
/// </summary>
public interface ICurrencyService
{
    /// <summary>
    /// Gets all currencies ordered by display order
    /// </summary>
    Task<Result<List<CurrencyListDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active currencies ordered by display order
    /// </summary>
    Task<Result<List<CurrencyListDto>>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a currency by ID
    /// </summary>
    Task<Result<CurrencyDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a currency by its ISO code
    /// </summary>
    Task<Result<CurrencyDto>> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the default currency
    /// </summary>
    Task<Result<CurrencyDto>> GetDefaultAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new currency
    /// </summary>
    Task<Result<CurrencyDto>> CreateAsync(CreateCurrencyDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing currency
    /// </summary>
    Task<Result<CurrencyDto>> UpdateAsync(Guid id, UpdateCurrencyDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a currency (soft delete)
    /// </summary>
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates a currency
    /// </summary>
    Task<Result<CurrencyDto>> ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a currency
    /// </summary>
    Task<Result<CurrencyDto>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a currency as the default
    /// </summary>
    Task<Result<CurrencyDto>> SetDefaultAsync(Guid id, CancellationToken cancellationToken = default);
}
