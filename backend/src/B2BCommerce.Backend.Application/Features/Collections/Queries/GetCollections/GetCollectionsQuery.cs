using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Collections;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.Features.Collections.Queries.GetCollections;

/// <summary>
/// Query to get all collections with pagination, search, and filtering
/// </summary>
public record GetCollectionsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Search = null,
    CollectionType? Type = null,
    bool? IsActive = null,
    bool? IsFeatured = null,
    string? SortBy = null,
    string? SortDirection = null) : IQuery<Result<PagedResult<CollectionListDto>>>;
