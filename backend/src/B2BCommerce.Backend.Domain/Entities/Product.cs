using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.Exceptions;
using B2BCommerce.Backend.Domain.ValueObjects;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Product entity representing items that can be ordered
/// </summary>
public class Product : ExternalEntity, IAggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string SKU { get; private set; }

    /// <summary>
    /// FK to ProductType - defines which attributes this product can have
    /// </summary>
    public Guid? ProductTypeId { get; private set; }

    /// <summary>
    /// Legacy: Primary category ID (kept for backward compatibility)
    /// New code should use ProductCategories with IsPrimary=true
    /// </summary>
    public Guid CategoryId { get; private set; }

    public Guid? BrandId { get; private set; }

    // Pricing
    public Money ListPrice { get; private set; }
    public Money? Tier1Price { get; private set; }
    public Money? Tier2Price { get; private set; }
    public Money? Tier3Price { get; private set; }
    public Money? Tier4Price { get; private set; }
    public Money? Tier5Price { get; private set; }

    // Stock
    public int StockQuantity { get; private set; }
    public int MinimumOrderQuantity { get; private set; }

    /// <summary>
    /// Product lifecycle status (Draft, Active, Inactive)
    /// </summary>
    public ProductStatus Status { get; private set; }

    /// <summary>
    /// Computed property for backward compatibility.
    /// Returns true only when Status is Active.
    /// </summary>
    public bool IsActive => Status == ProductStatus.Active;

    public bool IsSerialTracked { get; private set; }

    // Tax
    public decimal TaxRate { get; private set; }

    // Images and metadata
    public string? MainImageUrl { get; private set; }
    public List<string> ImageUrls { get; private set; }
    public Dictionary<string, string> Specifications { get; private set; }

    // Weight and dimensions
    public decimal? Weight { get; private set; }
    public decimal? Length { get; private set; }
    public decimal? Width { get; private set; }
    public decimal? Height { get; private set; }

    // Navigation properties
    public ProductType? ProductType { get; private set; }
    public Category? Category { get; set; }
    public Brand? Brand { get; set; }

    // Multi-category support
    private readonly List<ProductCategory> _productCategories = new();
    public IReadOnlyCollection<ProductCategory> ProductCategories => _productCategories.AsReadOnly();

    // Attribute values
    private readonly List<ProductAttributeValue> _attributeValues = new();
    public IReadOnlyCollection<ProductAttributeValue> AttributeValues => _attributeValues.AsReadOnly();

    private Product() // For EF Core
    {
        Name = string.Empty;
        Description = string.Empty;
        SKU = string.Empty;
        ListPrice = Money.Zero("USD");
        ImageUrls = new List<string>();
        Specifications = new Dictionary<string, string>();
    }

    /// <summary>
    /// Creates a new Product instance.
    /// Product will be created as Active if all required fields are provided,
    /// otherwise as Draft.
    /// </summary>
    public static Product Create(
        string name,
        string description,
        string sku,
        Guid categoryId,
        Money listPrice,
        int stockQuantity,
        int minimumOrderQuantity = 1,
        decimal taxRate = 0.18m,
        Guid? productTypeId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name cannot be null or empty", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new ArgumentException("Product SKU cannot be null or empty", nameof(sku));
        }

        if (stockQuantity < 0)
        {
            throw new ArgumentException("Stock quantity cannot be negative", nameof(stockQuantity));
        }

        if (minimumOrderQuantity < 1)
        {
            throw new ArgumentException("Minimum order quantity must be at least 1", nameof(minimumOrderQuantity));
        }

        var product = new Product
        {
            Name = name,
            Description = description ?? string.Empty,
            SKU = sku,
            CategoryId = categoryId,
            ListPrice = listPrice ?? Money.Zero("TRY"),
            StockQuantity = stockQuantity,
            MinimumOrderQuantity = minimumOrderQuantity,
            TaxRate = taxRate,
            ProductTypeId = productTypeId,
            IsSerialTracked = false,
            ImageUrls = new List<string>(),
            Specifications = new Dictionary<string, string>()
        };

        // Determine initial status based on required fields
        product.Status = product.CanActivate() ? ProductStatus.Active : ProductStatus.Draft;

        return product;
    }

    [Obsolete("Use Product.Create() factory method instead")]
    public Product(
        string name,
        string description,
        string sku,
        Guid categoryId,
        Money listPrice,
        int stockQuantity,
        int minimumOrderQuantity = 1,
        decimal taxRate = 0.18m)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name cannot be null or empty", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new ArgumentException("Product SKU cannot be null or empty", nameof(sku));
        }

        if (stockQuantity < 0)
        {
            throw new ArgumentException("Stock quantity cannot be negative", nameof(stockQuantity));
        }

        if (minimumOrderQuantity < 1)
        {
            throw new ArgumentException("Minimum order quantity must be at least 1", nameof(minimumOrderQuantity));
        }

        Name = name;
        Description = description ?? string.Empty;
        SKU = sku;
        CategoryId = categoryId;
        ListPrice = listPrice ?? throw new ArgumentNullException(nameof(listPrice));
        StockQuantity = stockQuantity;
        MinimumOrderQuantity = minimumOrderQuantity;
        TaxRate = taxRate;
        Status = ProductStatus.Draft; // Start as Draft, needs validation to activate
        IsSerialTracked = false;
        ImageUrls = new List<string>();
        Specifications = new Dictionary<string, string>();
    }

    public void UpdateBasicInfo(string name, string description, Guid categoryId, Guid? brandId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be null or empty", nameof(name));

        Name = name;
        Description = description ?? string.Empty;
        CategoryId = categoryId;
        BrandId = brandId;
    }

    public void UpdatePricing(Money listPrice, Money? tier1Price = null, Money? tier2Price = null,
        Money? tier3Price = null, Money? tier4Price = null, Money? tier5Price = null)
    {
        ListPrice = listPrice ?? throw new ArgumentNullException(nameof(listPrice));
        Tier1Price = tier1Price;
        Tier2Price = tier2Price;
        Tier3Price = tier3Price;
        Tier4Price = tier4Price;
        Tier5Price = tier5Price;
    }

    public Money GetPriceForTier(PriceTier tier)
    {
        return tier switch
        {
            PriceTier.List => ListPrice,
            PriceTier.Tier1 => Tier1Price ?? ListPrice,
            PriceTier.Tier2 => Tier2Price ?? ListPrice,
            PriceTier.Tier3 => Tier3Price ?? ListPrice,
            PriceTier.Tier4 => Tier4Price ?? ListPrice,
            PriceTier.Tier5 => Tier5Price ?? ListPrice,
            _ => ListPrice
        };
    }

    public void UpdateStock(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative", nameof(quantity));

        StockQuantity = quantity;
    }

    public void ReserveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        if (StockQuantity < quantity)
            throw new OutOfStockException(Id, quantity, StockQuantity);

        StockQuantity -= quantity;
    }

    public void ReleaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        StockQuantity += quantity;
    }

    #region Status Management

    /// <summary>
    /// Checks if the product has all required fields to be activated.
    /// Required fields: CategoryId, ProductTypeId, ListPrice > 0, TaxRate set
    /// </summary>
    public bool CanActivate()
    {
        return CategoryId != Guid.Empty
            && ProductTypeId.HasValue
            && ProductTypeId.Value != Guid.Empty
            && ListPrice is not null
            && ListPrice.Amount > 0
            && TaxRate >= 0;
    }

    /// <summary>
    /// Gets the list of missing fields that prevent activation
    /// </summary>
    public List<string> GetMissingActivationFields()
    {
        var missingFields = new List<string>();

        if (CategoryId == Guid.Empty)
        {
            missingFields.Add("Category");
        }

        if (!ProductTypeId.HasValue || ProductTypeId.Value == Guid.Empty)
        {
            missingFields.Add("ProductType");
        }

        if (ListPrice is null || ListPrice.Amount <= 0)
        {
            missingFields.Add("ListPrice");
        }

        if (TaxRate < 0)
        {
            missingFields.Add("TaxRate");
        }

        return missingFields;
    }

    /// <summary>
    /// Activates the product if all required fields are present
    /// </summary>
    /// <exception cref="DomainException">Thrown when required fields are missing</exception>
    public void Activate()
    {
        if (!CanActivate())
        {
            var missingFields = GetMissingActivationFields();
            throw new DomainException($"Cannot activate product. Missing required fields: {string.Join(", ", missingFields)}");
        }

        Status = ProductStatus.Active;
    }

    /// <summary>
    /// Deactivates the product (sets to Inactive status)
    /// </summary>
    public void Deactivate()
    {
        Status = ProductStatus.Inactive;
    }

    /// <summary>
    /// Sets the product to Draft status
    /// </summary>
    public void SetDraft()
    {
        Status = ProductStatus.Draft;
    }

    /// <summary>
    /// Sets the product status directly.
    /// For Active status, validates required fields first.
    /// </summary>
    public void SetStatus(ProductStatus status)
    {
        if (status == ProductStatus.Active && !CanActivate())
        {
            var missingFields = GetMissingActivationFields();
            throw new DomainException($"Cannot set product to Active status. Missing required fields: {string.Join(", ", missingFields)}");
        }

        Status = status;
    }

    #endregion

    public void SetMainImage(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Image URL cannot be null or empty", nameof(imageUrl));

        MainImageUrl = imageUrl;
    }

    public void AddImage(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Image URL cannot be null or empty", nameof(imageUrl));

        if (!ImageUrls.Contains(imageUrl))
            ImageUrls.Add(imageUrl);
    }

    public void RemoveImage(string imageUrl)
    {
        ImageUrls.Remove(imageUrl);
    }

    public void AddSpecification(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Specification key cannot be null or empty", nameof(key));

        Specifications[key] = value;
    }

    public void UpdateDimensions(decimal? weight, decimal? length, decimal? width, decimal? height)
    {
        Weight = weight;
        Length = length;
        Width = width;
        Height = height;
    }

    #region ProductType Management

    /// <summary>
    /// Sets the product type for this product
    /// </summary>
    public void SetProductType(Guid productTypeId)
    {
        if (productTypeId == Guid.Empty)
        {
            throw new DomainException("ProductTypeId is required");
        }

        ProductTypeId = productTypeId;
    }

    /// <summary>
    /// Clears the product type
    /// </summary>
    public void ClearProductType()
    {
        ProductTypeId = null;
    }

    #endregion

    #region Multi-Category Management

    /// <summary>
    /// Adds this product to a category
    /// </summary>
    public ProductCategory AddToCategory(Guid categoryId, bool isPrimary = false, int displayOrder = 0)
    {
        if (categoryId == Guid.Empty)
        {
            throw new DomainException("CategoryId is required");
        }

        if (_productCategories.Any(pc => pc.CategoryId == categoryId))
        {
            throw new DomainException("Product is already in this category");
        }

        // If this is primary, remove primary from other categories
        if (isPrimary)
        {
            foreach (var pc in _productCategories.Where(pc => pc.IsPrimary))
            {
                pc.RemovePrimaryStatus();
            }
        }

        var productCategory = ProductCategory.Create(Id, categoryId, isPrimary, displayOrder);
        _productCategories.Add(productCategory);

        // Update legacy CategoryId if this is primary
        if (isPrimary)
        {
            CategoryId = categoryId;
        }

        return productCategory;
    }

    /// <summary>
    /// Removes this product from a category
    /// </summary>
    public void RemoveFromCategory(Guid categoryId)
    {
        var productCategory = _productCategories.FirstOrDefault(pc => pc.CategoryId == categoryId);
        if (productCategory is not null)
        {
            _productCategories.Remove(productCategory);
        }
    }

    /// <summary>
    /// Sets a category as primary for this product
    /// </summary>
    public void SetPrimaryCategory(Guid categoryId)
    {
        var productCategory = _productCategories.FirstOrDefault(pc => pc.CategoryId == categoryId);
        if (productCategory is null)
        {
            throw new DomainException("Product is not in this category");
        }

        // Remove primary from other categories
        foreach (var pc in _productCategories.Where(pc => pc.IsPrimary))
        {
            pc.RemovePrimaryStatus();
        }

        productCategory.SetAsPrimary();

        // Update legacy CategoryId
        CategoryId = categoryId;
    }

    /// <summary>
    /// Gets the primary category ID
    /// </summary>
    public Guid? GetPrimaryCategoryId()
    {
        var primaryCategory = _productCategories.FirstOrDefault(pc => pc.IsPrimary);
        return primaryCategory?.CategoryId ?? (CategoryId != Guid.Empty ? CategoryId : null);
    }

    #endregion

    #region Attribute Value Management

    /// <summary>
    /// Sets or updates a text attribute value
    /// </summary>
    public void SetTextAttribute(Guid attributeDefinitionId, string value)
    {
        var existing = _attributeValues.FirstOrDefault(av => av.AttributeDefinitionId == attributeDefinitionId);
        if (existing is not null)
        {
            existing.UpdateTextValue(value);
        }
        else
        {
            _attributeValues.Add(ProductAttributeValue.CreateText(Id, attributeDefinitionId, value));
        }
    }

    /// <summary>
    /// Sets or updates a numeric attribute value
    /// </summary>
    public void SetNumericAttribute(Guid attributeDefinitionId, decimal value)
    {
        var existing = _attributeValues.FirstOrDefault(av => av.AttributeDefinitionId == attributeDefinitionId);
        if (existing is not null)
        {
            existing.UpdateNumericValue(value);
        }
        else
        {
            _attributeValues.Add(ProductAttributeValue.CreateNumber(Id, attributeDefinitionId, value));
        }
    }

    /// <summary>
    /// Sets or updates a select attribute value
    /// </summary>
    public void SetSelectAttribute(Guid attributeDefinitionId, Guid selectedValueId)
    {
        var existing = _attributeValues.FirstOrDefault(av => av.AttributeDefinitionId == attributeDefinitionId);
        if (existing is not null)
        {
            existing.UpdateSelectValue(selectedValueId);
        }
        else
        {
            _attributeValues.Add(ProductAttributeValue.CreateSelect(Id, attributeDefinitionId, selectedValueId));
        }
    }

    /// <summary>
    /// Sets or updates a multi-select attribute value
    /// </summary>
    public void SetMultiSelectAttribute(Guid attributeDefinitionId, List<Guid> selectedValueIds)
    {
        var existing = _attributeValues.FirstOrDefault(av => av.AttributeDefinitionId == attributeDefinitionId);
        if (existing is not null)
        {
            existing.UpdateMultiSelectValues(selectedValueIds);
        }
        else
        {
            _attributeValues.Add(ProductAttributeValue.CreateMultiSelect(Id, attributeDefinitionId, selectedValueIds));
        }
    }

    /// <summary>
    /// Sets or updates a boolean attribute value
    /// </summary>
    public void SetBooleanAttribute(Guid attributeDefinitionId, bool value)
    {
        var existing = _attributeValues.FirstOrDefault(av => av.AttributeDefinitionId == attributeDefinitionId);
        if (existing is not null)
        {
            existing.UpdateBooleanValue(value);
        }
        else
        {
            _attributeValues.Add(ProductAttributeValue.CreateBoolean(Id, attributeDefinitionId, value));
        }
    }

    /// <summary>
    /// Sets or updates a date attribute value
    /// </summary>
    public void SetDateAttribute(Guid attributeDefinitionId, DateTime value)
    {
        var existing = _attributeValues.FirstOrDefault(av => av.AttributeDefinitionId == attributeDefinitionId);
        if (existing is not null)
        {
            existing.UpdateDateValue(value);
        }
        else
        {
            _attributeValues.Add(ProductAttributeValue.CreateDate(Id, attributeDefinitionId, value));
        }
    }

    /// <summary>
    /// Removes an attribute value
    /// </summary>
    public void RemoveAttribute(Guid attributeDefinitionId)
    {
        var existing = _attributeValues.FirstOrDefault(av => av.AttributeDefinitionId == attributeDefinitionId);
        if (existing is not null)
        {
            _attributeValues.Remove(existing);
        }
    }

    /// <summary>
    /// Clears all attribute values
    /// </summary>
    public void ClearAttributeValues()
    {
        _attributeValues.Clear();
    }

    /// <summary>
    /// Gets the attribute value for a specific attribute
    /// </summary>
    public ProductAttributeValue? GetAttributeValue(Guid attributeDefinitionId)
    {
        return _attributeValues.FirstOrDefault(av => av.AttributeDefinitionId == attributeDefinitionId);
    }

    #endregion

    #region External System Integration

    /// <summary>
    /// Creates a product from an external system (LOGO ERP).
    /// Uses ExternalId as the primary upsert key.
    /// Product starts as Draft if required fields are missing, otherwise follows requestedStatus.
    /// </summary>
    public static Product CreateFromExternal(
        string externalId,
        string sku,
        string name,
        string description,
        Guid categoryId,
        Money listPrice,
        int stockQuantity,
        int minimumOrderQuantity = 1,
        decimal taxRate = 0.20m,
        Guid? brandId = null,
        Guid? productTypeId = null,
        ProductStatus? requestedStatus = null,
        string? externalCode = null)
    {
        if (string.IsNullOrWhiteSpace(externalId))
        {
            throw new ArgumentException("External ID is required", nameof(externalId));
        }

        var product = Create(name, description, sku, categoryId, listPrice, stockQuantity, minimumOrderQuantity, taxRate, productTypeId);

        product.BrandId = brandId;

        // Override status if explicitly requested and allowed
        if (requestedStatus.HasValue)
        {
            if (requestedStatus.Value == ProductStatus.Active && !product.CanActivate())
            {
                // Cannot activate, keep as Draft
                product.Status = ProductStatus.Draft;
            }
            else
            {
                product.Status = requestedStatus.Value;
            }
        }

        product.SetExternalIdentifiers(externalCode, externalId);
        product.MarkAsSynced();
        return product;
    }

    /// <summary>
    /// Creates a product from an external system with minimal required data.
    /// Always creates as Draft status for later completion.
    /// </summary>
    public static Product CreateDraftFromExternal(
        string externalId,
        string sku,
        string name,
        string? externalCode = null)
    {
        if (string.IsNullOrWhiteSpace(externalId))
        {
            throw new ArgumentException("External ID is required", nameof(externalId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name is required", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new ArgumentException("Product SKU is required", nameof(sku));
        }

        var product = new Product
        {
            Name = name,
            Description = string.Empty,
            SKU = sku,
            CategoryId = Guid.Empty,
            ListPrice = Money.Zero("TRY"),
            StockQuantity = 0,
            MinimumOrderQuantity = 1,
            TaxRate = 0,
            ProductTypeId = null,
            Status = ProductStatus.Draft,
            IsSerialTracked = false,
            ImageUrls = new List<string>(),
            Specifications = new Dictionary<string, string>()
        };

        product.SetExternalIdentifiers(externalCode, externalId);
        product.MarkAsSynced();
        return product;
    }

    /// <summary>
    /// Updates product from external system sync.
    /// Status handling: Active only if CanActivate(), otherwise stays Draft or becomes Inactive.
    /// </summary>
    public void UpdateFromExternal(
        string name,
        string description,
        Guid categoryId,
        Guid? brandId,
        Guid? productTypeId,
        ProductStatus? requestedStatus = null,
        string? externalCode = null)
    {
        UpdateBasicInfo(name, description, categoryId, brandId);

        if (productTypeId.HasValue)
        {
            SetProductType(productTypeId.Value);
        }
        else
        {
            ClearProductType();
        }

        // Handle status
        if (requestedStatus.HasValue)
        {
            if (requestedStatus.Value == ProductStatus.Active)
            {
                // Try to activate, will throw if missing fields
                if (CanActivate())
                {
                    Status = ProductStatus.Active;
                }
                // If can't activate, keep current status (likely Draft)
            }
            else
            {
                Status = requestedStatus.Value;
            }
        }
        else
        {
            // No explicit status - auto-determine based on data completeness
            if (CanActivate() && Status == ProductStatus.Draft)
            {
                Status = ProductStatus.Active;
            }
        }

        if (externalCode is not null)
        {
            SetExternalIdentifiers(externalCode, ExternalId);
        }

        MarkAsSynced();
    }

    #endregion
}
