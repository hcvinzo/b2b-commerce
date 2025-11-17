namespace B2BCommerce.Backend.Domain.ValueObjects;

/// <summary>
/// Value object representing a tax identification number
/// </summary>
public class TaxNumber : IEquatable<TaxNumber>
{
    public string Value { get; private set; }

    public TaxNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Tax number cannot be null or empty", nameof(value));

        if (value.Length < 10 || value.Length > 20)
            throw new ArgumentException("Tax number must be between 10 and 20 characters", nameof(value));

        Value = value;
    }

    public bool Equals(TaxNumber? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is TaxNumber other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(TaxNumber taxNumber) => taxNumber.Value;
}
