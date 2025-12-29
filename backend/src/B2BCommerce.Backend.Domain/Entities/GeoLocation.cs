using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Exceptions;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Represents a geographic location in a hierarchical structure.
/// Inherits from ExternalEntity to support external system synchronization.
/// </summary>
public class GeoLocation : ExternalEntity, IAggregateRoot
{
    /// <summary>
    /// FK to the type of this location (Country, State, City, etc.)
    /// </summary>
    public Guid GeoLocationTypeId { get; private set; }

    /// <summary>
    /// ISO code or other standard code for the location
    /// </summary>
    public string Code { get; private set; }

    /// <summary>
    /// Display name of the location
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// FK to parent location for hierarchy (null for root locations like countries)
    /// </summary>
    public Guid? ParentId { get; private set; }

    /// <summary>
    /// Latitude coordinate
    /// </summary>
    public decimal? Latitude { get; private set; }

    /// <summary>
    /// Longitude coordinate
    /// </summary>
    public decimal? Longitude { get; private set; }

    /// <summary>
    /// Materialized path for hierarchy (e.g., "1/25/36")
    /// </summary>
    public string? Path { get; private set; }

    /// <summary>
    /// Human-readable path (e.g., "Türkiye/İstanbul/Kadıköy")
    /// </summary>
    public string? PathName { get; private set; }

    /// <summary>
    /// Depth in hierarchy (0 for root)
    /// </summary>
    public int Depth { get; private set; }

    /// <summary>
    /// Additional metadata as JSON
    /// </summary>
    public string? Metadata { get; private set; }

    // Navigation properties
    public GeoLocationType Type { get; private set; } = null!;
    public GeoLocation? Parent { get; private set; }

    private readonly List<GeoLocation> _children = new();
    public IReadOnlyCollection<GeoLocation> Children => _children.AsReadOnly();

    private GeoLocation() // For EF Core
    {
        Code = string.Empty;
        Name = string.Empty;
    }

    /// <summary>
    /// Creates a new GeoLocation instance
    /// </summary>
    public static GeoLocation Create(
        Guid geoLocationTypeId,
        string code,
        string name,
        Guid? parentId = null,
        decimal? latitude = null,
        decimal? longitude = null,
        string? path = null,
        string? pathName = null,
        int depth = 0,
        string? metadata = null)
    {
        if (geoLocationTypeId == Guid.Empty)
        {
            throw new DomainException("GeoLocationTypeId is required");
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("Location code is required");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Location name is required");
        }

        var location = new GeoLocation
        {
            GeoLocationTypeId = geoLocationTypeId,
            Code = code.Trim(),
            Name = name.Trim(),
            ParentId = parentId,
            Latitude = latitude,
            Longitude = longitude,
            Path = path,
            PathName = pathName,
            Depth = depth,
            Metadata = metadata
        };

        // Auto-populate ExternalId for Integration API compatibility
        location.SetExternalIdentifiers(externalCode: null, externalId: location.Id.ToString());

        return location;
    }

    /// <summary>
    /// Creates a GeoLocation from an external system (LOGO ERP).
    /// Uses ExternalId as the primary upsert key.
    /// </summary>
    public static GeoLocation CreateFromExternal(
        string externalId,
        Guid geoLocationTypeId,
        string code,
        string name,
        Guid? parentId = null,
        decimal? latitude = null,
        decimal? longitude = null,
        string? path = null,
        string? pathName = null,
        int depth = 0,
        string? metadata = null,
        string? externalCode = null)
    {
        if (string.IsNullOrWhiteSpace(externalId))
        {
            throw new ArgumentException("External ID is required", nameof(externalId));
        }

        var location = Create(geoLocationTypeId, code, name, parentId, latitude, longitude, path, pathName, depth, metadata);

        // Use base class helper for consistent initialization
        InitializeFromExternal(location, externalId, externalCode);

        return location;
    }

    /// <summary>
    /// Updates the location details
    /// </summary>
    public void Update(
        string code,
        string name,
        decimal? latitude = null,
        decimal? longitude = null,
        string? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("Location code is required");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Location name is required");
        }

        Code = code.Trim();
        Name = name.Trim();
        Latitude = latitude;
        Longitude = longitude;
        Metadata = metadata;
    }

    /// <summary>
    /// Updates location from external system sync (LOGO ERP).
    /// </summary>
    public void UpdateFromExternal(
        string code,
        string name,
        decimal? latitude = null,
        decimal? longitude = null,
        string? metadata = null,
        string? externalCode = null)
    {
        Update(code, name, latitude, longitude, metadata);

        if (externalCode is not null)
        {
            SetExternalIdentifiers(externalCode, ExternalId);
        }

        MarkAsSynced();
    }

    /// <summary>
    /// Updates the hierarchy information (path, pathName, depth)
    /// </summary>
    public void UpdateHierarchy(string? path, string? pathName, int depth)
    {
        Path = path;
        PathName = pathName;
        Depth = depth;
    }

    /// <summary>
    /// Sets the parent location and updates hierarchy info
    /// </summary>
    public void SetParent(Guid? parentId, string? parentPath, string? parentPathName, int parentDepth)
    {
        ParentId = parentId;

        if (parentId.HasValue)
        {
            Path = string.IsNullOrEmpty(parentPath) ? Id.ToString() : $"{parentPath}/{Id}";
            PathName = string.IsNullOrEmpty(parentPathName) ? Name : $"{parentPathName}/{Name}";
            Depth = parentDepth + 1;
        }
        else
        {
            Path = Id.ToString();
            PathName = Name;
            Depth = 0;
        }
    }
}
