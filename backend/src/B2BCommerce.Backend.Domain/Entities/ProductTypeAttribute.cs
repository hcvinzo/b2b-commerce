using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Exceptions;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Junction entity linking ProductType to AttributeDefinition
/// Defines which attributes belong to which product types
/// </summary>
public class ProductTypeAttribute : BaseEntity
{
    /// <summary>
    /// FK to the ProductType
    /// </summary>
    public Guid ProductTypeId { get; private set; }

    /// <summary>
    /// FK to the AttributeDefinition
    /// </summary>
    public Guid AttributeDefinitionId { get; private set; }

    /// <summary>
    /// Whether this attribute is required for products of this type
    /// Overrides the AttributeDefinition.IsRequired default
    /// </summary>
    public bool IsRequired { get; private set; }

    /// <summary>
    /// Display order for this attribute within this product type
    /// </summary>
    public int DisplayOrder { get; private set; }

    // Navigation properties
    public ProductType ProductType { get; private set; } = null!;
    public AttributeDefinition AttributeDefinition { get; private set; } = null!;

    private ProductTypeAttribute() // For EF Core
    {
    }

    /// <summary>
    /// Creates a new ProductTypeAttribute instance
    /// </summary>
    internal static ProductTypeAttribute Create(
        Guid productTypeId,
        Guid attributeDefinitionId,
        bool isRequired = false,
        int displayOrder = 0)
    {
        if (productTypeId == Guid.Empty)
        {
            throw new DomainException("ProductTypeId is required");
        }

        if (attributeDefinitionId == Guid.Empty)
        {
            throw new DomainException("AttributeDefinitionId is required");
        }

        return new ProductTypeAttribute
        {
            ProductTypeId = productTypeId,
            AttributeDefinitionId = attributeDefinitionId,
            IsRequired = isRequired,
            DisplayOrder = displayOrder
        };
    }

    /// <summary>
    /// Updates the attribute settings for this product type
    /// </summary>
    public void Update(bool isRequired, int displayOrder)
    {
        IsRequired = isRequired;
        DisplayOrder = displayOrder;
    }
}
