namespace B2BCommerce.Backend.Application.DTOs.Collections;

/// <summary>
/// DTO for setting filter criteria on a dynamic collection
/// </summary>
public class SetCollectionFiltersDto
{
    public List<Guid>? CategoryIds { get; set; }
    public List<Guid>? BrandIds { get; set; }
    public List<Guid>? ProductTypeIds { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
