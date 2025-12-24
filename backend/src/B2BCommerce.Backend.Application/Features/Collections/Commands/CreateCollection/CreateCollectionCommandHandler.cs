using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Collections;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Application.Features.Collections.Commands.CreateCollection;

/// <summary>
/// Handler for CreateCollectionCommand
/// </summary>
public class CreateCollectionCommandHandler : ICommandHandler<CreateCollectionCommand, Result<CollectionDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCollectionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CollectionDto>> Handle(CreateCollectionCommand request, CancellationToken cancellationToken)
    {
        // Create collection using factory method with all parameters
        var collection = Collection.Create(
            request.Name,
            request.Type,
            request.Description,
            request.ImageUrl,
            request.DisplayOrder,
            request.IsActive,
            request.IsFeatured,
            request.StartDate,
            request.EndDate);

        // Check for duplicate slug
        var slugExists = await _unitOfWork.Collections.SlugExistsAsync(collection.Slug, cancellationToken: cancellationToken);
        if (slugExists)
        {
            return Result<CollectionDto>.Failure("A collection with this name already exists", "DUPLICATE_SLUG");
        }

        // Save to database
        await _unitOfWork.Collections.AddAsync(collection, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map to DTO
        var dto = MapToDto(collection);
        return Result<CollectionDto>.Success(dto);
    }

    private static CollectionDto MapToDto(Collection collection)
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
            ProductCount = 0, // New collection has no products
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
