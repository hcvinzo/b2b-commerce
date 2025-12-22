namespace B2BCommerce.Backend.Domain.ValueObjects;

/// <summary>
/// Value object representing a physical address
/// </summary>
public class Address : IEquatable<Address>
{
    public string Street { get; private set; }
    public string District { get; private set; }
    public string Neighborhood { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string Country { get; private set; }
    public string PostalCode { get; private set; }

    public Address(
        string street,
        string city,
        string state,
        string country,
        string postalCode,
        string? district = null,
        string? neighborhood = null)
    {
        if (string.IsNullOrWhiteSpace(street))
        {
            throw new ArgumentException("Street cannot be null or empty", nameof(street));
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            throw new ArgumentException("City cannot be null or empty", nameof(city));
        }

        if (string.IsNullOrWhiteSpace(country))
        {
            throw new ArgumentException("Country cannot be null or empty", nameof(country));
        }

        if (string.IsNullOrWhiteSpace(postalCode))
        {
            throw new ArgumentException("Postal code cannot be null or empty", nameof(postalCode));
        }

        Street = street;
        City = city;
        State = state ?? string.Empty;
        Country = country;
        PostalCode = postalCode;
        District = district ?? string.Empty;
        Neighborhood = neighborhood ?? string.Empty;
    }

    public bool Equals(Address? other)
    {
        if (other is null)
        {
            return false;
        }

        return Street == other.Street &&
               District == other.District &&
               Neighborhood == other.Neighborhood &&
               City == other.City &&
               State == other.State &&
               Country == other.Country &&
               PostalCode == other.PostalCode;
    }

    public override bool Equals(object? obj)
    {
        return obj is Address other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Street, District, Neighborhood, City, State, Country, PostalCode);
    }

    public override string ToString()
    {
        var neighborhoodPart = !string.IsNullOrWhiteSpace(Neighborhood) ? $"{Neighborhood}, " : string.Empty;
        var districtPart = !string.IsNullOrWhiteSpace(District) ? $"{District}, " : string.Empty;
        var statePart = !string.IsNullOrWhiteSpace(State) ? $", {State}" : string.Empty;
        return $"{Street}, {neighborhoodPart}{districtPart}{City}{statePart}, {PostalCode}, {Country}";
    }
}
