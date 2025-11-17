using B2BCommerce.Backend.Domain.Common;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Brand entity for product brands
/// </summary>
public class Brand : BaseEntity, IAggregateRoot
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
}
