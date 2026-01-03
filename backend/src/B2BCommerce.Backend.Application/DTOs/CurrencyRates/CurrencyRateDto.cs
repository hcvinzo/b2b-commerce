namespace B2BCommerce.Backend.Application.DTOs.CurrencyRates;

/// <summary>
/// Currency rate data transfer object for output
/// </summary>
public class CurrencyRateDto
{
    /// <summary>
    /// Currency rate identifier
    /// </summary>
    public Guid Id { get; set; }

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
    /// Date when this rate became effective
    /// </summary>
    public DateTime EffectiveDate { get; set; }

    /// <summary>
    /// Whether this rate is currently active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Date created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
