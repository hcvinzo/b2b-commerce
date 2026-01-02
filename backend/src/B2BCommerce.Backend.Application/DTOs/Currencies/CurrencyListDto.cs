namespace B2BCommerce.Backend.Application.DTOs.Currencies;

/// <summary>
/// Simplified currency data transfer object for list views
/// </summary>
public class CurrencyListDto
{
    /// <summary>
    /// Currency identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ISO 4217 currency code (e.g., USD, EUR, TRY)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Currency name (e.g., US Dollar, Euro, Turkish Lira)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Currency symbol (e.g., $, EUR, TRY)
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Number of decimal places for the currency
    /// </summary>
    public int DecimalPlaces { get; set; }

    /// <summary>
    /// Whether this is the default system currency
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Whether the currency is active and can be used
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Display order for UI sorting
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Date created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
