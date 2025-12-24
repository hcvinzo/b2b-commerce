using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Collections;

namespace B2BCommerce.Backend.Application.Features.Collections.Queries.GetCollectionById;

/// <summary>
/// Query to get a collection by its ID
/// </summary>
public record GetCollectionByIdQuery(Guid Id) : IQuery<Result<CollectionDto>>;
