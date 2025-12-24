using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Collections;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Application.Features.Collections.Queries.GetCollectionById;

/// <summary>
/// Handler for GetCollectionByIdQuery
/// </summary>
public class GetCollectionByIdQueryHandler : IQueryHandler<GetCollectionByIdQuery, Result<CollectionDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCollectionByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CollectionDto>> Handle(
        GetCollectionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var collection = await _unitOfWork.Collections.GetWithDetailsAsync(request.Id, cancellationToken);
        if (collection is null)
        {
            return Result<CollectionDto>.Failure("Collection not found", "COLLECTION_NOT_FOUND");
        }

        var productCount = await _unitOfWork.Collections.GetProductCountAsync(collection.Id, cancellationToken);
        var dto = MapToDto(collection, productCount);

        return Result<CollectionDto>.Success(dto);
    }

    private static CollectionDto MapToDto(Collection collection, int productCount)
    {
        return new CollectionDto
        {
            Id = collection.Id,
            Name = collection.Name,
            Slug = collection.Slug,
            Description = collection.Description,
            ImageUrl = collection.ImageUrl,
            Type = collection.Type,
            DisplayOrder = collection.DisplayOrder,
            IsActive = collection.IsActive,
            IsFeatured = collection.IsFeatured,
            StartDate = collection.StartDate,
            EndDate = collection.EndDate,
            IsCurrentlyActive = collection.IsCurrentlyActive,
            ProductCount = productCount,
            Filter = collection.Filter is not null ? new CollectionFilterDto
            {
                CategoryIds = collection.Filter.CategoryIds,
                BrandIds = collection.Filter.BrandIds,
                ProductTypeIds = collection.Filter.ProductTypeIds,
                MinPrice = collection.Filter.MinPrice,
                MaxPrice = collection.Filter.MaxPrice
            } : null,
            CreatedAt = collection.CreatedAt,
            UpdatedAt = collection.UpdatedAt
        };
    }
}
