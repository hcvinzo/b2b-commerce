using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.GeoLocations;

namespace B2BCommerce.Backend.Application.Features.GeoLocations.Queries.GetGeoLocationsByType;

/// <summary>
/// Query to get geo locations by type ID
/// </summary>
public record GetGeoLocationsByTypeQuery(Guid TypeId) : IQuery<Result<List<GeoLocationListDto>>>;
