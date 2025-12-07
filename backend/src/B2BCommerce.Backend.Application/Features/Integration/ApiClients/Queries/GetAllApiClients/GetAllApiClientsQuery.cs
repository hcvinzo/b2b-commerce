using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Integration;

namespace B2BCommerce.Backend.Application.Features.Integration.ApiClients.Queries.GetAllApiClients;

/// <summary>
/// Query to get all API clients with pagination
/// </summary>
public record GetAllApiClientsQuery(
    int Page = 1,
    int PageSize = 20,
    bool? IsActive = null) : IQuery<Result<PagedResult<ApiClientListDto>>>;
