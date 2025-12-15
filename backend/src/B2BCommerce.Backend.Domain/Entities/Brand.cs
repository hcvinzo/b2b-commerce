using B2BCommerce.Backend.Domain.Common;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Brand entity for product brands
/// </summary>
public class Brand : ExternalEntity, IAggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? WebsiteUrl { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation properties
    public ICollection<Product> Products { get; set; }

    private Brand() // For EF Core
    {
        Name = string.Empty;
        Description = string.Empty;
        Products = new List<Product>();
    }

    /// <summary>
    /// Creates a new Brand instance
    /// </summary>
    public static Brand Create(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Brand name cannot be null or empty", nameof(name));

        var brand = new Brand
        {
            Name = name,
            Description = description ?? string.Empty,
            IsActive = true,
            Products = new List<Product>()
        };

        return brand;
    }

    [Obsolete("Use Brand.Create() factory method instead")]
    public Brand(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Brand name cannot be null or empty", nameof(name));

        Name = name;
        Description = description ?? string.Empty;
        IsActive = true;
        Products = new List<Product>();
    }

    public void Update(string name, string description, string? websiteUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Brand name cannot be null or empty", nameof(name));

        Name = name;
        Description = description ?? string.Empty;
        WebsiteUrl = websiteUrl;
    }

    public void SetLogo(string logoUrl)
    {
        LogoUrl = logoUrl;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    #region External System Integration

    /// <summary>
    /// Creates a brand from an external system (LOGO ERP).
    /// Uses ExternalId as the primary upsert key.
    /// </summary>
    public static Brand CreateFromExternal(
        string externalId,
        string name,
        string description,
        string? logoUrl = null,
        string? websiteUrl = null,
        bool isActive = true,
        string? externalCode = null)
    {
        if (string.IsNullOrWhiteSpace(externalId))
        {
            throw new ArgumentException("External ID is required", nameof(externalId));
        }

        var brand = Create(name, description);
        brand.LogoUrl = logoUrl;
        brand.WebsiteUrl = websiteUrl;

        if (!isActive)
        {
            brand.Deactivate();
        }

        brand.SetExternalIdentifiers(externalCode, externalId);
        brand.MarkAsSynced();
        return brand;
    }

    /// <summary>
    /// Updates brand from external system sync
    /// </summary>
    public void UpdateFromExternal(
        string name,
        string description,
        string? logoUrl,
        string? websiteUrl,
        bool isActive,
        string? externalCode = null)
    {
        Update(name, description, websiteUrl);
        LogoUrl = logoUrl;

        if (isActive)
        {
            Activate();
        }
        else
        {
            Deactivate();
        }

        if (externalCode != null)
        {
            SetExternalIdentifiers(externalCode, ExternalId);
        }

        MarkAsSynced();
    }

    #endregion
}
