using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.GeoLocations;

namespace B2BCommerce.Backend.Application.Features.GeoLocations.Commands.UpdateGeoLocation;

/// <summary>
/// Command to update a geo location
/// </summary>
public record UpdateGeoLocationCommand(
    Guid Id,
    string Code,
    string Name,
    decimal? Latitude,
    decimal? Longitude,
    string? Metadata,
    string? UserId) : ICommand<Result<GeoLocationDto>>;
