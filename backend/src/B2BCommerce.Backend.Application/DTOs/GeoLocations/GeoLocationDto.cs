namespace B2BCommerce.Backend.Application.DTOs.GeoLocations;

/// <summary>
/// GeoLocation data transfer object
/// </summary>
public class GeoLocationDto
{
    public Guid Id { get; set; }
    public Guid GeoLocationTypeId { get; set; }
    public string GeoLocationTypeName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public string? ParentName { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Path { get; set; }
    public string? PathName { get; set; }
    public int Depth { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // External entity fields
    public string? ExternalCode { get; set; }
    public string? ExternalId { get; set; }
    public DateTime? LastSyncedAt { get; set; }
}

/// <summary>
/// GeoLocation list item for paginated responses
/// </summary>
public class GeoLocationListDto
{
    public Guid Id { get; set; }
    public Guid GeoLocationTypeId { get; set; }
    public string GeoLocationTypeName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public string? ParentName { get; set; }
    public string? PathName { get; set; }
    public int Depth { get; set; }
    public int ChildCount { get; set; }

    // External entity fields
    public string? ExternalCode { get; set; }
    public string? ExternalId { get; set; }
}

/// <summary>
/// GeoLocation tree node for hierarchical responses
/// </summary>
public class GeoLocationTreeDto
{
    public Guid Id { get; set; }
    public Guid GeoLocationTypeId { get; set; }
    public string GeoLocationTypeName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int Depth { get; set; }
    public List<GeoLocationTreeDto> Children { get; set; } = new();

    // External entity fields
    public string? ExternalCode { get; set; }
    public string? ExternalId { get; set; }
}

/// <summary>
/// DTO for creating a new GeoLocation
/// </summary>
public class CreateGeoLocationDto
{
    public Guid GeoLocationTypeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Metadata { get; set; }
}

/// <summary>
/// DTO for updating a GeoLocation
/// </summary>
public class UpdateGeoLocationDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Metadata { get; set; }
}
