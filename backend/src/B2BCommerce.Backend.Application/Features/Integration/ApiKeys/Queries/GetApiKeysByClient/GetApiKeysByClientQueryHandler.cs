using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Integration;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Queries.GetApiKeysByClient;

/// <summary>
/// Handler for GetApiKeysByClientQuery
/// </summary>
public class GetApiKeysByClientQueryHandler : IQueryHandler<GetApiKeysByClientQuery, Result<List<ApiKeyListDto>>>
{
    private readonly IApiKeyService _apiKeyService;

    public GetApiKeysByClientQueryHandler(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    public async Task<Result<List<ApiKeyListDto>>> Handle(GetApiKeysByClientQuery request, CancellationToken cancellationToken)
    {
        return await _apiKeyService.GetByClientIdAsync(request.ClientId, cancellationToken);
    }
}
