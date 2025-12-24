using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Collections;

namespace B2BCommerce.Backend.Application.Features.Collections.Commands.SetCollectionFilters;

/// <summary>
/// Command to set filter criteria for a dynamic collection
/// </summary>
public record SetCollectionFiltersCommand(
    Guid CollectionId,
    List<Guid>? CategoryIds,
    List<Guid>? BrandIds,
    List<Guid>? ProductTypeIds,
    decimal? MinPrice,
    decimal? MaxPrice) : ICommand<Result<CollectionFilterDto>>;
