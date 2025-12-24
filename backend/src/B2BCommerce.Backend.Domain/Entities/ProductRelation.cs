using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.Exceptions;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Junction entity representing a relationship between two products.
/// Relationships are bidirectional - when A relates to B, B automatically relates to A.
/// Each relationship type is limited to 10 products per product.
/// </summary>
public class ProductRelation : BaseEntity
{
    /// <summary>
    /// Maximum number of related products per relationship type per product
    /// </summary>
    public const int MaxRelatedProductsPerType = 10;

    /// <summary>
    /// FK to the source product (the product this relation belongs to)
    /// </summary>
    public Guid SourceProductId { get; private set; }

    /// <summary>
    /// FK to the related product
    /// </summary>
    public Guid RelatedProductId { get; private set; }

    /// <summary>
    /// Type of relationship (Related, CrossSell, UpSell, Accessories)
    /// </summary>
    public ProductRelationType RelationType { get; private set; }

    /// <summary>
    /// Display order within this relationship type
    /// </summary>
    public int DisplayOrder { get; private set; }

    // Navigation properties
    public Product SourceProduct { get; private set; } = null!;
    public Product RelatedProduct { get; private set; } = null!;

    private ProductRelation() { } // For EF Core

    /// <summary>
    /// Creates a new product relation
    /// </summary>
    internal static ProductRelation Create(
        Guid sourceProductId,
        Guid relatedProductId,
        ProductRelationType relationType,
        int displayOrder = 0)
    {
        if (sourceProductId == Guid.Empty)
        {
            throw new DomainException("SourceProductId is required");
        }

        if (relatedProductId == Guid.Empty)
        {
            throw new DomainException("RelatedProductId is required");
        }

        if (sourceProductId == relatedProductId)
        {
            throw new DomainException("A product cannot be related to itself");
        }

        if (displayOrder < 0)
        {
            throw new DomainException("Display order cannot be negative");
        }

        return new ProductRelation
        {
            SourceProductId = sourceProductId,
            RelatedProductId = relatedProductId,
            RelationType = relationType,
            DisplayOrder = displayOrder
        };
    }

    /// <summary>
    /// Updates the display order
    /// </summary>
    public void UpdateDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
        {
            throw new DomainException("Display order cannot be negative");
        }

        DisplayOrder = displayOrder;
    }
}
