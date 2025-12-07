using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Integration;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Commands.CreateApiKey;

/// <summary>
/// Command to create a new API key
/// </summary>
public record CreateApiKeyCommand(
    Guid ApiClientId,
    string Name,
    int RateLimitPerMinute,
    DateTime? ExpiresAt,
    List<string> Permissions,
    List<string> IpWhitelist,
    string CreatedBy) : ICommand<Result<CreateApiKeyResponseDto>>;
