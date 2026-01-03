using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.DTOs.Currencies;

/// <summary>
/// Data transfer object for creating a new currency
/// </summary>
public class CreateCurrencyDto
{
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
    /// Number of decimal places for the currency (default: 2)
    /// </summary>
    public int DecimalPlaces { get; set; } = 2;

    /// <summary>
    /// Display order for UI sorting
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// How exchange rates are managed for this currency (default: Manual)
    /// </summary>
    public RateManagementMode RateManagementMode { get; set; } = RateManagementMode.Manual;
}
