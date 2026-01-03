using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.CurrencyRates;

namespace B2BCommerce.Backend.Application.Interfaces.Services;

/// <summary>
/// Currency rate service interface for currency rate operations
/// </summary>
public interface ICurrencyRateService
{
    /// <summary>
    /// Gets all currency rates with optional filtering
    /// </summary>
    Task<Result<List<CurrencyRateListDto>>> GetAllAsync(
        CurrencyRateFiltersDto? filters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a currency rate by ID
    /// </summary>
    Task<Result<CurrencyRateDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the exchange rate between two currencies
    /// </summary>
    Task<Result<CurrencyRateDto>> GetRateAsync(
        string fromCurrency,
        string toCurrency,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new currency rate
    /// </summary>
    Task<Result<CurrencyRateDto>> CreateAsync(CreateCurrencyRateDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing currency rate
    /// </summary>
    Task<Result<CurrencyRateDto>> UpdateAsync(Guid id, UpdateCurrencyRateDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a currency rate (soft delete)
    /// </summary>
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates a currency rate
    /// </summary>
    Task<Result<CurrencyRateDto>> ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a currency rate
    /// </summary>
    Task<Result<CurrencyRateDto>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
