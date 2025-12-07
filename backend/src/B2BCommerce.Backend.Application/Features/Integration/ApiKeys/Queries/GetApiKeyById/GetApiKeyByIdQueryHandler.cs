using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Integration;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiKeys.Queries.GetApiKeyById;

/// <summary>
/// Handler for GetApiKeyByIdQuery
/// </summary>
public class GetApiKeyByIdQueryHandler : IQueryHandler<GetApiKeyByIdQuery, Result<ApiKeyDetailDto>>
{
    private readonly IApiKeyService _apiKeyService;

    public GetApiKeyByIdQueryHandler(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    public async Task<Result<ApiKeyDetailDto>> Handle(GetApiKeyByIdQuery request, CancellationToken cancellationToken)
    {
        return await _apiKeyService.GetByIdAsync(request.Id, cancellationToken);
    }
}
