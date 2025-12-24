using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Collections;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.Features.Collections.Commands.SetCollectionFilters;

/// <summary>
/// Handler for SetCollectionFiltersCommand.
/// Sets filter criteria for a dynamic collection.
/// </summary>
public class SetCollectionFiltersCommandHandler : ICommandHandler<SetCollectionFiltersCommand, Result<CollectionFilterDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public SetCollectionFiltersCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CollectionFilterDto>> Handle(SetCollectionFiltersCommand request, CancellationToken cancellationToken)
    {
        // Get collection (without filter - we'll manage it separately via repository)
        var collection = await _unitOfWork.Collections.GetByIdAsync(request.CollectionId, cancellationToken);
        if (collection is null)
        {
            return Result<CollectionFilterDto>.Failure("Collection not found", "COLLECTION_NOT_FOUND");
        }

        // Verify this is a dynamic collection
        if (collection.Type != CollectionType.Dynamic)
        {
            return Result<CollectionFilterDto>.Failure("Filters can only be set on dynamic collections", "INVALID_COLLECTION_TYPE");
        }

        // Validate categories exist
        if (request.CategoryIds is not null)
        {
            foreach (var categoryId in request.CategoryIds)
            {
                var exists = await _unitOfWork.Categories.AnyAsync(c => c.Id == categoryId, cancellationToken);
                if (!exists)
                {
                    return Result<CollectionFilterDto>.Failure($"Category {categoryId} not found", "CATEGORY_NOT_FOUND");
                }
            }
        }

        // Validate brands exist
        if (request.BrandIds is not null)
        {
            foreach (var brandId in request.BrandIds)
            {
                var exists = await _unitOfWork.Brands.AnyAsync(b => b.Id == brandId, cancellationToken);
                if (!exists)
                {
                    return Result<CollectionFilterDto>.Failure($"Brand {brandId} not found", "BRAND_NOT_FOUND");
                }
            }
        }

        // Validate product types exist
        if (request.ProductTypeIds is not null)
        {
            foreach (var productTypeId in request.ProductTypeIds)
            {
                var exists = await _unitOfWork.ProductTypes.AnyAsync(pt => pt.Id == productTypeId, cancellationToken);
                if (!exists)
                {
                    return Result<CollectionFilterDto>.Failure($"Product type {productTypeId} not found", "PRODUCT_TYPE_NOT_FOUND");
                }
            }
        }

        // Use repository method to set filter (handles EF Core tracking correctly)
        var filter = await _unitOfWork.Collections.SetFilterAsync(
            request.CollectionId,
            request.CategoryIds,
            request.BrandIds,
            request.ProductTypeIds,
            request.MinPrice,
            request.MaxPrice,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map to DTO
        var dto = new CollectionFilterDto
        {
            CategoryIds = filter.CategoryIds,
            BrandIds = filter.BrandIds,
            ProductTypeIds = filter.ProductTypeIds,
            MinPrice = filter.MinPrice,
            MaxPrice = filter.MaxPrice
        };

        return Result<CollectionFilterDto>.Success(dto);
    }
}
