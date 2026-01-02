namespace B2BCommerce.Backend.Domain.Enums;

/// <summary>
/// Defines the type of system configuration parameter
/// </summary>
public enum ParameterType
{
    /// <summary>
    /// Technical system settings (cache, timeouts, etc.)
    /// </summary>
    System = 0,

    /// <summary>
    /// Business rules and limits (order minimums, cart limits, defaults)
    /// </summary>
    Business = 1
}
