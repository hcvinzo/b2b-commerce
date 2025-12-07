using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Integration;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Commands.UpdatePermissions;

/// <summary>
/// Handler for UpdateApiKeyPermissionsCommand
/// </summary>
public class UpdateApiKeyPermissionsCommandHandler : ICommandHandler<UpdateApiKeyPermissionsCommand, Result>
{
    private readonly IApiKeyService _apiKeyService;

    public UpdateApiKeyPermissionsCommandHandler(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    public async Task<Result> Handle(UpdateApiKeyPermissionsCommand request, CancellationToken cancellationToken)
    {
        var dto = new UpdateApiKeyPermissionsDto { Permissions = request.Permissions };
        return await _apiKeyService.UpdatePermissionsAsync(request.KeyId, dto, cancellationToken);
    }
}
