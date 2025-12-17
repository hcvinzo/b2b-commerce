using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Common.CQRS;
using B2BCommerce.Backend.Application.Common.Helpers;
using B2BCommerce.Backend.Application.DTOs.Categories;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Application.Features.Categories.Commands.UpsertCategory;

/// <summary>
/// Handler for UpsertCategoryCommand.
/// Creates or updates a category based on ExternalId (primary) or ExternalCode (fallback).
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

            if (parentCategory is null)
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

            if (parentCategory is null)
            {
                return Result<CategoryDto>.Failure(
                    $"Parent category not found by ExternalCode: {request.ParentExternalCode}",
                    "PARENT_NOT_FOUND");
            }

            parentCategoryId = parentCategory.Id;
        }

        // 2. Use shared lookup helper: ExternalId → ExternalCode
        var lookup = await ExternalEntityLookupExtensions.LookupExternalEntityAsync(
            request.ExternalId,
            request.ExternalCode,  // fallback key
            (extId, ct) => _unitOfWork.Categories.GetByExternalIdAsync(extId, ct),
            (extCode, ct) => _unitOfWork.Categories.GetByExternalCodeAsync(extCode, ct),
            cancellationToken);

        Category category;

        // 3. Create or update
        if (lookup.IsNew)
        {
            // If ExternalId provided, use CreateFromExternal; otherwise use Create (auto-generates ExternalId)
            if (!string.IsNullOrEmpty(lookup.EffectiveExternalId))
            {
                category = Category.CreateFromExternal(
                    externalId: lookup.EffectiveExternalId,
                    name: request.Name,
                    description: request.Description ?? string.Empty,
                    parentCategoryId: parentCategoryId,
                    externalCode: request.ExternalCode,
                    imageUrl: request.ImageUrl,
                    displayOrder: request.DisplayOrder);
            }
            else
            {
                // No ExternalId provided - create with auto-generated Id and ExternalId
                category = Category.Create(
                    name: request.Name,
                    description: request.Description ?? string.Empty,
                    parentCategoryId: parentCategoryId,
                    displayOrder: request.DisplayOrder);
                if (!string.IsNullOrEmpty(request.ImageUrl))
                {
                    category.SetImage(request.ImageUrl);
                }
            }

            if (!request.IsActive)
            {
                category.Deactivate();
            }

            await _unitOfWork.Categories.AddAsync(category, cancellationToken);

            _logger.LogInformation(
                "Creating category: {ExternalId} - {Name} (Id: {Id})",
                category.ExternalId, request.Name, category.Id);
        }
        else
        {
            category = lookup.Entity!;

            // Update existing category
            category.UpdateFromExternal(
                name: request.Name,
                description: request.Description,
                parentCategoryId: parentCategoryId,
                imageUrl: request.ImageUrl,
                displayOrder: request.DisplayOrder,
                isActive: request.IsActive,
                externalCode: request.ExternalCode);

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
