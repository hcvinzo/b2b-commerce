using System.Text.RegularExpressions;

namespace B2BCommerce.Backend.Domain.ValueObjects;

/// <summary>
/// Value object representing an email address
/// </summary>
public partial class Email : IEquatable<Email>
{
    private static readonly Regex EmailRegex = CreateEmailRegex();

    public string Value { get; private set; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be null or empty", nameof(value));

        if (!EmailRegex.IsMatch(value))
            throw new ArgumentException($"Invalid email format: {value}", nameof(value));

        Value = value.ToLowerInvariant();
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex CreateEmailRegex();

    public bool Equals(Email? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is Email other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(Email email) => email.Value;
}
