using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.Exceptions;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Defines a product attribute template (e.g., "Screen Size", "Memory Capacity").
/// Inherits from ExternalEntity to support external system synchronization.
/// </summary>
public class AttributeDefinition : ExternalEntity, IAggregateRoot
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

        var attribute = new AttributeDefinition
        {
            Code = code.Trim().ToLowerInvariant(),
            Name = name.Trim(),
            Type = type,
            Unit = unit?.Trim(),
            IsFilterable = isFilterable,
            IsRequired = isRequired,
            IsVisibleOnProductPage = isVisibleOnProductPage,
            DisplayOrder = displayOrder
        };

        // Auto-populate ExternalId for Integration API compatibility
        // This ensures entities created by B2B Commerce can be referenced by external systems
        attribute.SetExternalIdentifiers(externalCode: null, externalId: attribute.Id.ToString());

        return attribute;
    }

    /// <summary>
    /// Creates an attribute definition from an external system (LOGO ERP).
    /// Uses ExternalId as the primary upsert key.
    /// </summary>
    /// <param name="externalId">External system ID (required)</param>
    /// <param name="code">Attribute code (required, unique)</param>
    /// <param name="name">Display name in Turkish (required)</param>
    /// <param name="type">Data type for this attribute</param>
    /// <param name="unit">Unit of measurement (optional)</param>
    /// <param name="isFilterable">Whether this attribute should appear in product filters</param>
    /// <param name="isRequired">Default required status</param>
    /// <param name="isVisibleOnProductPage">Whether to display on product detail page</param>
    /// <param name="displayOrder">Display order in UI</param>
    /// <param name="externalCode">External system code (optional)</param>
    /// <param name="specificId">Optional specific internal ID to use instead of auto-generated</param>
    public static AttributeDefinition CreateFromExternal(
        string externalId,
        string code,
        string name,
        AttributeType type,
        string? unit = null,
        bool isFilterable = true,
        bool isRequired = false,
        bool isVisibleOnProductPage = true,
        int displayOrder = 0,
        string? externalCode = null,
        Guid? specificId = null)
    {
        if (string.IsNullOrWhiteSpace(externalId))
        {
            throw new ArgumentException("External ID is required", nameof(externalId));
        }

        var attr = Create(code, name, type, unit, isFilterable, isRequired, isVisibleOnProductPage, displayOrder);

        if (specificId.HasValue)
        {
            attr.Id = specificId.Value;
        }

        attr.SetExternalIdentifiers(externalCode, externalId);
        attr.MarkAsSynced();
        return attr;
    }

    /// <summary>
    /// Updates the attribute definition details
    /// </summary>
    public void Update(
        string name,
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
        Unit = unit?.Trim();
        IsFilterable = isFilterable;
        IsRequired = isRequired;
        IsVisibleOnProductPage = isVisibleOnProductPage;
        DisplayOrder = displayOrder;
    }

    /// <summary>
    /// Updates attribute definition from external system sync (LOGO ERP).
    /// ExternalCode is optional and can be updated if provided.
    /// </summary>
    public void UpdateFromExternal(
        string name,
        string? unit,
        bool isFilterable,
        bool isRequired,
        bool isVisibleOnProductPage,
        int displayOrder,
        string? externalCode = null)
    {
        Update(name, unit, isFilterable, isRequired, isVisibleOnProductPage, displayOrder);

        if (externalCode != null)
        {
            SetExternalIdentifiers(externalCode, ExternalId);
        }

        MarkAsSynced();
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
    /// Updates an existing predefined value
    /// </summary>
    public void UpdatePredefinedValue(Guid valueId, string value, string? displayText, int displayOrder)
    {
        var existing = _predefinedValues.FirstOrDefault(v => v.Id == valueId);
        if (existing == null)
        {
            throw new DomainException($"Value with ID '{valueId}' not found");
        }

        // Check for duplicate value (excluding current)
        if (_predefinedValues.Any(v => v.Id != valueId && v.Value.Equals(value, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException($"Value '{value}' already exists");
        }

        existing.Update(value, displayText, displayOrder);
    }

    /// <summary>
    /// Finds a predefined value by its value string
    /// </summary>
    public AttributeValue? FindPredefinedValueByValue(string value)
    {
        return _predefinedValues.FirstOrDefault(v => v.Value.Equals(value, StringComparison.OrdinalIgnoreCase));
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
