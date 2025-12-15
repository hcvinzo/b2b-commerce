using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.Exceptions;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Defines a product attribute template (e.g., "Screen Size", "Memory Capacity")
/// </summary>
public class AttributeDefinition : BaseEntity, IAggregateRoot
{
    /// <summary>
    /// Unique code for the attribute (e.g., "screen_size", "memory_capacity")
    /// </summary>
    public string Code { get; private set; }

    /// <summary>
    /// Display name in Turkish
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Display name in English (optional)
    /// </summary>
    public string? NameEn { get; private set; }

    /// <summary>
    /// Data type for this attribute
    /// </summary>
    public AttributeType Type { get; private set; }

    /// <summary>
    /// Unit of measurement (e.g., "GB", "MB/s", "mm")
    /// </summary>
    public string? Unit { get; private set; }

    /// <summary>
    /// Whether this attribute should appear in product filters
    /// </summary>
    public bool IsFilterable { get; private set; }

    /// <summary>
    /// Default required status (can be overridden per ProductType)
    /// </summary>
    public bool IsRequired { get; private set; }

    /// <summary>
    /// Whether to display on product detail page
    /// </summary>
    public bool IsVisibleOnProductPage { get; private set; }

    /// <summary>
    /// Display order in UI
    /// </summary>
    public int DisplayOrder { get; private set; }

    // Navigation properties
    private readonly List<AttributeValue> _predefinedValues = new();
    public IReadOnlyCollection<AttributeValue> PredefinedValues => _predefinedValues.AsReadOnly();

    private AttributeDefinition() // For EF Core
    {
        Code = string.Empty;
        Name = string.Empty;
    }

    /// <summary>
    /// Creates a new AttributeDefinition instance
    /// </summary>
    public static AttributeDefinition Create(
        string code,
        string name,
        AttributeType type,
        string? nameEn = null,
        string? unit = null,
        bool isFilterable = true,
        bool isRequired = false,
        bool isVisibleOnProductPage = true,
        int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("Attribute code is required");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Attribute name is required");
        }

        // Validate that Select/MultiSelect types should have predefined values
        // This is just a warning during creation, values can be added later

        return new AttributeDefinition
        {
            Code = code.Trim().ToLowerInvariant(),
            Name = name.Trim(),
            NameEn = nameEn?.Trim(),
            Type = type,
            Unit = unit?.Trim(),
            IsFilterable = isFilterable,
            IsRequired = isRequired,
            IsVisibleOnProductPage = isVisibleOnProductPage,
            DisplayOrder = displayOrder
        };
    }

    /// <summary>
    /// Updates the attribute definition details
    /// </summary>
    public void Update(
        string name,
        string? nameEn,
        string? unit,
        bool isFilterable,
        bool isRequired,
        bool isVisibleOnProductPage,
        int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Attribute name is required");
        }

        Name = name.Trim();
        NameEn = nameEn?.Trim();
        Unit = unit?.Trim();
        IsFilterable = isFilterable;
        IsRequired = isRequired;
        IsVisibleOnProductPage = isVisibleOnProductPage;
        DisplayOrder = displayOrder;
    }

    /// <summary>
    /// Adds a predefined value for Select/MultiSelect types
    /// </summary>
    public AttributeValue AddPredefinedValue(string value, string? displayText = null, int displayOrder = 0)
    {
        if (Type != AttributeType.Select && Type != AttributeType.MultiSelect)
        {
            throw new DomainException("Predefined values can only be added to Select or MultiSelect attributes");
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Value is required");
        }

        if (_predefinedValues.Any(v => v.Value.Equals(value, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException($"Value '{value}' already exists");
        }

        var attributeValue = AttributeValue.Create(Id, value, displayText, displayOrder);
        _predefinedValues.Add(attributeValue);
        return attributeValue;
    }

    /// <summary>
    /// Removes a predefined value
    /// </summary>
    public void RemovePredefinedValue(Guid valueId)
    {
        var value = _predefinedValues.FirstOrDefault(v => v.Id == valueId);
        if (value is not null)
        {
            _predefinedValues.Remove(value);
        }
    }

    /// <summary>
    /// Clears all predefined values
    /// </summary>
    public void ClearPredefinedValues()
    {
        _predefinedValues.Clear();
    }

    /// <summary>
    /// Checks if the attribute requires predefined values
    /// </summary>
    public bool RequiresPredefinedValues() => Type == AttributeType.Select || Type == AttributeType.MultiSelect;

    /// <summary>
    /// Validates that an attribute value is valid for this definition
    /// </summary>
    public bool IsValidValue(string? textValue, decimal? numericValue, Guid? selectedValueId, bool? booleanValue, DateTime? dateValue, List<Guid>? multiSelectValueIds)
    {
        return Type switch
        {
            AttributeType.Text => !string.IsNullOrEmpty(textValue),
            AttributeType.Number => numericValue.HasValue,
            AttributeType.Select => selectedValueId.HasValue && _predefinedValues.Any(v => v.Id == selectedValueId.Value),
            AttributeType.MultiSelect => multiSelectValueIds?.Count > 0 && multiSelectValueIds.All(id => _predefinedValues.Any(v => v.Id == id)),
            AttributeType.Boolean => booleanValue.HasValue,
            AttributeType.Date => dateValue.HasValue,
            _ => false
        };
    }
}
