using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Collections;

namespace B2BCommerce.Backend.Application.Features.Collections.Queries.GetCollectionProducts;

/// <summary>
/// Query to get products in a collection.
/// For manual collections, returns products from ProductCollections junction.
/// For dynamic collections, returns products matching the filter criteria.
/// </summary>
public record GetCollectionProductsQuery(
    Guid CollectionId,
    int PageNumber = 1,
    int PageSize = 20) : IQuery<Result<PagedResult<ProductInCollectionDto>>>;
