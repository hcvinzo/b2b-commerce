using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Commands.RemoveIpWhitelist;

/// <summary>
/// Handler for RemoveIpWhitelistCommand
/// </summary>
public class RemoveIpWhitelistCommandHandler : ICommandHandler<RemoveIpWhitelistCommand, Result>
{
    private readonly IApiKeyService _apiKeyService;

    public RemoveIpWhitelistCommandHandler(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    public async Task<Result> Handle(RemoveIpWhitelistCommand request, CancellationToken cancellationToken)
    {
        return await _apiKeyService.RemoveIpFromWhitelistAsync(request.KeyId, request.WhitelistId, cancellationToken);
    }
}
