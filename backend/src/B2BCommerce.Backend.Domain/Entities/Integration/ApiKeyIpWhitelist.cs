using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Exceptions;
using B2BCommerce.Backend.Domain.Helpers;

namespace B2BCommerce.Backend.Domain.Entities.Integration;

/// <summary>
/// Represents an IP address or CIDR range in the whitelist for an API key
/// </summary>
public class ApiKeyIpWhitelist : BaseEntity
{
    public Guid ApiKeyId { get; private set; }
    public string IpAddress { get; private set; }
    public string? Description { get; private set; }

    // Navigation
    public ApiKey ApiKey { get; private set; } = null!;

    private ApiKeyIpWhitelist() // For EF Core
    {
        IpAddress = string.Empty;
    }

    /// <summary>
    /// Creates a new IP whitelist entry
    /// </summary>
    public ApiKeyIpWhitelist(Guid apiKeyId, string ipAddress, string? description = null)
    {
        if (apiKeyId == Guid.Empty)
            throw new DomainException("API Key ID is required");

        if (string.IsNullOrWhiteSpace(ipAddress))
            throw new DomainException("IP address is required");

        if (!IpAddressHelper.IsValidIpOrCidr(ipAddress))
            throw new DomainException($"Invalid IP address or CIDR: {ipAddress}");

        ApiKeyId = apiKeyId;
        IpAddress = ipAddress.Trim();
        Description = description?.Trim();
    }
}
