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
    /// Attribute definition identifier
    /// </summary>
    public Guid AttributeDefinitionId { get; set; }

    /// <summary>
    /// Attribute definition code
    /// </summary>
    public string AttributeCode { get; set; } = string.Empty;

    /// <summary>
    /// Attribute definition name for display
    /// </summary>
    public string AttributeName { get; set; } = string.Empty;

    /// <summary>
    /// JSON value containing the attribute data
    /// </summary>
    public string Value { get; set; } = string.Empty;

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
    /// Attribute definition identifier
    /// </summary>
    public Guid AttributeDefinitionId { get; set; }

    /// <summary>
    /// JSON value containing the attribute data
    /// </summary>
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating a customer attribute
/// </summary>
public class UpdateCustomerAttributeDto
{
    /// <summary>
    /// JSON value containing the attribute data
    /// </summary>
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// DTO for bulk upserting customer attributes by definition
/// </summary>
public class UpsertCustomerAttributesByDefinitionDto
{
    /// <summary>
    /// Attribute definition identifier
    /// </summary>
    public Guid AttributeDefinitionId { get; set; }

    /// <summary>
    /// List of attributes to upsert (replaces all existing attributes of this definition)
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
    /// JSON value containing the attribute data
    /// </summary>
    public string Value { get; set; } = string.Empty;
}
