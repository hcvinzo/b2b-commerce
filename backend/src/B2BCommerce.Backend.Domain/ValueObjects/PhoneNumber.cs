using System.Text.RegularExpressions;

namespace B2BCommerce.Backend.Domain.ValueObjects;

/// <summary>
/// Value object representing a phone number
/// </summary>
public partial class PhoneNumber : IEquatable<PhoneNumber>
{
    private static readonly Regex PhoneRegex = CreatePhoneRegex();

    public string Value { get; private set; }

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number cannot be null or empty", nameof(value));

        // Remove common formatting characters
        var cleaned = value.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("+", "");

        if (!PhoneRegex.IsMatch(cleaned))
            throw new ArgumentException($"Invalid phone number format: {value}", nameof(value));

        Value = value;
    }

    [GeneratedRegex(@"^\d{10,15}$", RegexOptions.Compiled)]
    private static partial Regex CreatePhoneRegex();

    public bool Equals(PhoneNumber? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is PhoneNumber other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(PhoneNumber phone) => phone.Value;
}
