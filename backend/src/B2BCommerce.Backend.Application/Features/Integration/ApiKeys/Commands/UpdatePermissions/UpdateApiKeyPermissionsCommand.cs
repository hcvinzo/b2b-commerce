using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Commands.UpdatePermissions;

/// <summary>
/// Command to update API key permissions
/// </summary>
public record UpdateApiKeyPermissionsCommand(
    Guid KeyId,
    List<string> Permissions) : ICommand<Result>;
