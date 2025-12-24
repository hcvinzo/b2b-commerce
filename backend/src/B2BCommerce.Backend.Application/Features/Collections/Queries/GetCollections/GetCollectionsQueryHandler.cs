using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Collections;
using B2BCommerce.Backend.Application.Interfaces.Repositories;

namespace B2BCommerce.Backend.Application.Features.Collections.Queries.GetCollections;

/// <summary>
/// Handler for GetCollectionsQuery
/// </summary>
public class GetCollectionsQueryHandler : IQueryHandler<GetCollectionsQuery, Result<PagedResult<CollectionListDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCollectionsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<CollectionListDto>>> Handle(
        GetCollectionsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _unitOfWork.Collections.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.Search,
            request.Type,
            request.IsActive,
            request.IsFeatured,
            request.SortBy,
            request.SortDirection,
            cancellationToken);

        var dtos = items.Select(c => new CollectionListDto
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug,
            ImageUrl = c.ImageUrl,
            Type = c.Type,
            IsActive = c.IsActive,
            IsFeatured = c.IsFeatured,
            ProductCount = c.ProductCollections.Count,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            IsCurrentlyActive = c.IsCurrentlyActive,
            CreatedAt = c.CreatedAt
        }).ToList();

        var pagedResult = new PagedResult<CollectionListDto>(
            dtos,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return Result<PagedResult<CollectionListDto>>.Success(pagedResult);
    }
}
