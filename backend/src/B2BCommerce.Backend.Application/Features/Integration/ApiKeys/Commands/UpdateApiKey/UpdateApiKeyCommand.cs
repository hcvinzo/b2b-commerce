using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Integration;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Commands.UpdateApiKey;

/// <summary>
/// Command to update an existing API key
/// </summary>
public record UpdateApiKeyCommand(
    Guid Id,
    string Name,
    int RateLimitPerMinute,
    DateTime? ExpiresAt,
    string UpdatedBy) : ICommand<Result<ApiKeyDetailDto>>;
