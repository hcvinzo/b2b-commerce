using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.DTOs.Customers;

/// <summary>
/// Customer attribute data transfer object for output
/// </summary>
public class CustomerAttributeDto
{
    /// <summary>
    /// Attribute identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Customer identifier
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Attribute type
    /// </summary>
    public CustomerAttributeType AttributeType { get; set; }

    /// <summary>
    /// Attribute type name for display
    /// </summary>
    public string AttributeTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Display order within the type
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// JSON data containing the attribute value
    /// </summary>
    public string JsonData { get; set; } = string.Empty;

    /// <summary>
    /// Date created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating a customer attribute
/// </summary>
public class CreateCustomerAttributeDto
{
    /// <summary>
    /// Attribute type
    /// </summary>
    public CustomerAttributeType AttributeType { get; set; }

    /// <summary>
    /// Display order within the type
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// JSON data containing the attribute value
    /// </summary>
    public string JsonData { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating a customer attribute
/// </summary>
public class UpdateCustomerAttributeDto
{
    /// <summary>
    /// Display order within the type
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// JSON data containing the attribute value
    /// </summary>
    public string JsonData { get; set; } = string.Empty;
}

/// <summary>
/// DTO for bulk upserting customer attributes by type
/// </summary>
public class UpsertCustomerAttributesByTypeDto
{
    /// <summary>
    /// Attribute type
    /// </summary>
    public CustomerAttributeType AttributeType { get; set; }

    /// <summary>
    /// List of attributes to upsert (replaces all existing attributes of this type)
    /// </summary>
    public List<CustomerAttributeItemDto> Items { get; set; } = new();
}

/// <summary>
/// Individual attribute item for bulk operations
/// </summary>
public class CustomerAttributeItemDto
{
    /// <summary>
    /// Optional ID for updating existing attribute
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Display order
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// JSON data
    /// </summary>
    public string JsonData { get; set; } = string.Empty;
}
