using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.Exceptions;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Stores the actual attribute value for a specific product
/// </summary>
public class ProductAttributeValue : BaseEntity
{
    /// <summary>
    /// FK to the Product
    /// </summary>
    public Guid ProductId { get; private set; }

    /// <summary>
    /// FK to the AttributeDefinition
    /// </summary>
    public Guid AttributeDefinitionId { get; private set; }

    /// <summary>
    /// Value for Text type attributes
    /// </summary>
    public string? TextValue { get; private set; }

    /// <summary>
    /// Value for Number type attributes
    /// </summary>
    public decimal? NumericValue { get; private set; }

    /// <summary>
    /// FK to AttributeValue for Select type attributes
    /// </summary>
    public Guid? AttributeValueId { get; private set; }

    /// <summary>
    /// Value for Boolean type attributes
    /// </summary>
    public bool? BooleanValue { get; private set; }

    /// <summary>
    /// Value for Date type attributes
    /// </summary>
    public DateTime? DateValue { get; private set; }

    /// <summary>
    /// JSON array of AttributeValue IDs for MultiSelect type attributes
    /// </summary>
    public string? MultiSelectValueIds { get; private set; }

    // Navigation properties
    public Product Product { get; private set; } = null!;
    public AttributeDefinition AttributeDefinition { get; private set; } = null!;
    public AttributeValue? SelectedValue { get; private set; }

    private ProductAttributeValue() // For EF Core
    {
    }

    /// <summary>
    /// Creates a new ProductAttributeValue for Text type
    /// </summary>
    public static ProductAttributeValue CreateText(Guid productId, Guid attributeDefinitionId, string value)
    {
        ValidateIds(productId, attributeDefinitionId);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Text value is required");
        }

        return new ProductAttributeValue
        {
            ProductId = productId,
            AttributeDefinitionId = attributeDefinitionId,
            TextValue = value.Trim()
        };
    }

    /// <summary>
    /// Creates a new ProductAttributeValue for Number type
    /// </summary>
    public static ProductAttributeValue CreateNumber(Guid productId, Guid attributeDefinitionId, decimal value)
    {
        ValidateIds(productId, attributeDefinitionId);

        return new ProductAttributeValue
        {
            ProductId = productId,
            AttributeDefinitionId = attributeDefinitionId,
            NumericValue = value
        };
    }

    /// <summary>
    /// Creates a new ProductAttributeValue for Select type
    /// </summary>
    public static ProductAttributeValue CreateSelect(Guid productId, Guid attributeDefinitionId, Guid selectedValueId)
    {
        ValidateIds(productId, attributeDefinitionId);

        if (selectedValueId == Guid.Empty)
        {
            throw new DomainException("Selected value ID is required");
        }

        return new ProductAttributeValue
        {
            ProductId = productId,
            AttributeDefinitionId = attributeDefinitionId,
            AttributeValueId = selectedValueId
        };
    }

    /// <summary>
    /// Creates a new ProductAttributeValue for MultiSelect type
    /// </summary>
    public static ProductAttributeValue CreateMultiSelect(Guid productId, Guid attributeDefinitionId, List<Guid> selectedValueIds)
    {
        ValidateIds(productId, attributeDefinitionId);

        if (selectedValueIds == null || selectedValueIds.Count == 0)
        {
            throw new DomainException("At least one selected value is required");
        }

        return new ProductAttributeValue
        {
            ProductId = productId,
            AttributeDefinitionId = attributeDefinitionId,
            MultiSelectValueIds = System.Text.Json.JsonSerializer.Serialize(selectedValueIds)
        };
    }

    /// <summary>
    /// Creates a new ProductAttributeValue for Boolean type
    /// </summary>
    public static ProductAttributeValue CreateBoolean(Guid productId, Guid attributeDefinitionId, bool value)
    {
        ValidateIds(productId, attributeDefinitionId);

        return new ProductAttributeValue
        {
            ProductId = productId,
            AttributeDefinitionId = attributeDefinitionId,
            BooleanValue = value
        };
    }

    /// <summary>
    /// Creates a new ProductAttributeValue for Date type
    /// </summary>
    public static ProductAttributeValue CreateDate(Guid productId, Guid attributeDefinitionId, DateTime value)
    {
        ValidateIds(productId, attributeDefinitionId);

        return new ProductAttributeValue
        {
            ProductId = productId,
            AttributeDefinitionId = attributeDefinitionId,
            DateValue = value
        };
    }

    /// <summary>
    /// Creates a ProductAttributeValue based on the attribute type
    /// </summary>
    public static ProductAttributeValue Create(
        Guid productId,
        Guid attributeDefinitionId,
        AttributeType type,
        string? textValue = null,
        decimal? numericValue = null,
        Guid? selectedValueId = null,
        bool? booleanValue = null,
        DateTime? dateValue = null,
        List<Guid>? multiSelectValueIds = null)
    {
        return type switch
        {
            AttributeType.Text => CreateText(productId, attributeDefinitionId, textValue!),
            AttributeType.Number => CreateNumber(productId, attributeDefinitionId, numericValue!.Value),
            AttributeType.Select => CreateSelect(productId, attributeDefinitionId, selectedValueId!.Value),
            AttributeType.MultiSelect => CreateMultiSelect(productId, attributeDefinitionId, multiSelectValueIds!),
            AttributeType.Boolean => CreateBoolean(productId, attributeDefinitionId, booleanValue!.Value),
            AttributeType.Date => CreateDate(productId, attributeDefinitionId, dateValue!.Value),
            _ => throw new DomainException($"Unknown attribute type: {type}")
        };
    }

    /// <summary>
    /// Updates the text value
    /// </summary>
    public void UpdateTextValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Text value is required");
        }

        TextValue = value.Trim();
        ClearOtherValues(AttributeType.Text);
    }

    /// <summary>
    /// Updates the numeric value
    /// </summary>
    public void UpdateNumericValue(decimal value)
    {
        NumericValue = value;
        ClearOtherValues(AttributeType.Number);
    }

    /// <summary>
    /// Updates the selected value
    /// </summary>
    public void UpdateSelectValue(Guid selectedValueId)
    {
        if (selectedValueId == Guid.Empty)
        {
            throw new DomainException("Selected value ID is required");
        }

        AttributeValueId = selectedValueId;
        ClearOtherValues(AttributeType.Select);
    }

    /// <summary>
    /// Updates the multi-select values
    /// </summary>
    public void UpdateMultiSelectValues(List<Guid> selectedValueIds)
    {
        if (selectedValueIds == null || selectedValueIds.Count == 0)
        {
            throw new DomainException("At least one selected value is required");
        }

        MultiSelectValueIds = System.Text.Json.JsonSerializer.Serialize(selectedValueIds);
        ClearOtherValues(AttributeType.MultiSelect);
    }

    /// <summary>
    /// Updates the boolean value
    /// </summary>
    public void UpdateBooleanValue(bool value)
    {
        BooleanValue = value;
        ClearOtherValues(AttributeType.Boolean);
    }

    /// <summary>
    /// Updates the date value
    /// </summary>
    public void UpdateDateValue(DateTime value)
    {
        DateValue = value;
        ClearOtherValues(AttributeType.Date);
    }

    /// <summary>
    /// Gets the multi-select value IDs as a list
    /// </summary>
    public List<Guid> GetMultiSelectValueIds()
    {
        if (string.IsNullOrEmpty(MultiSelectValueIds))
        {
            return new List<Guid>();
        }

        return System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(MultiSelectValueIds) ?? new List<Guid>();
    }

    private static void ValidateIds(Guid productId, Guid attributeDefinitionId)
    {
        if (productId == Guid.Empty)
        {
            throw new DomainException("ProductId is required");
        }

        if (attributeDefinitionId == Guid.Empty)
        {
            throw new DomainException("AttributeDefinitionId is required");
        }
    }

    private void ClearOtherValues(AttributeType keepType)
    {
        if (keepType != AttributeType.Text)
        {
            TextValue = null;
        }

        if (keepType != AttributeType.Number)
        {
            NumericValue = null;
        }

        if (keepType != AttributeType.Select)
        {
            AttributeValueId = null;
        }

        if (keepType != AttributeType.MultiSelect)
        {
            MultiSelectValueIds = null;
        }

        if (keepType != AttributeType.Boolean)
        {
            BooleanValue = null;
        }

        if (keepType != AttributeType.Date)
        {
            DateValue = null;
        }
    }
}
