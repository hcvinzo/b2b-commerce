using System.ComponentModel.DataAnnotations;

namespace B2BCommerce.Backend.IntegrationAPI.DTOs.ProductTypes;

/// <summary>
/// Request DTO for syncing a product type from external system.
/// Id = ExternalId (string) - the primary upsert key.
/// Code is the business code (not ExternalCode).
/// </summary>
public class ProductTypeSyncRequest
{
    /// <summary>
    /// External ID (PRIMARY upsert key).
    /// This is the ID from the source system (LOGO ERP).
    /// Required for creating new product types.
    /// </summary>
    [StringLength(100)]
    public string? Id { get; set; }

    /// <summary>
    /// Unique business code for the product type (required, e.g., "memory_card", "ssd").
    /// Used as fallback for matching if Id is not provided.
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
/// Request DTO for syncing a product type attribute assignment.
/// AttributeId = Attribute's ExternalId (string).
/// </summary>
public class ProductTypeAttributeSyncRequest
{
    /// <summary>
    /// Attribute's external ID (primary lookup).
    /// This is the ID from the source system (LOGO ERP).
    /// </summary>
    [StringLength(100)]
    public string? AttributeId { get; set; }

    /// <summary>
    /// Attribute business code (fallback lookup if AttributeId not provided)
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
