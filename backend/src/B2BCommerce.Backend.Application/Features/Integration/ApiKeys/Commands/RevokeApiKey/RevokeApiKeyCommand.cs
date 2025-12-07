using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Commands.RevokeApiKey;

/// <summary>
/// Command to revoke an API key
/// </summary>
public record RevokeApiKeyCommand(
    Guid Id,
    string Reason,
    string RevokedBy) : ICommand<Result>;
