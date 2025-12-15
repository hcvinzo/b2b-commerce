using B2BCommerce.Backend.Application.Features.Categories.Commands.DeleteCategory;
using B2BCommerce.Backend.Application.Features.Categories.Commands.UpsertCategory;
using B2BCommerce.Backend.Application.Features.Categories.Queries.GetCategories;
using B2BCommerce.Backend.Application.Features.Categories.Queries.GetCategoryTree;
using B2BCommerce.Backend.Application.Features.Categories.Queries.GetRootCategories;
using B2BCommerce.Backend.Application.Features.Categories.Queries.GetSubCategories;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.IntegrationAPI.DTOs.Categories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2BCommerce.Backend.IntegrationAPI.Controllers;

/// <summary>
/// Categories API endpoints for external integrations.
/// All operations use ExternalId as the primary identifier.
/// </summary>
public class CategoriesController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<CategoriesController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CategoriesController(
        IMediator mediator,
        ILogger<CategoriesController> logger,
        IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get all categories with filtering and pagination
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "categories:read")]
    [ProducesResponseType(typeof(Models.PagedApiResponse<CategoryListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategories([FromQuery] CategoryFilterDto filter)
    {
        // Resolve ParentId (ExternalId) to internal Guid if provided
        Guid? parentCategoryId = null;
        if (!string.IsNullOrEmpty(filter.ParentId))
        {
            var parentCategory = await _unitOfWork.Categories.GetByExternalIdAsync(filter.ParentId);
            if (parentCategory == null)
            {
                return NotFoundResponse($"Parent category with ID '{filter.ParentId}' not found");
            }
            parentCategoryId = parentCategory.Id;
        }

        var query = new GetCategoriesQuery(
            filter.Search,
            parentCategoryId,
            filter.IsActive,
            filter.PageNumber,
            filter.PageSize,
            filter.SortBy,
            filter.SortDirection);

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequestResponse(result.ErrorMessage ?? "Failed to get categories", result.ErrorCode);
        }

        var pagedResult = result.Data!;
        var dtos = pagedResult.Items.Select(MapToCategoryListDto).ToList();

        return PagedResponse(dtos, pagedResult.PageNumber, pagedResult.PageSize, pagedResult.TotalCount);
    }

    /// <summary>
    /// Get a category by ID (ExternalId)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Policy = "categories:read")]
    [ProducesResponseType(typeof(Models.ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategory(string id)
    {
        var category = await _unitOfWork.Categories.GetByExternalIdAsync(id);

        if (category == null)
        {
            return NotFoundResponse($"Category with ID '{id}' not found");
        }

        return OkResponse(await MapCategoryToDto(category));
    }

    /// <summary>
    /// Get category tree (hierarchical structure)
    /// </summary>
    [HttpGet("tree")]
    [Authorize(Policy = "categories:read")]
    [ProducesResponseType(typeof(Models.ApiResponse<List<CategoryTreeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategoryTree([FromQuery] bool activeOnly = true)
    {
        var query = new GetCategoryTreeQuery(activeOnly);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequestResponse(result.ErrorMessage ?? "Failed to get category tree", result.ErrorCode);
        }

        var dtos = result.Data!.Select(MapToCategoryTreeDto).ToList();
        return OkResponse(dtos);
    }

    /// <summary>
    /// Get root categories (categories without parent)
    /// </summary>
    [HttpGet("root")]
    [Authorize(Policy = "categories:read")]
    [ProducesResponseType(typeof(Models.ApiResponse<List<CategoryListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRootCategories([FromQuery] bool activeOnly = true)
    {
        var query = new GetRootCategoriesQuery(activeOnly);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequestResponse(result.ErrorMessage ?? "Failed to get root categories", result.ErrorCode);
        }

        var dtos = result.Data!.Select(MapToCategoryListDto).ToList();
        return OkResponse(dtos);
    }

    /// <summary>
    /// Get subcategories of a parent category by ID (ExternalId)
    /// </summary>
    [HttpGet("{id}/subcategories")]
    [Authorize(Policy = "categories:read")]
    [ProducesResponseType(typeof(Models.ApiResponse<List<CategoryListDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubCategories(string id, [FromQuery] bool activeOnly = true)
    {
        var parentCategory = await _unitOfWork.Categories.GetByExternalIdAsync(id);

        if (parentCategory == null)
        {
            return NotFoundResponse($"Category with ID '{id}' not found");
        }

        var query = new GetSubCategoriesQuery(parentCategory.Id, activeOnly);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequestResponse(result.ErrorMessage ?? "Failed to get subcategories", result.ErrorCode);
        }

        var dtos = result.Data!.Select(MapToCategoryListDto).ToList();
        return OkResponse(dtos);
    }

    /// <summary>
    /// Upserts category. If category with given ID (ExternalId) exists, it is updated; otherwise, a new category is created.
    /// Id is required for creating new categories.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "categories:write")]
    [ProducesResponseType(typeof(Models.ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertCategory([FromBody] CategorySyncRequest request)
    {
        // Validate: Id (ExternalId) is required
        if (string.IsNullOrEmpty(request.Id))
        {
            return BadRequestResponse("Id is required", "ID_REQUIRED");
        }

        var command = new UpsertCategoryCommand
        {
            ExternalId = request.Id,
            ExternalCode = request.Code,
            Name = request.Name,
            Description = request.Description,
            ParentExternalId = request.ParentId,
            ImageUrl = request.ImageUrl,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            ModifiedBy = GetClientName()
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "PARENT_NOT_FOUND")
            {
                return BadRequestResponse(result.ErrorMessage ?? "Parent category not found", result.ErrorCode);
            }
            if (result.ErrorCode == "NOT_FOUND")
            {
                return NotFoundResponse(result.ErrorMessage ?? "Category not found");
            }
            return BadRequestResponse(result.ErrorMessage ?? "Failed to sync category", result.ErrorCode);
        }

        _logger.LogInformation(
            "Category {ExternalId} synced by API client {ClientName}",
            request.Id,
            GetClientName());

        return OkResponse(MapToCategoryDto(result.Data!));
    }

    /// <summary>
    /// Delete a category by ID (ExternalId) - soft delete
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "categories:write")]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteCategory(string id)
    {
        var category = await _unitOfWork.Categories.GetByExternalIdAsync(id);

        if (category == null)
        {
            return NotFoundResponse($"Category with ID '{id}' not found");
        }

        var command = new DeleteCategoryCommand(category.Id, GetClientName());
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "HAS_SUBCATEGORIES" || result.ErrorCode == "HAS_PRODUCTS")
            {
                return BadRequestResponse(result.ErrorMessage ?? "Cannot delete category", result.ErrorCode);
            }
            return BadRequestResponse(result.ErrorMessage ?? "Failed to delete category", result.ErrorCode);
        }

        _logger.LogInformation(
            "Category {ExternalId} deleted by API client {ClientName}",
            id,
            GetClientName());

        return OkResponse<object?>(null, "Category deleted successfully");
    }

    private async Task<CategoryDto> MapCategoryToDto(Domain.Entities.Category category)
    {
        // Get parent ExternalId if exists
        string? parentId = null;
        string? parentName = null;
        if (category.ParentCategoryId.HasValue)
        {
            var parent = await _unitOfWork.Categories.GetByIdAsync(category.ParentCategoryId.Value);
            parentId = parent?.ExternalId;
            parentName = parent?.Name;
        }

        return new CategoryDto
        {
            Id = category.ExternalId ?? string.Empty,
            Code = category.ExternalCode,
            Name = category.Name,
            Description = category.Description,
            ParentId = parentId,
            ParentName = parentName,
            ImageUrl = category.ImageUrl,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            Slug = category.Slug,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt,
            LastSyncedAt = category.LastSyncedAt
        };
    }

    #region Private Mapping Methods

    private static CategoryDto MapToCategoryDto(Application.DTOs.Categories.CategoryDto source)
    {
        return new CategoryDto
        {
            Id = source.ExternalId ?? string.Empty,
            Code = source.ExternalCode,
            Name = source.Name,
            Description = source.Description,
            ParentId = source.ParentExternalId,
            ParentName = source.ParentCategoryName,
            ImageUrl = source.ImageUrl,
            DisplayOrder = source.DisplayOrder,
            IsActive = source.IsActive,
            Slug = source.Slug,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt,
            LastSyncedAt = source.LastSyncedAt
        };
    }

    private static CategoryListDto MapToCategoryListDto(Application.DTOs.Categories.CategoryListDto source)
    {
        return new CategoryListDto
        {
            Id = source.ExternalId ?? string.Empty,
            Code = source.ExternalCode,
            Name = source.Name,
            ParentId = source.ParentExternalId,
            ParentName = source.ParentCategoryName,
            DisplayOrder = source.DisplayOrder,
            IsActive = source.IsActive,
            SubCategoryCount = source.SubCategoryCount,
            ProductCount = source.ProductCount,
            LastSyncedAt = source.LastSyncedAt
        };
    }

    private static CategoryTreeDto MapToCategoryTreeDto(Application.DTOs.Categories.CategoryTreeDto source)
    {
        return new CategoryTreeDto
        {
            Id = source.ExternalId ?? string.Empty,
            Code = source.ExternalCode,
            Name = source.Name,
            Description = source.Description,
            ImageUrl = source.ImageUrl,
            DisplayOrder = source.DisplayOrder,
            IsActive = source.IsActive,
            SubCategories = source.SubCategories.Select(MapToCategoryTreeDto).ToList(),
            LastSyncedAt = source.LastSyncedAt
        };
    }

    #endregion
}
