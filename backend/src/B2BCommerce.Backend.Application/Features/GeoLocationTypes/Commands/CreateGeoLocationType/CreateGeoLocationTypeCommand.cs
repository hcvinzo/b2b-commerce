using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.GeoLocations;

namespace B2BCommerce.Backend.Application.Features.GeoLocationTypes.Commands.CreateGeoLocationType;

/// <summary>
/// Command to create a new geo location type
/// </summary>
public record CreateGeoLocationTypeCommand(
    string Name,
    int DisplayOrder,
    string? UserId) : ICommand<Result<GeoLocationTypeDto>>;
