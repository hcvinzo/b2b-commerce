using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Exceptions;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Defines a product type that determines which attributes a product has
/// (e.g., "Memory Card", "SSD", "USB Flash Drive")
/// </summary>
public class ProductType : BaseEntity, IAggregateRoot
{
    /// <summary>
    /// Unique code for the product type (e.g., "memory_card", "ssd")
    /// </summary>
    public string Code { get; private set; }

    /// <summary>
    /// Display name
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Admin description for this product type
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Whether new products can use this type
    /// </summary>
    public bool IsActive { get; private set; }

    // Navigation properties
    private readonly List<ProductTypeAttribute> _attributes = new();
    public IReadOnlyCollection<ProductTypeAttribute> Attributes => _attributes.AsReadOnly();

    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    private ProductType() // For EF Core
    {
        Code = string.Empty;
        Name = string.Empty;
    }

    /// <summary>
    /// Creates a new ProductType instance
    /// </summary>
    public static ProductType Create(
        string code,
        string name,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("Product type code is required");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Product type name is required");
        }

        return new ProductType
        {
            Code = code.Trim().ToLowerInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            IsActive = true
        };
    }

    /// <summary>
    /// Updates the product type details
    /// </summary>
    public void Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Product type name is required");
        }

        Name = name.Trim();
        Description = description?.Trim();
    }

    /// <summary>
    /// Activates the product type
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates the product type (existing products keep the type)
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Adds an attribute to this product type
    /// </summary>
    public ProductTypeAttribute AddAttribute(
        Guid attributeDefinitionId,
        bool isRequired = false,
        int displayOrder = 0)
    {
        if (attributeDefinitionId == Guid.Empty)
        {
            throw new DomainException("Attribute definition ID is required");
        }

        if (_attributes.Any(a => a.AttributeDefinitionId == attributeDefinitionId))
        {
            throw new DomainException("This attribute is already assigned to this product type");
        }

        var productTypeAttribute = ProductTypeAttribute.Create(Id, attributeDefinitionId, isRequired, displayOrder);
        _attributes.Add(productTypeAttribute);
        return productTypeAttribute;
    }

    /// <summary>
    /// Removes an attribute from this product type
    /// </summary>
    public void RemoveAttribute(Guid attributeDefinitionId)
    {
        var attribute = _attributes.FirstOrDefault(a => a.AttributeDefinitionId == attributeDefinitionId);
        if (attribute is not null)
        {
            _attributes.Remove(attribute);
        }
    }

    /// <summary>
    /// Updates an attribute's settings for this product type
    /// </summary>
    public void UpdateAttribute(Guid attributeDefinitionId, bool isRequired, int displayOrder)
    {
        var attribute = _attributes.FirstOrDefault(a => a.AttributeDefinitionId == attributeDefinitionId);
        if (attribute is null)
        {
            throw new DomainException("Attribute not found in this product type");
        }

        attribute.Update(isRequired, displayOrder);
    }

    /// <summary>
    /// Checks if this product type has a specific attribute
    /// </summary>
    public bool HasAttribute(Guid attributeDefinitionId)
    {
        return _attributes.Any(a => a.AttributeDefinitionId == attributeDefinitionId);
    }

    /// <summary>
    /// Gets all required attribute IDs for this product type
    /// </summary>
    public IEnumerable<Guid> GetRequiredAttributeIds()
    {
        return _attributes.Where(a => a.IsRequired).Select(a => a.AttributeDefinitionId);
    }
}
