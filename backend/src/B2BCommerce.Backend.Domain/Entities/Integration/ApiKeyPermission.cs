using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.Exceptions;

namespace B2BCommerce.Backend.Domain.Entities.Integration;

/// <summary>
/// Represents a permission scope assigned to an API key
/// </summary>
public class ApiKeyPermission : BaseEntity
{
    public Guid ApiKeyId { get; private set; }
    public string Scope { get; private set; }

    // Navigation
    public ApiKey ApiKey { get; private set; } = null!;

    private ApiKeyPermission() // For EF Core
    {
        Scope = string.Empty;
    }

    /// <summary>
    /// Creates a new API key permission
    /// </summary>
    public ApiKeyPermission(Guid apiKeyId, string scope)
    {
        if (apiKeyId == Guid.Empty)
            throw new DomainException("API Key ID is required");

        if (string.IsNullOrWhiteSpace(scope))
            throw new DomainException("Scope is required");

        if (!IntegrationPermissionScopes.IsValidScope(scope))
            throw new DomainException($"Invalid permission scope: {scope}");

        ApiKeyId = apiKeyId;
        Scope = scope.Trim().ToLowerInvariant();
    }
}
