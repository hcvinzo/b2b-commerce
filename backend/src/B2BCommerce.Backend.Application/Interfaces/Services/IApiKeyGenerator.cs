namespace B2BCommerce.Backend.Application.Interfaces.Services;

/// <summary>
/// Service interface for API key generation and hashing
/// </summary>
public interface IApiKeyGenerator
{
    /// <summary>
    /// Generates a new API key with prefix
    /// </summary>
    /// <returns>Tuple of (PlainTextKey, KeyHash, KeyPrefix)</returns>
    (string PlainTextKey, string KeyHash, string KeyPrefix) GenerateKey();

    /// <summary>
    /// Hashes a plain text key for storage
    /// </summary>
    string HashKey(string plainTextKey);

    /// <summary>
    /// Verifies a plain text key against a hash
    /// </summary>
    bool VerifyKey(string plainTextKey, string keyHash);

    /// <summary>
    /// Validates the format of an API key
    /// </summary>
    bool ValidateKeyFormat(string plainTextKey);
}
