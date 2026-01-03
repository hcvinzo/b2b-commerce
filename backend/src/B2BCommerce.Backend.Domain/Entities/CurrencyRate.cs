using B2BCommerce.Backend.Domain.Common;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Currency exchange rate entity
/// </summary>
public class CurrencyRate : BaseEntity
{
    /// <summary>
    /// Source currency code (ISO 4217)
    /// </summary>
    public string FromCurrency { get; private set; }

    /// <summary>
    /// Target currency code (ISO 4217)
    /// </summary>
    public string ToCurrency { get; private set; }

    /// <summary>
    /// Exchange rate value (how much ToCurrency equals 1 unit of FromCurrency)
    /// </summary>
    public decimal Rate { get; private set; }

    /// <summary>
    /// Date when this rate became effective
    /// </summary>
    public DateTime EffectiveDate { get; private set; }

    /// <summary>
    /// Whether this rate is currently active
    /// </summary>
    public bool IsActive { get; private set; }

    private CurrencyRate() : base() // For EF Core
    {
        FromCurrency = string.Empty;
        ToCurrency = string.Empty;
    }

    /// <summary>
    /// Creates a new currency exchange rate
    /// </summary>
    public static CurrencyRate Create(
        string fromCurrency,
        string toCurrency,
        decimal rate,
        DateTime? effectiveDate = null)
    {
        if (string.IsNullOrWhiteSpace(fromCurrency))
        {
            throw new ArgumentException("From currency cannot be null or empty", nameof(fromCurrency));
        }

        if (string.IsNullOrWhiteSpace(toCurrency))
        {
            throw new ArgumentException("To currency cannot be null or empty", nameof(toCurrency));
        }

        if (rate <= 0)
        {
            throw new ArgumentException("Exchange rate must be greater than zero", nameof(rate));
        }

        if (fromCurrency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("From and To currencies must be different");
        }

        return new CurrencyRate
        {
            FromCurrency = fromCurrency.ToUpperInvariant(),
            ToCurrency = toCurrency.ToUpperInvariant(),
            Rate = rate,
            EffectiveDate = effectiveDate ?? DateTime.UtcNow,
            IsActive = true
        };
    }

    /// <summary>
    /// Updates the exchange rate value
    /// </summary>
    public void UpdateRate(decimal newRate, DateTime? effectiveDate = null)
    {
        if (newRate <= 0)
        {
            throw new ArgumentException("Exchange rate must be greater than zero", nameof(newRate));
        }

        Rate = newRate;
        EffectiveDate = effectiveDate ?? DateTime.UtcNow;
    }

    /// <summary>
    /// Activates this exchange rate
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates this exchange rate
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }
}
