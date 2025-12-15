using B2BCommerce.Backend.Application.DTOs.Attributes;

namespace B2BCommerce.Backend.Application.DTOs.ProductTypes;

/// <summary>
/// Product type data transfer object for output
/// </summary>
public class ProductTypeDto
{
    /// <summary>
    /// Product type identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Unique code (e.g., "memory_card", "ssd")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Admin description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether new products can use this type
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Attributes associated with this product type
    /// </summary>
    public List<ProductTypeAttributeDto> Attributes { get; set; } = new();

    /// <summary>
    /// External system ID (primary key for ERP integration)
    /// </summary>
    public string? ExternalId { get; set; }

    /// <summary>
    /// External system code (optional reference)
    /// </summary>
    public string? ExternalCode { get; set; }

    /// <summary>
    /// When the entity was last synchronized with the external system
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }

    /// <summary>
    /// Date created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
