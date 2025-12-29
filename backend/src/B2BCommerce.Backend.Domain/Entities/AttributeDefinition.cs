using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.Exceptions;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Defines an attribute template for products or customers (e.g., "Screen Size", "Credit Limit").
/// Inherits from ExternalEntity to support external system synchronization.
/// </summary>
public class AttributeDefinition : ExternalEntity, IAggregateRoot
{
    /// <summary>
    /// Unique code for the attribute (e.g., "screen_size", "credit_limit")
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
    /// Which entity type this attribute belongs to (Product or Customer)
    /// </summary>
    public AttributeEntityType EntityType { get; private set; }

    /// <summary>
    /// Unit of measurement (e.g., "GB", "MB/s", "mm", "TRY")
    /// </summary>
    public string? Unit { get; private set; }

    /// <summary>
    /// Whether this attribute should appear in filters (for products)
    /// </summary>
    public bool IsFilterable { get; private set; }

    /// <summary>
    /// Default required status
    /// </summary>
    public bool IsRequired { get; private set; }

    /// <summary>
    /// Whether to display on product detail page (for products)
    /// </summary>
    public bool IsVisibleOnProductPage { get; private set; }

    /// <summary>
    /// Display order in UI
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// FK to parent attribute for composite type children
    /// </summary>
    public Guid? ParentAttributeId { get; private set; }

    /// <summary>
    /// Whether entity can have multiple values for this attribute (e.g., multiple bank accounts)
    /// </summary>
    public bool IsList { get; private set; }

    // Navigation properties
    private readonly List<AttributeValue> _predefinedValues = new();
    public IReadOnlyCollection<AttributeValue> PredefinedValues => _predefinedValues.AsReadOnly();

    public AttributeDefinition? ParentAttribute { get; private set; }

    private readonly List<AttributeDefinition> _childAttributes = new();
    public IReadOnlyCollection<AttributeDefinition> ChildAttributes => _childAttributes.AsReadOnly();

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
        AttributeEntityType entityType = AttributeEntityType.Product,
        string? unit = null,
        bool isFilterable = true,
        bool isRequired = false,
        bool isVisibleOnProductPage = true,
        int displayOrder = 0,
        Guid? parentAttributeId = null,
        bool isList = false)
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
            EntityType = entityType,
            Unit = unit?.Trim(),
            IsFilterable = isFilterable,
            IsRequired = isRequired,
            IsVisibleOnProductPage = isVisibleOnProductPage,
            DisplayOrder = displayOrder,
            ParentAttributeId = parentAttributeId,
            IsList = isList
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
    /// <param name="entityType">Which entity type this attribute belongs to</param>
    /// <param name="unit">Unit of measurement (optional)</param>
    /// <param name="isFilterable">Whether this attribute should appear in product filters</param>
    /// <param name="isRequired">Default required status</param>
    /// <param name="isVisibleOnProductPage">Whether to display on product detail page</param>
    /// <param name="displayOrder">Display order in UI</param>
    /// <param name="parentAttributeId">Parent attribute ID for composite children</param>
    /// <param name="isList">Whether entity can have multiple values</param>
    /// <param name="externalCode">External system code (optional)</param>
    public static AttributeDefinition CreateFromExternal(
        string externalId,
        string code,
        string name,
        AttributeType type,
        AttributeEntityType entityType = AttributeEntityType.Product,
        string? unit = null,
        bool isFilterable = true,
        bool isRequired = false,
        bool isVisibleOnProductPage = true,
        int displayOrder = 0,
        Guid? parentAttributeId = null,
        bool isList = false,
        string? externalCode = null)
    {
        if (string.IsNullOrWhiteSpace(externalId))
        {
            throw new ArgumentException("External ID is required", nameof(externalId));
        }

        var attr = Create(code, name, type, entityType, unit, isFilterable, isRequired, isVisibleOnProductPage, displayOrder, parentAttributeId, isList);

        // Use base class helper for consistent initialization
        InitializeFromExternal(attr, externalId, externalCode);

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
        int displayOrder,
        bool isList = false)
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
        IsList = isList;
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
        bool isList = false,
        string? externalCode = null)
    {
        Update(name, unit, isFilterable, isRequired, isVisibleOnProductPage, displayOrder, isList);

        if (externalCode is not null)
        {
            SetExternalIdentifiers(externalCode, ExternalId);
        }

        MarkAsSynced();
    }

    /// <summary>
    /// Sets the parent attribute for composite type children
    /// </summary>
    public void SetParentAttribute(Guid? parentAttributeId)
    {
        ParentAttributeId = parentAttributeId;
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
    public bool IsValidValue(string? textValue, decimal? numericValue, Guid? selectedValueId, bool? booleanValue, DateTime? dateValue, List<Guid>? multiSelectValueIds, string? jsonValue = null)
    {
        return Type switch
        {
            AttributeType.Text => !string.IsNullOrEmpty(textValue),
            AttributeType.Number => numericValue.HasValue,
            AttributeType.Select => selectedValueId.HasValue && _predefinedValues.Any(v => v.Id == selectedValueId.Value),
            AttributeType.MultiSelect => multiSelectValueIds?.Count > 0 && multiSelectValueIds.All(id => _predefinedValues.Any(v => v.Id == id)),
            AttributeType.Boolean => booleanValue.HasValue,
            AttributeType.Date => dateValue.HasValue,
            AttributeType.Composite => !string.IsNullOrEmpty(jsonValue),
            _ => false
        };
    }

    /// <summary>
    /// Checks if this attribute is a composite type with child attributes
    /// </summary>
    public bool IsCompositeType() => Type == AttributeType.Composite;
}
