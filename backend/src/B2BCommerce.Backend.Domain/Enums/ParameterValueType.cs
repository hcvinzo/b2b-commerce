namespace B2BCommerce.Backend.Domain.Enums;

/// <summary>
/// Defines the data type of a configuration parameter value
/// </summary>
public enum ParameterValueType
{
    /// <summary>
    /// Free text value
    /// </summary>
    String = 0,

    /// <summary>
    /// Numeric value (integer or decimal)
    /// </summary>
    Number = 1,

    /// <summary>
    /// Boolean true/false value
    /// </summary>
    Boolean = 2,

    /// <summary>
    /// Date and time value
    /// </summary>
    DateTime = 3,

    /// <summary>
    /// JSON object or array
    /// </summary>
    Json = 4
}
