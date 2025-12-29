using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.GeoLocations;

namespace B2BCommerce.Backend.Application.Features.GeoLocationTypes.Queries.GetGeoLocationTypes;

/// <summary>
/// Query to get all geo location types
/// </summary>
public record GetGeoLocationTypesQuery() : IQuery<Result<List<GeoLocationTypeDto>>>;
