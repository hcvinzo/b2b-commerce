using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Integration;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiClients.Queries.GetApiClientById;

/// <summary>
/// Handler for GetApiClientByIdQuery
/// </summary>
public class GetApiClientByIdQueryHandler : IQueryHandler<GetApiClientByIdQuery, Result<ApiClientDetailDto>>
{
    private readonly IApiClientService _apiClientService;

    public GetApiClientByIdQueryHandler(IApiClientService apiClientService)
    {
        _apiClientService = apiClientService;
    }

    public async Task<Result<ApiClientDetailDto>> Handle(GetApiClientByIdQuery request, CancellationToken cancellationToken)
    {
        return await _apiClientService.GetByIdAsync(request.Id, cancellationToken);
    }
}
