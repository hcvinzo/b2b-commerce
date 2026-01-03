using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.DTOs.Currencies;

/// <summary>
/// Data transfer object for updating an existing currency
/// </summary>
public class UpdateCurrencyDto
{
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
    /// Display order for UI sorting
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// How exchange rates are managed for this currency
    /// </summary>
    public RateManagementMode RateManagementMode { get; set; }
}
