namespace B2BCommerce.Backend.Application.DTOs.CurrencyRates;

/// <summary>
/// Filter parameters for currency rate queries
/// </summary>
public class CurrencyRateFiltersDto
{
    /// <summary>
    /// Filter by source currency code
    /// </summary>
    public string? FromCurrency { get; set; }

    /// <summary>
    /// Filter by target currency code
    /// </summary>
    public string? ToCurrency { get; set; }

    /// <summary>
    /// Filter by active status
    /// </summary>
    public bool? ActiveOnly { get; set; }
}
