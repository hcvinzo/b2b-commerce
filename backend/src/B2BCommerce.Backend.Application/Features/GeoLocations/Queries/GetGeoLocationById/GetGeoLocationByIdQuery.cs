using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.GeoLocations;

namespace B2BCommerce.Backend.Application.Features.GeoLocations.Queries.GetGeoLocationById;

/// <summary>
/// Query to get a geo location by ID
/// </summary>
public record GetGeoLocationByIdQuery(Guid Id) : IQuery<Result<GeoLocationDto>>;
