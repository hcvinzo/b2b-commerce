using B2BCommerce.Backend.Application.Features.Categories.Commands.ActivateCategory;
using B2BCommerce.Backend.Application.Features.Categories.Commands.CreateCategory;
using B2BCommerce.Backend.Application.Features.Categories.Commands.DeactivateCategory;
using B2BCommerce.Backend.Application.Features.Categories.Commands.DeleteCategory;
using B2BCommerce.Backend.Application.Features.Categories.Commands.UpdateCategory;
using B2BCommerce.Backend.Application.Features.Categories.Queries.GetCategories;
using B2BCommerce.Backend.Application.Features.Categories.Queries.GetCategoryById;
using B2BCommerce.Backend.Application.Features.Categories.Queries.GetCategoryTree;
using B2BCommerce.Backend.Application.Features.Categories.Queries.GetRootCategories;
using B2BCommerce.Backend.Application.Features.Categories.Queries.GetSubCategories;
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

    public CategoriesController(
        IMediator mediator,
        ILogger<CategoriesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all categories with filtering and pagination
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "categories:read")]
    [ProducesResponseType(typeof(Models.PagedApiResponse<CategoryListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories([FromQuery] CategoryFilterDto filter)
    {
        var query = new GetCategoriesQuery(
            filter.Search,
            filter.ParentCategoryId,
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
    /// Create a new category
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "categories:write")]
    [ProducesResponseType(typeof(Models.ApiResponse<CategoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        var command = new CreateCategoryCommand(
            dto.Name,
            dto.Description,
            dto.ParentCategoryId,
            dto.ImageUrl,
            dto.DisplayOrder,
            dto.IsActive,
            GetClientName());

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "INVALID_PARENT")
            {
                return BadRequestResponse(result.ErrorMessage ?? "Invalid parent category", result.ErrorCode);
            }
            if (result.ErrorCode == "DUPLICATE_NAME")
            {
                return ConflictResponse(result.ErrorMessage ?? "Duplicate category name", result.ErrorCode);
            }
            return BadRequestResponse(result.ErrorMessage ?? "Failed to create category", result.ErrorCode);
        }

        _logger.LogInformation(
            "Category {CategoryId} created by API client {ClientName}",
            result.Data!.Id,
            GetClientName());

        return CreatedResponse(MapToCategoryDto(result.Data!), $"/api/v1/categories/{result.Data!.Id}");
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "categories:write")]
    [ProducesResponseType(typeof(Models.ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto dto)
    {
        var command = new UpdateCategoryCommand(
            id,
            dto.Name,
            dto.Description,
            dto.ImageUrl,
            dto.DisplayOrder,
            dto.IsActive,
            GetClientName());

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
            {
                return NotFoundResponse(result.ErrorMessage ?? $"Category with ID {id} not found");
            }
            if (result.ErrorCode == "DUPLICATE_NAME")
            {
                return ConflictResponse(result.ErrorMessage ?? "Duplicate category name", result.ErrorCode);
            }
            return BadRequestResponse(result.ErrorMessage ?? "Failed to update category", result.ErrorCode);
        }

        _logger.LogInformation(
            "Category {CategoryId} updated by API client {ClientName}",
            id,
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
    /// Activate a category
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = "categories:write")]
    [ProducesResponseType(typeof(Models.ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateCategory(Guid id)
    {
        var command = new ActivateCategoryCommand(id, GetClientName());
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
            {
                return NotFoundResponse(result.ErrorMessage ?? $"Category with ID {id} not found");
            }
            return BadRequestResponse(result.ErrorMessage ?? "Failed to activate category", result.ErrorCode);
        }

        _logger.LogInformation(
            "Category {CategoryId} activated by API client {ClientName}",
            id,
            GetClientName());

        return OkResponse(MapToCategoryDto(result.Data!));
    }

    /// <summary>
    /// Deactivate a category
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = "categories:write")]
    [ProducesResponseType(typeof(Models.ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Models.ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateCategory(Guid id)
    {
        var command = new DeactivateCategoryCommand(id, GetClientName());
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
            {
                return NotFoundResponse(result.ErrorMessage ?? $"Category with ID {id} not found");
            }
            return BadRequestResponse(result.ErrorMessage ?? "Failed to deactivate category", result.ErrorCode);
        }

        _logger.LogInformation(
            "Category {CategoryId} deactivated by API client {ClientName}",
            id,
            GetClientName());

        return OkResponse(MapToCategoryDto(result.Data!));
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
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
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
            ProductCount = source.ProductCount
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
            SubCategories = source.SubCategories.Select(MapToCategoryTreeDto).ToList()
        };
    }

    #endregion
}
