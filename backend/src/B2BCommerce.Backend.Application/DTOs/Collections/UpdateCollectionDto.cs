namespace B2BCommerce.Backend.Application.DTOs.Collections;

/// <summary>
/// DTO for updating a collection (Type is immutable)
/// </summary>
public class UpdateCollectionDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
