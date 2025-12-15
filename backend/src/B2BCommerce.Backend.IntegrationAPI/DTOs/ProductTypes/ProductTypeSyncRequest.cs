using System.ComponentModel.DataAnnotations;

namespace B2BCommerce.Backend.IntegrationAPI.DTOs.ProductTypes;

/// <summary>
/// Request DTO for syncing a product type from external system.
/// Uses ExtId (primary) or Id (internal) as the upsert key.
/// One of ExtId or Id is required.
/// </summary>
public class ProductTypeSyncRequest
{
    /// <summary>
    /// Internal ID (optional for upsert - if provided without ExtId, ExtId will be set to Id.ToString())
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// External system ID (PRIMARY upsert key)
    /// One of ExtId or Id is required.
    /// </summary>
    [StringLength(100)]
    public string? ExtId { get; set; }

    /// <summary>
    /// External system code (OPTIONAL reference)
    /// </summary>
    [StringLength(100)]
    public string? ExtCode { get; set; }

    /// <summary>
    /// Unique code for the product type (required, e.g., "memory_card", "ssd")
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Code { get; set; } = null!;

    /// <summary>
    /// Display name (required)
    /// </summary>
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Admin description
    /// </summary>
    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Whether new products can use this type
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Attributes to assign to this product type (full replacement on update)
    /// </summary>
    [MaxLength(100)]
    public List<ProductTypeAttributeSyncRequest>? Attributes { get; set; }
}

/// <summary>
/// Request DTO for syncing a product type attribute assignment
/// </summary>
public class ProductTypeAttributeSyncRequest
{
    /// <summary>
    /// Attribute definition ID (optional if ExtId or Code provided)
    /// </summary>
    public Guid? AttributeId { get; set; }

    /// <summary>
    /// Attribute definition external ID (for lookup by external system)
    /// </summary>
    [StringLength(100)]
    public string? AttributeExtId { get; set; }

    /// <summary>
    /// Attribute code (for lookup by code)
    /// </summary>
    [StringLength(100)]
    public string? AttributeCode { get; set; }

    /// <summary>
    /// Whether this attribute is required for products of this type
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// Display order within this product type
    /// </summary>
    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; } = 0;
}

/// <summary>
/// Request DTO for bulk syncing product types
/// </summary>
public class BulkProductTypeSyncRequest
{
    /// <summary>
    /// List of product types to sync
    /// </summary>
    [Required]
    [MinLength(1)]
    [MaxLength(100)]
    public List<ProductTypeSyncRequest> ProductTypes { get; set; } = new();
}
