using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Exceptions;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Represents a predefined value for Select/MultiSelect attributes
/// </summary>
public class AttributeValue : BaseEntity
{
    /// <summary>
    /// FK to the parent AttributeDefinition
    /// </summary>
    public Guid AttributeDefinitionId { get; private set; }

    /// <summary>
    /// Internal value (e.g., "256", "red", "yes")
    /// </summary>
    public string Value { get; private set; }

    /// <summary>
    /// User-facing display text (e.g., "256 GB", "Red", "Yes")
    /// If null, Value is used for display
    /// </summary>
    public string? DisplayText { get; private set; }

    /// <summary>
    /// Display order in dropdowns/lists
    /// </summary>
    public int DisplayOrder { get; private set; }

    // Navigation property
    public AttributeDefinition AttributeDefinition { get; private set; } = null!;

    private AttributeValue() // For EF Core
    {
        Value = string.Empty;
    }

    /// <summary>
    /// Creates a new AttributeValue instance
    /// </summary>
    internal static AttributeValue Create(
        Guid attributeDefinitionId,
        string value,
        string? displayText = null,
        int displayOrder = 0)
    {
        if (attributeDefinitionId == Guid.Empty)
        {
            throw new DomainException("AttributeDefinitionId is required");
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Value is required");
        }

        return new AttributeValue
        {
            AttributeDefinitionId = attributeDefinitionId,
            Value = value.Trim(),
            DisplayText = displayText?.Trim(),
            DisplayOrder = displayOrder
        };
    }

    /// <summary>
    /// Updates the attribute value
    /// </summary>
    public void Update(string value, string? displayText, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Value is required");
        }

        Value = value.Trim();
        DisplayText = displayText?.Trim();
        DisplayOrder = displayOrder;
    }

    /// <summary>
    /// Gets the display text, falling back to Value if DisplayText is null
    /// </summary>
    public string GetDisplayText() => DisplayText ?? Value;
}
