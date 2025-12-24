using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.DTOs.Products;

/// <summary>
/// DTO for a product relation
/// </summary>
public class ProductRelationDto
{
    public Guid Id { get; set; }
    public Guid RelatedProductId { get; set; }
    public string RelatedProductName { get; set; } = string.Empty;
    public string RelatedProductSku { get; set; } = string.Empty;
    public string? RelatedProductImageUrl { get; set; }
    public decimal RelatedProductPrice { get; set; }
    public bool RelatedProductIsActive { get; set; }
    public ProductRelationType RelationType { get; set; }
    public int DisplayOrder { get; set; }
}

/// <summary>
/// DTO for grouped product relations by type
/// </summary>
public class ProductRelationsGroupDto
{
    public ProductRelationType RelationType { get; set; }
    public string RelationTypeName { get; set; } = string.Empty;
    public List<ProductRelationDto> Relations { get; set; } = new();
}

/// <summary>
/// Input DTO for a single related product when setting relations
/// </summary>
public class RelatedProductInputDto
{
    public Guid ProductId { get; set; }
    public int DisplayOrder { get; set; }
}
