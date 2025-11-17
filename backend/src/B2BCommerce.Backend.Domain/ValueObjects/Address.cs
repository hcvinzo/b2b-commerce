namespace B2BCommerce.Backend.Domain.ValueObjects;

/// <summary>
/// Value object representing a physical address
/// </summary>
public class Address : IEquatable<Address>
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string Country { get; private set; }
    public string PostalCode { get; private set; }

    public Address(string street, string city, string state, string country, string postalCode)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be null or empty", nameof(street));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be null or empty", nameof(city));

        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be null or empty", nameof(country));

        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code cannot be null or empty", nameof(postalCode));

        Street = street;
        City = city;
        State = state ?? string.Empty;
        Country = country;
        PostalCode = postalCode;
    }

    public bool Equals(Address? other)
    {
        if (other is null) return false;

        return Street == other.Street &&
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
        return HashCode.Combine(Street, City, State, Country, PostalCode);
    }

    public override string ToString()
    {
        var statePart = !string.IsNullOrWhiteSpace(State) ? $", {State}" : string.Empty;
        return $"{Street}, {City}{statePart}, {PostalCode}, {Country}";
    }
}
