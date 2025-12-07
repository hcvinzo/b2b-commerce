using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Integration;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Commands.RotateApiKey;

/// <summary>
/// Handler for RotateApiKeyCommand
/// </summary>
public class RotateApiKeyCommandHandler : ICommandHandler<RotateApiKeyCommand, Result<CreateApiKeyResponseDto>>
{
    private readonly IApiKeyService _apiKeyService;

    public RotateApiKeyCommandHandler(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    public async Task<Result<CreateApiKeyResponseDto>> Handle(RotateApiKeyCommand request, CancellationToken cancellationToken)
    {
        return await _apiKeyService.RotateAsync(request.Id, request.CreatedBy, cancellationToken);
    }
}
