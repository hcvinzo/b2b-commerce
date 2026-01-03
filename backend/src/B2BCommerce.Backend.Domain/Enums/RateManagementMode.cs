namespace B2BCommerce.Backend.Domain.Enums;

/// <summary>
/// Defines how exchange rates are managed for a currency
/// </summary>
public enum RateManagementMode
{
    /// <summary>
    /// Exchange rates are entered manually by admin users
    /// </summary>
    Manual = 1,

    /// <summary>
    /// Exchange rates are imported from Turkish Central Bank (TCMB)
    /// </summary>
    TCMB = 2
}
