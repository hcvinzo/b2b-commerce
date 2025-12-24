namespace B2BCommerce.Backend.Application.DTOs.Collections;

/// <summary>
/// DTO for collection filter criteria (dynamic collections)
/// </summary>
public class CollectionFilterDto
{
    public List<Guid>? CategoryIds { get; set; }
    public List<Guid>? BrandIds { get; set; }
    public List<Guid>? ProductTypeIds { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
