using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.GeoLocations;

namespace B2BCommerce.Backend.Application.Features.GeoLocations.Queries.GetGeoLocationTree;

/// <summary>
/// Query to get geo location tree (hierarchical structure)
/// </summary>
public record GetGeoLocationTreeQuery(Guid? TypeId = null) : IQuery<Result<List<GeoLocationTreeDto>>>;
