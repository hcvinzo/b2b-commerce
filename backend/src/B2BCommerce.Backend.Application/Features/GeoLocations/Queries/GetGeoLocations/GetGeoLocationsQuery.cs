using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.GeoLocations;

namespace B2BCommerce.Backend.Application.Features.GeoLocations.Queries.GetGeoLocations;

/// <summary>
/// Query to get geo locations with pagination and filtering
/// </summary>
public record GetGeoLocationsQuery(
    string? Search,
    Guid? TypeId,
    Guid? ParentId,
    int PageNumber = 1,
    int PageSize = 20,
    string SortBy = "Name",
    string SortDirection = "asc") : IQuery<Result<PagedResult<GeoLocationListDto>>>;
