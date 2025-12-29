using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Exceptions;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Stores customer attribute values in a flexible EAV pattern.
/// Links to AttributeDefinition for schema and stores values as JSON.
/// </summary>
public class CustomerAttribute : BaseEntity
{
    /// <summary>
    /// FK to the customer
    /// </summary>
    public Guid CustomerId { get; private set; }

    /// <summary>
    /// FK to the attribute definition
    /// </summary>
    public Guid AttributeDefinitionId { get; private set; }

    /// <summary>
    /// JSON value for the attribute.
    /// Format depends on AttributeDefinition.Type:
    /// - Text: { "value": "string" }
    /// - Number: { "value": 123.45 }
    /// - Select: { "selectedValueId": "guid", "displayText": "text" }
    /// - MultiSelect: { "selectedValueIds": ["guid1", "guid2"], "displayTexts": ["text1", "text2"] }
    /// - Boolean: { "value": true }
    /// - Date: { "value": "2024-01-15" }
    /// - Composite: { "field1": "value1", "field2": 123 }
    /// For IsList=true attributes, value is an array: [{ ... }, { ... }]
    /// </summary>
    public string Value { get; private set; }

    // Navigation properties
    public Customer Customer { get; private set; } = null!;
    public AttributeDefinition AttributeDefinition { get; private set; } = null!;

    private CustomerAttribute() // For EF Core
    {
        Value = string.Empty;
    }

    /// <summary>
    /// Creates a new CustomerAttribute instance
    /// </summary>
    public static CustomerAttribute Create(
        Guid customerId,
        Guid attributeDefinitionId,
        string value)
    {
        if (customerId == Guid.Empty)
        {
            throw new DomainException("CustomerId is required");
        }

        if (attributeDefinitionId == Guid.Empty)
        {
            throw new DomainException("AttributeDefinitionId is required");
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Value is required");
        }

        return new CustomerAttribute
        {
            CustomerId = customerId,
            AttributeDefinitionId = attributeDefinitionId,
            Value = value
        };
    }

    /// <summary>
    /// Updates the attribute value
    /// </summary>
    public void UpdateValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Value is required");
        }

        Value = value;
    }
}
