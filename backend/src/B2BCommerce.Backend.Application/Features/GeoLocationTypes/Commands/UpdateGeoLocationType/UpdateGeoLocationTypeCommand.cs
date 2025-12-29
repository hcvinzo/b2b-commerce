using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.GeoLocations;

namespace B2BCommerce.Backend.Application.Features.GeoLocationTypes.Commands.UpdateGeoLocationType;

/// <summary>
/// Command to update a geo location type
/// </summary>
public record UpdateGeoLocationTypeCommand(
    Guid Id,
    string Name,
    int DisplayOrder,
    string? UserId) : ICommand<Result<GeoLocationTypeDto>>;
