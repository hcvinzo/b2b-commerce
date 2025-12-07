using System.Security.Cryptography;
using System.Text;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Infrastructure.Services.Integration;

/// <summary>
/// Service for generating and hashing API keys
/// </summary>
public class ApiKeyGenerator : IApiKeyGenerator
{
    private const string Prefix = "b2b_";
    private const int KeyLength = 32; // 32 bytes = 256 bits of entropy

    public (string PlainTextKey, string KeyHash, string KeyPrefix) GenerateKey()
    {
        // Generate cryptographically secure random bytes
        var keyBytes = new byte[KeyLength];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(keyBytes);
        }

        // Convert to base64 and make URL-safe
        var keyBase64 = Convert.ToBase64String(keyBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        // Create the full key with prefix
        var plainTextKey = $"{Prefix}{keyBase64}";

        // Generate hash for storage
        var keyHash = HashKey(plainTextKey);

        // Extract prefix for quick lookup (first 8 chars after "b2b_")
        var keyPrefix = keyBase64.Length >= 8 ? keyBase64[..8] : keyBase64;

        return (plainTextKey, keyHash, keyPrefix);
    }

    public string HashKey(string plainTextKey)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(plainTextKey));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public bool VerifyKey(string plainTextKey, string keyHash)
    {
        var computedHash = HashKey(plainTextKey);
        return string.Equals(computedHash, keyHash, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates the format of an API key
    /// </summary>
    public bool ValidateKeyFormat(string plainTextKey)
    {
        if (string.IsNullOrWhiteSpace(plainTextKey))
            return false;

        if (!plainTextKey.StartsWith(Prefix))
            return false;

        // Key should be at least prefix + some characters
        if (plainTextKey.Length < Prefix.Length + 8)
            return false;

        // The rest should be valid base64url characters
        var keyPart = plainTextKey[Prefix.Length..];
        return keyPart.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
    }
}
