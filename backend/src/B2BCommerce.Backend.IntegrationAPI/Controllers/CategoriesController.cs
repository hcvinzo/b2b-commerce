using B2BCommerce.Backend.Application.Features.Categories.Commands.ActivateCategory;
using B2BCommerce.Backend.Application.Features.Categories.Commands.CreateCategory;
using B2BCommerce.Backend.Application.Features.Categories.Commands.DeactivateCategory;
using B2BCommerce.Backend.Application.Features.Categories.Commands.DeleteCategory;
using B2BCommerce.Backend.Application.Features.Categories.Commands.UpdateCategory;
using B2BCommerce.Backend.Application.Features.Categories.Commands.UpsertCategory;
using B2BCommerce.Backend.Application.Features.Categories.Queries.GetCategories;
using B2BCommerce.Backend.Application.Features.Categories.Queries.GetCategoryById;
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
/// Categories API endpoints for external integrations
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
        // Resolve ParentCategoryExtId to ParentCategoryId if provided
        Guid? parentCategoryId = filter.ParentCategoryId;
        if (!parentCategoryId.HasValue && !string.IsNullOrEmpty(filter.ParentCategoryExtId))
        {
            var parentCategory = await _unitOfWork.Categories.GetByExternalIdAsync(filter.ParentCategoryExtId);
            if (parentCategory == null)
            {
                return NotFoundResponse($"Parent category with external ID '{filter.ParentCategoryExtId}' not found");
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
    /// Get a category by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "categories:read")]
    [ProducesResponseType(typeof(Models.ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategory(Guid id)
    {
        var query = new GetCategoryByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
            {
                return NotFoundResponse(result.ErrorMessage ?? $"Category with ID {id} not found");
            }
            return BadRequestResponse(result.ErrorMessage ?? "Failed to get category", result.ErrorCode);
        }

        return OkResponse(MapToCategoryDto(result.Data!));
    }

    /// <summary>
    /// Get category by external ID (primary lookup)
    /// </summary>
    [HttpGet("ext/{extId}")]
    [Authorize(Policy = "categories:read")]
    [ProducesResponseType(typeof(Models.ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryByExtId(string extId)
    {
        var category = await _unitOfWork.Categories.GetByExternalIdAsync(extId);

        if (category == null)
        {
            return NotFoundResponse($"Category with external ID '{extId}' not found");
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
    /// Get subcategories of a parent category
    /// </summary>
    [HttpGet("{id:guid}/subcategories")]
    [Authorize(Policy = "categories:read")]
    [ProducesResponseType(typeof(Models.ApiResponse<List<CategoryListDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubCategories(Guid id, [FromQuery] bool activeOnly = true)
    {
        var query = new GetSubCategoriesQuery(id, activeOnly);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
            {
                return NotFoundResponse(result.ErrorMessage ?? $"Category with ID {id} not found");
            }
            return BadRequestResponse(result.ErrorMessage ?? "Failed to get subcategories", result.ErrorCode);
        }

        var dtos = result.Data!.Select(MapToCategoryListDto).ToList();
        return OkResponse(dtos);
    }

    /// <summary>
    /// Get subcategories by parent's external ID
    /// </summary>
    [HttpGet("ext/{extId}/subcategories")]
    [Authorize(Policy = "categories:read")]
    [ProducesResponseType(typeof(Models.ApiResponse<List<CategoryListDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubCategoriesByExtId(string extId, [FromQuery] bool activeOnly = true)
    {
        var parentCategory = await _unitOfWork.Categories.GetByExternalIdAsync(extId);

        if (parentCategory == null)
        {
            return NotFoundResponse($"Category with external ID '{extId}' not found");
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
    /// Upserts category. If category with given external ID or internal ID exists, it is updated; otherwise, a new category is created.
    /// One of ExtId or Id is required. If only Id is provided, ExtId will be set to Id.ToString().
    /// </summary>
    [HttpPost()]
    [Authorize(Policy = "categories:write")]
    [ProducesResponseType(typeof(Models.ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertCategory([FromBody] CategorySyncRequest request)
    {
        // Validate: One of ExtId or Id is required
        if (string.IsNullOrEmpty(request.ExtId) && !request.Id.HasValue)
        {
            return BadRequestResponse("One of ExtId or Id is required", "ID_REQUIRED");
        }

        // If only Id is provided, set ExtId to Id.ToString()
        var externalId = request.ExtId;
        if (string.IsNullOrEmpty(externalId) && request.Id.HasValue)
        {
            externalId = request.Id.Value.ToString();
        }

        var command = new UpsertCategoryCommand
        {
            Id = request.Id,
            ExternalId = externalId,
            ExternalCode = request.ExtCode,
            Name = request.Name,
            Description = request.Description,
            ParentExternalId = request.ParentId,
            ParentExternalCode = request.ParentCode,
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
            externalId,
            GetClientName());

        return OkResponse(MapToCategoryDto(result.Data!));
    }

    /// <summary>
    /// Delete a category (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "categories:write")]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var command = new DeleteCategoryCommand(id, GetClientName());
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
            {
                return NotFoundResponse(result.ErrorMessage ?? $"Category with ID {id} not found");
            }
            if (result.ErrorCode == "HAS_SUBCATEGORIES" || result.ErrorCode == "HAS_PRODUCTS")
            {
                return BadRequestResponse(result.ErrorMessage ?? "Cannot delete category", result.ErrorCode);
            }
            return BadRequestResponse(result.ErrorMessage ?? "Failed to delete category", result.ErrorCode);
        }

        _logger.LogInformation(
            "Category {CategoryId} deleted by API client {ClientName}",
            id,
            GetClientName());

        return OkResponse<object?>(null, "Category deleted successfully");
    }

    /// <summary>
    /// Delete a category by external ID (soft delete)
    /// </summary>
    [HttpDelete("ext/{extId}")]
    [Authorize(Policy = "categories:write")]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteCategoryByExtId(string extId)
    {
        var category = await _unitOfWork.Categories.GetByExternalIdAsync(extId);

        if (category == null)
        {
            return NotFoundResponse($"Category with external ID '{extId}' not found");
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
            "Category with external ID {ExternalId} deleted by API client {ClientName}",
            extId,
            GetClientName());

        return OkResponse<object?>(null, "Category deleted successfully");
    }

    private async Task<CategoryDto> MapCategoryToDto(Domain.Entities.Category category)
    {
        // Get parent name if exists
        string? parentCategoryName = null;
        if (category.ParentCategoryId.HasValue)
        {
            var parent = await _unitOfWork.Categories.GetByIdAsync(category.ParentCategoryId.Value);
            parentCategoryName = parent?.Name;
        }

        return new CategoryDto
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
    }

    #region Private Mapping Methods

    private static CategoryDto MapToCategoryDto(Application.DTOs.Categories.CategoryDto source)
    {
        return new CategoryDto
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            ParentCategoryId = source.ParentCategoryId,
            ParentCategoryName = source.ParentCategoryName,
            ImageUrl = source.ImageUrl,
            DisplayOrder = source.DisplayOrder,
            IsActive = source.IsActive,
            Slug = source.Slug,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt,
            ExternalCode = source.ExternalCode,
            ExternalId = source.ExternalId,
            LastSyncedAt = source.LastSyncedAt
        };
    }

    private static CategoryListDto MapToCategoryListDto(Application.DTOs.Categories.CategoryListDto source)
    {
        return new CategoryListDto
        {
            Id = source.Id,
            Name = source.Name,
            ParentCategoryId = source.ParentCategoryId,
            ParentCategoryName = source.ParentCategoryName,
            DisplayOrder = source.DisplayOrder,
            IsActive = source.IsActive,
            SubCategoryCount = source.SubCategoryCount,
            ProductCount = source.ProductCount,
            ExternalCode = source.ExternalCode,
            ExternalId = source.ExternalId,
            LastSyncedAt = source.LastSyncedAt
        };
    }

    private static CategoryTreeDto MapToCategoryTreeDto(Application.DTOs.Categories.CategoryTreeDto source)
    {
        return new CategoryTreeDto
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            ImageUrl = source.ImageUrl,
            DisplayOrder = source.DisplayOrder,
            IsActive = source.IsActive,
            SubCategories = source.SubCategories.Select(MapToCategoryTreeDto).ToList(),
            ExternalCode = source.ExternalCode,
            ExternalId = source.ExternalId,
            LastSyncedAt = source.LastSyncedAt
        };
    }

    #endregion
}
