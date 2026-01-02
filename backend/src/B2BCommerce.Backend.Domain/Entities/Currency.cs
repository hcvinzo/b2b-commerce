using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Exceptions;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Currency entity representing supported currencies in the system
/// </summary>
public class Currency : BaseEntity, IAggregateRoot
{
    /// <summary>
    /// ISO 4217 currency code (e.g., USD, EUR, TRY)
    /// </summary>
    public string Code { get; private set; }

    /// <summary>
    /// Currency name (e.g., US Dollar, Euro, Turkish Lira)
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Currency symbol (e.g., $, €, ₺)
    /// </summary>
    public string Symbol { get; private set; }

    /// <summary>
    /// Number of decimal places for the currency (typically 2)
    /// </summary>
    public int DecimalPlaces { get; private set; }

    /// <summary>
    /// Whether this is the default system currency
    /// </summary>
    public bool IsDefault { get; private set; }

    /// <summary>
    /// Whether the currency is active and can be used
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Display order for UI sorting
    /// </summary>
    public int DisplayOrder { get; private set; }

    private Currency() // For EF Core
    {
        Code = string.Empty;
        Name = string.Empty;
        Symbol = string.Empty;
    }

    /// <summary>
    /// Creates a new currency
    /// </summary>
    public static Currency Create(
        string code,
        string name,
        string symbol,
        int decimalPlaces = 2,
        int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Currency code cannot be null or empty", nameof(code));
        }

        if (code.Length != 3)
        {
            throw new ArgumentException("Currency code must be exactly 3 characters (ISO 4217)", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Currency name cannot be null or empty", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("Currency symbol cannot be null or empty", nameof(symbol));
        }

        if (decimalPlaces < 0 || decimalPlaces > 4)
        {
            throw new ArgumentException("Decimal places must be between 0 and 4", nameof(decimalPlaces));
        }

        return new Currency
        {
            Code = code.ToUpperInvariant(),
            Name = name.Trim(),
            Symbol = symbol.Trim(),
            DecimalPlaces = decimalPlaces,
            IsDefault = false,
            IsActive = true,
            DisplayOrder = displayOrder
        };
    }

    /// <summary>
    /// Updates the currency details
    /// </summary>
    public void Update(string name, string symbol, int decimalPlaces, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Currency name cannot be null or empty", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("Currency symbol cannot be null or empty", nameof(symbol));
        }

        if (decimalPlaces < 0 || decimalPlaces > 4)
        {
            throw new ArgumentException("Decimal places must be between 0 and 4", nameof(decimalPlaces));
        }

        Name = name.Trim();
        Symbol = symbol.Trim();
        DecimalPlaces = decimalPlaces;
        DisplayOrder = displayOrder;
    }

    /// <summary>
    /// Sets this currency as the default
    /// </summary>
    public void SetAsDefault()
    {
        if (!IsActive)
        {
            throw new InvalidOperationDomainException("Cannot set inactive currency as default");
        }

        IsDefault = true;
    }

    /// <summary>
    /// Removes the default status from this currency
    /// </summary>
    public void ClearDefault()
    {
        IsDefault = false;
    }

    /// <summary>
    /// Activates the currency
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates the currency
    /// </summary>
    public void Deactivate()
    {
        if (IsDefault)
        {
            throw new InvalidOperationDomainException("Cannot deactivate the default currency");
        }

        IsActive = false;
    }
}
