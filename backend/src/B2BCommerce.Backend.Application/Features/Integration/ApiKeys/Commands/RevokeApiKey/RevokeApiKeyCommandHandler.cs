using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Integration;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Commands.RevokeApiKey;

/// <summary>
/// Handler for RevokeApiKeyCommand
/// </summary>
public class RevokeApiKeyCommandHandler : ICommandHandler<RevokeApiKeyCommand, Result>
{
    private readonly IApiKeyService _apiKeyService;

    public RevokeApiKeyCommandHandler(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    public async Task<Result> Handle(RevokeApiKeyCommand request, CancellationToken cancellationToken)
    {
        var dto = new RevokeApiKeyDto { Reason = request.Reason };
        return await _apiKeyService.RevokeAsync(request.Id, dto, request.RevokedBy, cancellationToken);
    }
}
