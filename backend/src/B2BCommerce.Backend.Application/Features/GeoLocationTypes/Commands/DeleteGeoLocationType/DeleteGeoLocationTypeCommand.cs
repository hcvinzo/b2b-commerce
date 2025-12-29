using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;

namespace B2BCommerce.Backend.Application.Features.GeoLocationTypes.Commands.DeleteGeoLocationType;

/// <summary>
/// Command to delete a geo location type
/// </summary>
public record DeleteGeoLocationTypeCommand(Guid Id, string? UserId) : ICommand<Result>;
