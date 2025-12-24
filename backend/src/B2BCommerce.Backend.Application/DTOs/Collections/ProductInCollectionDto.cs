namespace B2BCommerce.Backend.Application.DTOs.Collections;

/// <summary>
/// DTO for a product within a collection
/// </summary>
public class ProductInCollectionDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public decimal ProductPrice { get; set; }
    public bool ProductIsActive { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsFeatured { get; set; }
}

/// <summary>
/// Input DTO for setting products in a collection
/// </summary>
public class ProductInCollectionInputDto
{
    public Guid ProductId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsFeatured { get; set; }
}

/// <summary>
/// DTO for setting all products in a collection
/// </summary>
public class SetCollectionProductsDto
{
    public List<ProductInCollectionInputDto> Products { get; set; } = new();
}
