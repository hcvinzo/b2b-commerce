using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Exceptions;

namespace B2BCommerce.Backend.Domain.Entities.Integration;

/// <summary>
/// Represents an integration client that can access the Integration API
/// </summary>
public class ApiClient : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }
    public bool IsActive { get; private set; }

    /// <summary>
    /// The user ID associated with this API client.
    /// Used for audit trail tracking (CreatedBy, UpdatedBy).
    /// </summary>
    public string? UserId { get; private set; }

    // Navigation
    private readonly List<ApiKey> _apiKeys = new();
    public IReadOnlyCollection<ApiKey> ApiKeys => _apiKeys.AsReadOnly();

    private ApiClient() // For EF Core
    {
        Name = string.Empty;
        ContactEmail = string.Empty;
    }

    /// <summary>
    /// Creates a new API client instance
    /// </summary>
    public static ApiClient Create(
        string name,
        string contactEmail,
        string? description = null,
        string? contactPhone = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Client name is required");

        if (string.IsNullOrWhiteSpace(contactEmail))
            throw new DomainException("Contact email is required");

        var client = new ApiClient
        {
            Name = name.Trim(),
            ContactEmail = contactEmail.Trim().ToLowerInvariant(),
            Description = description?.Trim(),
            ContactPhone = contactPhone?.Trim(),
            IsActive = true
        };

        return client;
    }

    [Obsolete("Use ApiClient.Create() factory method instead")]
    public ApiClient(
        string name,
        string contactEmail,
        string? description = null,
        string? contactPhone = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Client name is required");

        if (string.IsNullOrWhiteSpace(contactEmail))
            throw new DomainException("Contact email is required");

        Name = name.Trim();
        ContactEmail = contactEmail.Trim().ToLowerInvariant();
        Description = description?.Trim();
        ContactPhone = contactPhone?.Trim();
        IsActive = true;
    }

    /// <summary>
    /// Updates the client information
    /// </summary>
    public void Update(string name, string contactEmail, string? description, string? contactPhone)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Client name is required");

        if (string.IsNullOrWhiteSpace(contactEmail))
            throw new DomainException("Contact email is required");

        Name = name.Trim();
        ContactEmail = contactEmail.Trim().ToLowerInvariant();
        Description = description?.Trim();
        ContactPhone = contactPhone?.Trim();
    }

    /// <summary>
    /// Activates the client
    /// </summary>
    public void Activate() => IsActive = true;

    /// <summary>
    /// Deactivates the client and revokes all associated keys
    /// </summary>
    public void Deactivate(string deactivatedBy)
    {
        IsActive = false;
        // Revoke all associated keys
        foreach (var key in _apiKeys.Where(k => k.IsActive && !k.IsRevoked()))
        {
            key.Revoke("Parent client deactivated", deactivatedBy);
        }
    }

    /// <summary>
    /// Sets the associated user for this API client.
    /// Used for audit trail tracking.
    /// </summary>
    public void SetUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new DomainException("User ID is required");
        }

        UserId = userId;
    }

    /// <summary>
    /// Creates a new API key for this client
    /// </summary>
    public ApiKey CreateApiKey(
        string keyHash,
        string keyPrefix,
        string name,
        int rateLimitPerMinute = 500,
        DateTime? expiresAt = null,
        string? createdBy = null)
    {
        if (!IsActive)
            throw new DomainException("Cannot create API key for inactive client");

        var apiKey = new ApiKey(
            this.Id,
            keyHash,
            keyPrefix,
            name,
            rateLimitPerMinute,
            expiresAt,
            createdBy);

        _apiKeys.Add(apiKey);
        return apiKey;
    }
}
