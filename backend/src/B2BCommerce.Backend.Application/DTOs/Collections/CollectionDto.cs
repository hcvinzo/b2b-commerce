using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.DTOs.Collections;

/// <summary>
/// Full collection DTO with all details
/// </summary>
public class CollectionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public CollectionType Type { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrentlyActive { get; set; }
    public int ProductCount { get; set; }
    public CollectionFilterDto? Filter { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Collection DTO for list views (lighter weight)
/// </summary>
public class CollectionListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public CollectionType Type { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public int ProductCount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrentlyActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
