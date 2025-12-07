using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Integration;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Queries.GetUsageStats;

/// <summary>
/// Handler for GetApiKeyUsageStatsQuery
/// </summary>
public class GetApiKeyUsageStatsQueryHandler : IQueryHandler<GetApiKeyUsageStatsQuery, Result<ApiKeyUsageStatsDto>>
{
    private readonly IApiKeyService _apiKeyService;

    public GetApiKeyUsageStatsQueryHandler(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    public async Task<Result<ApiKeyUsageStatsDto>> Handle(GetApiKeyUsageStatsQuery request, CancellationToken cancellationToken)
    {
        return await _apiKeyService.GetUsageStatsAsync(request.ApiKeyId, request.FromDate, request.ToDate, cancellationToken);
    }
}
