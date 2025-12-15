namespace B2BCommerce.Backend.Domain.Enums;

/// <summary>
/// Defines the data type for product attributes
/// </summary>
public enum AttributeType
{
    /// <summary>
    /// Free text input
    /// </summary>
    Text = 1,

    /// <summary>
    /// Numeric value with optional unit
    /// </summary>
    Number = 2,

    /// <summary>
    /// Single selection from predefined values
    /// </summary>
    Select = 3,

    /// <summary>
    /// Multiple selection from predefined values
    /// </summary>
    MultiSelect = 4,

    /// <summary>
    /// Yes/No boolean value
    /// </summary>
    Boolean = 5,

    /// <summary>
    /// Date value
    /// </summary>
    Date = 6
}
