using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.GeoLocations;

namespace B2BCommerce.Backend.Application.Features.GeoLocations.Commands.CreateGeoLocation;

/// <summary>
/// Command to create a new geo location
/// </summary>
public record CreateGeoLocationCommand(
    Guid GeoLocationTypeId,
    string Code,
    string Name,
    Guid? ParentId,
    decimal? Latitude,
    decimal? Longitude,
    string? Metadata,
    string? UserId) : ICommand<Result<GeoLocationDto>>;
