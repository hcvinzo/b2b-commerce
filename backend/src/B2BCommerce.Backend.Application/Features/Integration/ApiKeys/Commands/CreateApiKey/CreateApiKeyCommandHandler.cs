using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Integration;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Commands.CreateApiKey;

/// <summary>
/// Handler for CreateApiKeyCommand
/// </summary>
public class CreateApiKeyCommandHandler : ICommandHandler<CreateApiKeyCommand, Result<CreateApiKeyResponseDto>>
{
    private readonly IApiKeyService _apiKeyService;

    public CreateApiKeyCommandHandler(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    public async Task<Result<CreateApiKeyResponseDto>> Handle(CreateApiKeyCommand request, CancellationToken cancellationToken)
    {
        var dto = new CreateApiKeyDto
        {
            ApiClientId = request.ApiClientId,
            Name = request.Name,
            RateLimitPerMinute = request.RateLimitPerMinute,
            ExpiresAt = request.ExpiresAt,
            Permissions = request.Permissions,
            IpWhitelist = request.IpWhitelist
        };

        return await _apiKeyService.CreateAsync(dto, request.CreatedBy, cancellationToken);
    }
}
