using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Integration;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Commands.UpdateApiKey;

/// <summary>
/// Handler for UpdateApiKeyCommand
/// </summary>
public class UpdateApiKeyCommandHandler : ICommandHandler<UpdateApiKeyCommand, Result<ApiKeyDetailDto>>
{
    private readonly IApiKeyService _apiKeyService;

    public UpdateApiKeyCommandHandler(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    public async Task<Result<ApiKeyDetailDto>> Handle(UpdateApiKeyCommand request, CancellationToken cancellationToken)
    {
        var dto = new UpdateApiKeyDto
        {
            Name = request.Name,
            RateLimitPerMinute = request.RateLimitPerMinute,
            ExpiresAt = request.ExpiresAt
        };

        return await _apiKeyService.UpdateAsync(request.Id, dto, request.UpdatedBy, cancellationToken);
    }
}
