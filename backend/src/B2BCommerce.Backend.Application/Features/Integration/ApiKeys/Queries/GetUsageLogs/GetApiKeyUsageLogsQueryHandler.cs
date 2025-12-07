using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Integration;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Queries.GetUsageLogs;

/// <summary>
/// Handler for GetApiKeyUsageLogsQuery
/// </summary>
public class GetApiKeyUsageLogsQueryHandler : IQueryHandler<GetApiKeyUsageLogsQuery, Result<PagedResult<ApiKeyUsageLogDto>>>
{
    private readonly IApiKeyService _apiKeyService;

    public GetApiKeyUsageLogsQueryHandler(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    public async Task<Result<PagedResult<ApiKeyUsageLogDto>>> Handle(GetApiKeyUsageLogsQuery request, CancellationToken cancellationToken)
    {
        var filter = new UsageLogFilterDto
        {
            ApiKeyId = request.ApiKeyId,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            Page = request.Page,
            PageSize = request.PageSize
        };

        return await _apiKeyService.GetUsageLogsAsync(filter, cancellationToken);
    }
}
