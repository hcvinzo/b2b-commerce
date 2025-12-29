using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;

namespace B2BCommerce.Backend.Application.Features.GeoLocations.Commands.DeleteGeoLocation;

/// <summary>
/// Command to delete a geo location
/// </summary>
public record DeleteGeoLocationCommand(Guid Id, string? UserId) : ICommand<Result>;
