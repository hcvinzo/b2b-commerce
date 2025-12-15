using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.DTOs.Categories;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Application.Features.Categories.Commands.UpsertCategory;

/// <summary>
/// Handler for UpsertCategoryCommand.
/// Creates or updates a category based on ExternalId (primary), ExternalCode (fallback), or Id.
/// </summary>
public class UpsertCategoryCommandHandler : ICommandHandler<UpsertCategoryCommand, Result<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpsertCategoryCommandHandler> _logger;

    public UpsertCategoryCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpsertCategoryCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CategoryDto>> Handle(UpsertCategoryCommand request, CancellationToken cancellationToken)
    {
        // 1. Resolve parent category (by ID, ExternalId, or ExternalCode)
        Guid? parentCategoryId = request.ParentCategoryId;

        if (!parentCategoryId.HasValue && !string.IsNullOrEmpty(request.ParentExternalId))
        {
            var parentCategory = await _unitOfWork.Categories
                .GetByExternalIdAsync(request.ParentExternalId, cancellationToken);

            if (parentCategory == null)
            {
                return Result<CategoryDto>.Failure(
                    $"Parent category not found by ExternalId: {request.ParentExternalId}",
                    "PARENT_NOT_FOUND");
            }

            parentCategoryId = parentCategory.Id;
        }
        else if (!parentCategoryId.HasValue && !string.IsNullOrEmpty(request.ParentExternalCode))
        {
            var parentCategory = await _unitOfWork.Categories
                .GetByExternalCodeAsync(request.ParentExternalCode, cancellationToken);

            if (parentCategory == null)
            {
                return Result<CategoryDto>.Failure(
                    $"Parent category not found by ExternalCode: {request.ParentExternalCode}",
                    "PARENT_NOT_FOUND");
            }

            parentCategoryId = parentCategory.Id;
        }

        // 2. Find existing category (by ID, ExternalId, or ExternalCode)
        Category? category = null;
        bool createWithSpecificId = false;

        if (request.Id.HasValue)
        {
            category = await _unitOfWork.Categories.GetByIdAsync(request.Id.Value, cancellationToken);

            // If Id is provided but not found, we'll create with that specific Id
            if (category == null)
            {
                createWithSpecificId = true;
            }
        }

        // If not found by Id, try ExternalId
        if (category == null && !createWithSpecificId && !string.IsNullOrEmpty(request.ExternalId))
        {
            // Primary lookup by ExternalId
            category = await _unitOfWork.Categories
                .GetByExternalIdAsync(request.ExternalId, cancellationToken);
        }

        // If still not found, try ExternalCode as fallback
        if (category == null && !createWithSpecificId && !string.IsNullOrEmpty(request.ExternalCode))
        {
            // Fallback lookup by ExternalCode for backward compatibility
            category = await _unitOfWork.Categories
                .GetByExternalCodeAsync(request.ExternalCode, cancellationToken);
        }

        // 3. Create or update
        if (category == null)
        {
            // Create new category - ExternalId is required for new external entities
            if (string.IsNullOrEmpty(request.ExternalId))
            {
                return Result<CategoryDto>.Failure(
                    "ExternalId is required for creating new categories via sync",
                    "EXTERNAL_ID_REQUIRED");
            }

            category = Category.CreateFromExternal(
                externalId: request.ExternalId,
                name: request.Name,
                description: request.Description ?? string.Empty,
                parentCategoryId: parentCategoryId,
                externalCode: request.ExternalCode,
                imageUrl: request.ImageUrl,
                displayOrder: request.DisplayOrder,
                specificId: createWithSpecificId ? request.Id : null);

            if (!request.IsActive)
            {
                category.Deactivate();
            }

            category.CreatedBy = request.ModifiedBy;

            await _unitOfWork.Categories.AddAsync(category, cancellationToken);

            _logger.LogInformation(
                "Creating category from external sync: {ExternalId} - {Name} (Id: {Id})",
                request.ExternalId, request.Name, category.Id);
        }
        else
        {
            // Update existing category
            category.UpdateFromExternal(
                name: request.Name,
                description: request.Description,
                parentCategoryId: parentCategoryId,
                imageUrl: request.ImageUrl,
                displayOrder: request.DisplayOrder,
                isActive: request.IsActive,
                externalCode: request.ExternalCode);

            category.UpdatedBy = request.ModifiedBy;
            category.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Updating category from external sync: {ExternalId} - {Name}",
                category.ExternalId, request.Name);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 4. Get parent name for response
        string? parentCategoryName = null;
        if (category.ParentCategoryId.HasValue)
        {
            var parent = await _unitOfWork.Categories.GetByIdAsync(category.ParentCategoryId.Value, cancellationToken);
            parentCategoryName = parent?.Name;
        }

        // 5. Map to DTO and return
        var dto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = parentCategoryName,
            ImageUrl = category.ImageUrl,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            Slug = category.Slug,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt,
            ExternalCode = category.ExternalCode,
            ExternalId = category.ExternalId,
            LastSyncedAt = category.LastSyncedAt
        };

        return Result<CategoryDto>.Success(dto);
    }
}
