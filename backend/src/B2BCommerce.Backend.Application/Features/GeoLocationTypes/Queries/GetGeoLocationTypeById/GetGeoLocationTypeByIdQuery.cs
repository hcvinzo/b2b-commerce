using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.GeoLocations;

namespace B2BCommerce.Backend.Application.Features.GeoLocationTypes.Queries.GetGeoLocationTypeById;

/// <summary>
/// Query to get a geo location type by ID
/// </summary>
public record GetGeoLocationTypeByIdQuery(Guid Id) : IQuery<Result<GeoLocationTypeDto>>;
