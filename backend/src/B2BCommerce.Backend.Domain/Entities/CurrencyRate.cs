using B2BCommerce.Backend.Domain.Common;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Currency exchange rate entity
/// </summary>
public class CurrencyRate : BaseEntity
{
    public string FromCurrency { get; private set; }
    public string ToCurrency { get; private set; }
    public decimal Rate { get; private set; }
    public DateTime EffectiveDate { get; private set; }
    public bool IsActive { get; private set; }

    private CurrencyRate() // For EF Core
    {
        FromCurrency = string.Empty;
        ToCurrency = string.Empty;
    }

    public CurrencyRate(string fromCurrency, string toCurrency, decimal rate, DateTime? effectiveDate = null)
    {
        if (string.IsNullOrWhiteSpace(fromCurrency))
            throw new ArgumentException("From currency cannot be null or empty", nameof(fromCurrency));

        if (string.IsNullOrWhiteSpace(toCurrency))
            throw new ArgumentException("To currency cannot be null or empty", nameof(toCurrency));

        if (rate <= 0)
            throw new ArgumentException("Exchange rate must be greater than zero", nameof(rate));

        if (fromCurrency == toCurrency)
            throw new ArgumentException("From and To currencies must be different");

        FromCurrency = fromCurrency.ToUpperInvariant();
        ToCurrency = toCurrency.ToUpperInvariant();
        Rate = rate;
        EffectiveDate = effectiveDate ?? DateTime.UtcNow;
        IsActive = true;
    }

    public void UpdateRate(decimal newRate)
    {
        if (newRate <= 0)
            throw new ArgumentException("Exchange rate must be greater than zero", nameof(newRate));

        Rate = newRate;
        EffectiveDate = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
