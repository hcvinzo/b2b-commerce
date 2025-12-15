using System.Text.RegularExpressions;
using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Exceptions;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Category entity for product categorization
/// </summary>
public class Category : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public string? ImageUrl { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }

    /// <summary>
    /// URL-friendly identifier for the category
    /// </summary>
    public string Slug { get; private set; }

    /// <summary>
    /// Default ProductType suggested for products in this category
    /// </summary>
    public Guid? DefaultProductTypeId { get; private set; }

    // Navigation properties
    public Category? ParentCategory { get; set; }
    public ICollection<Category> SubCategories { get; set; }
    public ICollection<Product> Products { get; set; }
    public ProductType? DefaultProductType { get; private set; }

    // Multi-category support
    private readonly List<ProductCategory> _productCategories = new();
    public IReadOnlyCollection<ProductCategory> ProductCategories => _productCategories.AsReadOnly();

    private Category() // For EF Core
    {
        Name = string.Empty;
        Description = string.Empty;
        Slug = string.Empty;
        SubCategories = new List<Category>();
        Products = new List<Product>();
    }

    /// <summary>
    /// Creates a new Category instance
    /// </summary>
    public static Category Create(string name, string description, string? slug = null, Guid? parentCategoryId = null, int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name cannot be null or empty", nameof(name));
        }

        var category = new Category
        {
            Name = name,
            Description = description ?? string.Empty,
            Slug = slug ?? GenerateSlug(name),
            ParentCategoryId = parentCategoryId,
            DisplayOrder = displayOrder,
            IsActive = true,
            SubCategories = new List<Category>(),
            Products = new List<Product>()
        };

        return category;
    }

    [Obsolete("Use Category.Create() factory method instead")]
    public Category(string name, string description, Guid? parentCategoryId = null, int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name cannot be null or empty", nameof(name));
        }

        Name = name;
        Description = description ?? string.Empty;
        Slug = GenerateSlug(name);
        ParentCategoryId = parentCategoryId;
        DisplayOrder = displayOrder;
        IsActive = true;
        SubCategories = new List<Category>();
        Products = new List<Product>();
    }

    public void Update(string name, string description, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name cannot be null or empty", nameof(name));
        }

        Name = name;
        Description = description ?? string.Empty;
        DisplayOrder = displayOrder;
    }

    public void SetImage(string imageUrl)
    {
        ImageUrl = imageUrl;
    }

    /// <summary>
    /// Updates the slug
    /// </summary>
    public void SetSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ArgumentException("Slug cannot be null or empty", nameof(slug));
        }

        Slug = slug.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Sets the default product type for this category
    /// </summary>
    public void SetDefaultProductType(Guid? productTypeId)
    {
        DefaultProductTypeId = productTypeId;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Generates a URL-friendly slug from a name
    /// </summary>
    private static string GenerateSlug(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        // Convert to lowercase
        var slug = name.ToLowerInvariant();

        // Replace Turkish characters
        slug = slug
            .Replace("ı", "i")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ö", "o")
            .Replace("ç", "c")
            .Replace("İ", "i")
            .Replace("Ğ", "g")
            .Replace("Ü", "u")
            .Replace("Ş", "s")
            .Replace("Ö", "o")
            .Replace("Ç", "c");

        // Replace spaces and special characters with hyphens
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");

        // Trim hyphens from ends
        slug = slug.Trim('-');

        return slug;
    }
}
