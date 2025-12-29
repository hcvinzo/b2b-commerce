namespace B2BCommerce.Backend.Application.DTOs.GeoLocations;

/// <summary>
/// GeoLocationType data transfer object
/// </summary>
public class GeoLocationTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public int LocationCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new GeoLocationType
/// </summary>
public class CreateGeoLocationTypeDto
{
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

/// <summary>
/// DTO for updating a GeoLocationType
/// </summary>
public class UpdateGeoLocationTypeDto
{
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
