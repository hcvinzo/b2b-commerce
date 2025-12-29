using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.GeoLocations;

namespace B2BCommerce.Backend.Application.Features.GeoLocations.Queries.GetGeoLocationsByParent;

/// <summary>
/// Query to get geo locations by parent ID (children of a location)
/// Pass null to get root locations
/// </summary>
public record GetGeoLocationsByParentQuery(Guid? ParentId) : IQuery<Result<List<GeoLocationListDto>>>;
