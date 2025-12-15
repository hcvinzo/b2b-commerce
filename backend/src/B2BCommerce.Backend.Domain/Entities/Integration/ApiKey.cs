using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.Exceptions;
using B2BCommerce.Backend.Domain.Helpers;

namespace B2BCommerce.Backend.Domain.Entities.Integration;

/// <summary>
/// Represents an API key for Integration API authentication
/// </summary>
public class ApiKey : BaseEntity
{
    public Guid ApiClientId { get; private set; }
    public string KeyHash { get; private set; }
    public string KeyPrefix { get; private set; }
    public string Name { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? LastUsedAt { get; private set; }
    public string? LastUsedIp { get; private set; }
    public int RateLimitPerMinute { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedBy { get; private set; }
    public string? RevocationReason { get; private set; }

    // Navigation
    public ApiClient ApiClient { get; private set; } = null!;

    private readonly List<ApiKeyPermission> _permissions = new();
    public IReadOnlyCollection<ApiKeyPermission> Permissions => _permissions.AsReadOnly();

    private readonly List<ApiKeyIpWhitelist> _ipWhitelist = new();
    public IReadOnlyCollection<ApiKeyIpWhitelist> IpWhitelist => _ipWhitelist.AsReadOnly();

    private ApiKey() // For EF Core
    {
        KeyHash = string.Empty;
        KeyPrefix = string.Empty;
        Name = string.Empty;
    }

    /// <summary>
    /// Creates a new API key instance
    /// </summary>
    public static ApiKey Create(
        Guid apiClientId,
        string keyHash,
        string keyPrefix,
        string name,
        int rateLimitPerMinute = 500,
        DateTime? expiresAt = null,
        string? createdBy = null)
    {
        if (apiClientId == Guid.Empty)
            throw new DomainException("API Client ID is required");

        if (string.IsNullOrWhiteSpace(keyHash))
            throw new DomainException("Key hash is required");

        if (string.IsNullOrWhiteSpace(keyPrefix))
            throw new DomainException("Key prefix is required");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Key name is required");

        if (rateLimitPerMinute <= 0)
            throw new DomainException("Rate limit must be positive");

        if (expiresAt.HasValue && expiresAt.Value <= DateTime.UtcNow)
            throw new DomainException("Expiration date must be in the future");

        var apiKey = new ApiKey
        {
            ApiClientId = apiClientId,
            KeyHash = keyHash,
            KeyPrefix = keyPrefix,
            Name = name.Trim(),
            RateLimitPerMinute = rateLimitPerMinute,
            ExpiresAt = expiresAt,
            IsActive = true,
            CreatedBy = createdBy
        };

        return apiKey;
    }

    [Obsolete("Use ApiKey.Create() factory method instead")]
    public ApiKey(
        Guid apiClientId,
        string keyHash,
        string keyPrefix,
        string name,
        int rateLimitPerMinute = 500,
        DateTime? expiresAt = null,
        string? createdBy = null)
    {
        if (apiClientId == Guid.Empty)
            throw new DomainException("API Client ID is required");

        if (string.IsNullOrWhiteSpace(keyHash))
            throw new DomainException("Key hash is required");

        if (string.IsNullOrWhiteSpace(keyPrefix))
            throw new DomainException("Key prefix is required");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Key name is required");

        if (rateLimitPerMinute <= 0)
            throw new DomainException("Rate limit must be positive");

        if (expiresAt.HasValue && expiresAt.Value <= DateTime.UtcNow)
            throw new DomainException("Expiration date must be in the future");

        ApiClientId = apiClientId;
        KeyHash = keyHash;
        KeyPrefix = keyPrefix;
        Name = name.Trim();
        RateLimitPerMinute = rateLimitPerMinute;
        ExpiresAt = expiresAt;
        IsActive = true;
        CreatedBy = createdBy;
    }

    /// <summary>
    /// Checks if the key is valid for use
    /// </summary>
    public bool IsValid()
    {
        if (!IsActive) return false;
        if (RevokedAt.HasValue) return false;
        if (ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow) return false;
        return true;
    }

    /// <summary>
    /// Checks if the key is expired
    /// </summary>
    public bool IsExpired() => ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;

    /// <summary>
    /// Checks if the key is revoked
    /// </summary>
    public bool IsRevoked() => RevokedAt.HasValue;

    /// <summary>
    /// Records usage of the key
    /// </summary>
    public void RecordUsage(string ipAddress)
    {
        LastUsedAt = DateTime.UtcNow;
        LastUsedIp = ipAddress;
    }

    /// <summary>
    /// Updates the key name
    /// </summary>
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Key name is required");

        Name = name.Trim();
    }

    /// <summary>
    /// Updates the rate limit
    /// </summary>
    public void UpdateRateLimit(int rateLimitPerMinute)
    {
        if (rateLimitPerMinute <= 0)
            throw new DomainException("Rate limit must be positive");

        RateLimitPerMinute = rateLimitPerMinute;
    }

    /// <summary>
    /// Updates the expiration date
    /// </summary>
    public void UpdateExpiration(DateTime? expiresAt)
    {
        if (expiresAt.HasValue && expiresAt.Value <= DateTime.UtcNow)
            throw new DomainException("Expiration date must be in the future");

        ExpiresAt = expiresAt;
    }

    /// <summary>
    /// Revokes the key
    /// </summary>
    public void Revoke(string reason, string revokedBy)
    {
        if (RevokedAt.HasValue)
            throw new DomainException("Key is already revoked");

        IsActive = false;
        RevokedAt = DateTime.UtcNow;
        RevokedBy = revokedBy;
        RevocationReason = reason;
    }

    /// <summary>
    /// Adds a permission scope to the key
    /// </summary>
    public void AddPermission(string scope)
    {
        if (_permissions.Any(p => p.Scope.Equals(scope, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException($"Permission '{scope}' already exists");

        _permissions.Add(new ApiKeyPermission(this.Id, scope));
    }

    /// <summary>
    /// Removes a permission scope from the key
    /// </summary>
    public void RemovePermission(string scope)
    {
        var permission = _permissions.FirstOrDefault(p => p.Scope.Equals(scope, StringComparison.OrdinalIgnoreCase));
        if (permission is not null)
        {
            _permissions.Remove(permission);
        }
    }

    /// <summary>
    /// Clears all permissions from the key
    /// </summary>
    public void ClearPermissions()
    {
        _permissions.Clear();
    }

    /// <summary>
    /// Checks if the key has a specific permission
    /// </summary>
    public bool HasPermission(string scope)
    {
        // Check for exact match or wildcard
        return _permissions.Any(p =>
            p.Scope.Equals(scope, StringComparison.OrdinalIgnoreCase) ||
            p.Scope == IntegrationPermissionScopes.All ||
            (scope.Contains(':') && p.Scope.Equals(scope.Split(':')[0] + ":*", StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// Adds an IP address to the whitelist
    /// </summary>
    public void AddIpToWhitelist(string ipAddress, string? description = null)
    {
        if (_ipWhitelist.Any(ip => ip.IpAddress.Equals(ipAddress, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException($"IP address '{ipAddress}' already whitelisted");

        _ipWhitelist.Add(new ApiKeyIpWhitelist(this.Id, ipAddress, description));
    }

    /// <summary>
    /// Removes an IP address from the whitelist
    /// </summary>
    public void RemoveIpFromWhitelist(string ipAddress)
    {
        var ip = _ipWhitelist.FirstOrDefault(x => x.IpAddress.Equals(ipAddress, StringComparison.OrdinalIgnoreCase));
        if (ip is not null)
        {
            _ipWhitelist.Remove(ip);
        }
    }

    /// <summary>
    /// Removes an IP whitelist entry by ID
    /// </summary>
    public void RemoveIpWhitelistById(Guid whitelistId)
    {
        var ip = _ipWhitelist.FirstOrDefault(x => x.Id == whitelistId);
        if (ip is not null)
        {
            _ipWhitelist.Remove(ip);
        }
    }

    /// <summary>
    /// Checks if an IP address is allowed
    /// </summary>
    public bool IsIpAllowed(string ipAddress)
    {
        // If no whitelist configured, allow all
        if (!_ipWhitelist.Any()) return true;

        // Check exact match or CIDR range
        return _ipWhitelist.Any(w =>
            w.IpAddress.Equals(ipAddress, StringComparison.OrdinalIgnoreCase) ||
            IpAddressHelper.IsInRange(ipAddress, w.IpAddress));
    }
}
