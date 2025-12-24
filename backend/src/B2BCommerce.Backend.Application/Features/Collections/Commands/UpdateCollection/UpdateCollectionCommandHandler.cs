using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Collections;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Application.Features.Collections.Commands.UpdateCollection;

/// <summary>
/// Handler for UpdateCollectionCommand
/// </summary>
public class UpdateCollectionCommandHandler : ICommandHandler<UpdateCollectionCommand, Result<CollectionDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCollectionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CollectionDto>> Handle(UpdateCollectionCommand request, CancellationToken cancellationToken)
    {
        // Get existing collection
        var collection = await _unitOfWork.Collections.GetWithFilterAsync(request.Id, cancellationToken);
        if (collection is null)
        {
            return Result<CollectionDto>.Failure("Collection not found", "COLLECTION_NOT_FOUND");
        }

        // Update collection using the Update method
        collection.Update(
            request.Name,
            request.Description,
            request.ImageUrl,
            request.DisplayOrder,
            request.IsFeatured,
            request.StartDate,
            request.EndDate);

        // Update active status
        if (request.IsActive)
        {
            collection.Activate();
        }
        else
        {
            collection.Deactivate();
        }

        // Check for duplicate slug (if name changed)
        var slugExists = await _unitOfWork.Collections.SlugExistsAsync(
            collection.Slug,
            excludeId: collection.Id,
            cancellationToken: cancellationToken);

        if (slugExists)
        {
            return Result<CollectionDto>.Failure("A collection with this name already exists", "DUPLICATE_SLUG");
        }

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Get product count for DTO
        var productCount = await _unitOfWork.Collections.GetProductCountAsync(collection.Id, cancellationToken);

        // Map to DTO
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
