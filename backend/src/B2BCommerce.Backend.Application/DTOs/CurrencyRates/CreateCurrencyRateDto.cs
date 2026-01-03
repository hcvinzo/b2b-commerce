namespace B2BCommerce.Backend.Application.DTOs.CurrencyRates;

/// <summary>
/// Data transfer object for creating a new currency rate
/// </summary>
public class CreateCurrencyRateDto
{
    /// <summary>
    /// Source currency code (ISO 4217)
    /// </summary>
    public string FromCurrency { get; set; } = string.Empty;

    /// <summary>
    /// Target currency code (ISO 4217)
    /// </summary>
    public string ToCurrency { get; set; } = string.Empty;

    /// <summary>
    /// Exchange rate value
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// Date when this rate becomes effective (defaults to now if not provided)
    /// </summary>
    public DateTime? EffectiveDate { get; set; }
}
