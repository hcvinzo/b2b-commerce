namespace B2BCommerce.Backend.Application.DTOs.CurrencyRates;

/// <summary>
/// Data transfer object for updating an existing currency rate
/// </summary>
public class UpdateCurrencyRateDto
{
    /// <summary>
    /// New exchange rate value
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// Date when this rate becomes effective (defaults to now if not provided)
    /// </summary>
    public DateTime? EffectiveDate { get; set; }
}
