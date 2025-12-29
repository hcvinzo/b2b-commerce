using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Exceptions;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Defines types of geographic locations (e.g., Country, State, City, District)
/// </summary>
public class GeoLocationType : BaseEntity, IAggregateRoot
{
    /// <summary>
    /// Name of the location type (e.g., "Country", "State", "City", "District", "Neighborhood")
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Display order for sorting location types hierarchically
    /// </summary>
    public int DisplayOrder { get; private set; }

    // Navigation properties
    private readonly List<GeoLocation> _locations = new();
    public IReadOnlyCollection<GeoLocation> Locations => _locations.AsReadOnly();

    private GeoLocationType() // For EF Core
    {
        Name = string.Empty;
    }

    /// <summary>
    /// Creates a new GeoLocationType instance
    /// </summary>
    public static GeoLocationType Create(string name, int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Location type name is required");
        }

        return new GeoLocationType
        {
            Name = name.Trim(),
            DisplayOrder = displayOrder
        };
    }

    /// <summary>
    /// Updates the location type details
    /// </summary>
    public void Update(string name, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Location type name is required");
        }

        Name = name.Trim();
        DisplayOrder = displayOrder;
    }
}
