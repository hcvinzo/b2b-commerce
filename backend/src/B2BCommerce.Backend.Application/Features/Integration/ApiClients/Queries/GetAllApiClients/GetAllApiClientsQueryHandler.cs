using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Integration;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiClients.Queries.GetAllApiClients;

/// <summary>
/// Handler for GetAllApiClientsQuery
/// </summary>
public class GetAllApiClientsQueryHandler : IQueryHandler<GetAllApiClientsQuery, Result<PagedResult<ApiClientListDto>>>
{
    private readonly IApiClientService _apiClientService;

    public GetAllApiClientsQueryHandler(IApiClientService apiClientService)
    {
        _apiClientService = apiClientService;
    }

    public async Task<Result<PagedResult<ApiClientListDto>>> Handle(GetAllApiClientsQuery request, CancellationToken cancellationToken)
    {
        return await _apiClientService.GetAllAsync(request.Page, request.PageSize, request.IsActive, cancellationToken);
    }
}
