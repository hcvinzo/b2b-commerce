using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.IntegrationAPI.DTOs.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace B2BCommerce.Backend.IntegrationAPI.Controllers;

/// <summary>
/// Categories API endpoints for external integrations
/// </summary>
public class CategoriesController : BaseApiController
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ILogger<CategoriesController> logger)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
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
        // Validate and constrain page size
        filter.PageSize = Math.Clamp(filter.PageSize, 1, 100);
        filter.PageNumber = Math.Max(1, filter.PageNumber);

        var query = _categoryRepository.Query();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(c => c.Name.Contains(filter.Search));
        }

        if (filter.ParentCategoryId.HasValue)
        {
            query = query.Where(c => c.ParentCategoryId == filter.ParentCategoryId.Value);
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == filter.IsActive.Value);
        }

        // Apply sorting
        query = filter.SortBy.ToLowerInvariant() switch
        {
            "name" => filter.SortDirection.ToLowerInvariant() == "desc"
                ? query.OrderByDescending(c => c.Name)
                : query.OrderBy(c => c.Name),
            "createdat" => filter.SortDirection.ToLowerInvariant() == "desc"
                ? query.OrderByDescending(c => c.CreatedAt)
                : query.OrderBy(c => c.CreatedAt),
            _ => filter.SortDirection.ToLowerInvariant() == "desc"
                ? query.OrderByDescending(c => c.DisplayOrder)
                : query.OrderBy(c => c.DisplayOrder)
        };

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination
        var categories = await query
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .Include(c => c.Products)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var dtos = categories.Select(c => new CategoryListDto
        {
            Id = c.Id,
            Name = c.Name,
            ParentCategoryId = c.ParentCategoryId,
            ParentCategoryName = c.ParentCategory?.Name,
            DisplayOrder = c.DisplayOrder,
            IsActive = c.IsActive,
            SubCategoryCount = c.SubCategories.Count(sc => !sc.IsDeleted),
            ProductCount = c.Products.Count(p => !p.IsDeleted)
        }).ToList();

        return PagedResponse(dtos, filter.PageNumber, filter.PageSize, totalCount);
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
        var category = await _categoryRepository.Query()
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFoundResponse($"Category with ID {id} not found");
        }

        var dto = MapToDto(category);
        return OkResponse(dto);
    }

    /// <summary>
    /// Get category tree (hierarchical structure)
    /// </summary>
    [HttpGet("tree")]
    [Authorize(Policy = "categories:read")]
    [ProducesResponseType(typeof(Models.ApiResponse<List<CategoryTreeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategoryTree([FromQuery] bool activeOnly = true)
    {
        var query = _categoryRepository.Query()
            .Include(c => c.SubCategories)
            .AsQueryable();

        if (activeOnly)
        {
            query = query.Where(c => c.IsActive);
        }

        var allCategories = await query.ToListAsync();
        var rootCategories = allCategories.Where(c => c.ParentCategoryId == null);

        var tree = rootCategories
            .OrderBy(c => c.DisplayOrder)
            .Select(c => BuildCategoryTree(c, allCategories, activeOnly))
            .ToList();

        return OkResponse(tree);
    }

    /// <summary>
    /// Get root categories (categories without parent)
    /// </summary>
    [HttpGet("root")]
    [Authorize(Policy = "categories:read")]
    [ProducesResponseType(typeof(Models.ApiResponse<List<CategoryListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRootCategories([FromQuery] bool activeOnly = true)
    {
        var categories = await _categoryRepository.GetByParentIdAsync(null);

        if (activeOnly)
        {
            categories = categories.Where(c => c.IsActive);
        }

        var dtos = categories
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new CategoryListDto
            {
                Id = c.Id,
                Name = c.Name,
                ParentCategoryId = null,
                ParentCategoryName = null,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
                SubCategoryCount = c.SubCategories.Count(sc => !sc.IsDeleted),
                ProductCount = c.Products.Count(p => !p.IsDeleted)
            }).ToList();

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
        // Verify parent exists
        var parent = await _categoryRepository.GetByIdAsync(id);
        if (parent == null)
        {
            return NotFoundResponse($"Category with ID {id} not found");
        }

        var categories = await _categoryRepository.GetByParentIdAsync(id);

        if (activeOnly)
        {
            categories = categories.Where(c => c.IsActive);
        }

        var dtos = categories
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new CategoryListDto
            {
                Id = c.Id,
                Name = c.Name,
                ParentCategoryId = c.ParentCategoryId,
                ParentCategoryName = parent.Name,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
                SubCategoryCount = c.SubCategories.Count(sc => !sc.IsDeleted),
                ProductCount = c.Products.Count(p => !p.IsDeleted)
            }).ToList();

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
        // Validate parent category if specified
        if (dto.ParentCategoryId.HasValue)
        {
            var parent = await _categoryRepository.GetByIdAsync(dto.ParentCategoryId.Value);
            if (parent == null)
            {
                return BadRequestResponse($"Parent category with ID {dto.ParentCategoryId} not found", "INVALID_PARENT");
            }
        }

        // Check for duplicate name under same parent
        var existingCategory = await _categoryRepository.Query()
            .FirstOrDefaultAsync(c => c.Name == dto.Name && c.ParentCategoryId == dto.ParentCategoryId);

        if (existingCategory != null)
        {
            return ConflictResponse($"A category with name '{dto.Name}' already exists under the same parent", "DUPLICATE_NAME");
        }

        var category = new Category(
            dto.Name,
            dto.Description ?? string.Empty,
            dto.ParentCategoryId,
            dto.DisplayOrder);

        if (!string.IsNullOrEmpty(dto.ImageUrl))
        {
            category.SetImage(dto.ImageUrl);
        }

        if (!dto.IsActive)
        {
            category.Deactivate();
        }

        category.CreatedBy = GetClientName();

        await _categoryRepository.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Category {CategoryId} created by API client {ClientName}",
            category.Id,
            GetClientName());

        // Reload with parent for response
        var createdCategory = await _categoryRepository.Query()
            .Include(c => c.ParentCategory)
            .FirstAsync(c => c.Id == category.Id);

        return CreatedResponse(MapToDto(createdCategory), $"/api/v1/categories/{category.Id}");
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
        var category = await _categoryRepository.Query()
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFoundResponse($"Category with ID {id} not found");
        }

        // Check for duplicate name under same parent (excluding current category)
        var existingCategory = await _categoryRepository.Query()
            .FirstOrDefaultAsync(c => c.Name == dto.Name
                && c.ParentCategoryId == category.ParentCategoryId
                && c.Id != id);

        if (existingCategory != null)
        {
            return ConflictResponse($"A category with name '{dto.Name}' already exists under the same parent", "DUPLICATE_NAME");
        }

        category.Update(dto.Name, dto.Description ?? string.Empty, dto.DisplayOrder);

        if (!string.IsNullOrEmpty(dto.ImageUrl))
        {
            category.SetImage(dto.ImageUrl);
        }

        if (dto.IsActive)
        {
            category.Activate();
        }
        else
        {
            category.Deactivate();
        }

        category.UpdatedBy = GetClientName();

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Category {CategoryId} updated by API client {ClientName}",
            category.Id,
            GetClientName());

        return OkResponse(MapToDto(category));
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
        var category = await _categoryRepository.Query()
            .Include(c => c.SubCategories)
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFoundResponse($"Category with ID {id} not found");
        }

        // Check for subcategories
        if (category.SubCategories.Any(sc => !sc.IsDeleted))
        {
            return BadRequestResponse(
                "Cannot delete category with subcategories. Delete subcategories first or move them to another parent.",
                "HAS_SUBCATEGORIES");
        }

        // Check for products
        if (category.Products.Any(p => !p.IsDeleted))
        {
            return BadRequestResponse(
                "Cannot delete category with products. Remove or reassign products first.",
                "HAS_PRODUCTS");
        }

        category.DeletedBy = GetClientName();
        await _categoryRepository.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

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
        var category = await _categoryRepository.Query()
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFoundResponse($"Category with ID {id} not found");
        }

        category.Activate();
        category.UpdatedBy = GetClientName();
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Category {CategoryId} activated by API client {ClientName}",
            category.Id,
            GetClientName());

        return OkResponse(MapToDto(category));
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
        var category = await _categoryRepository.Query()
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFoundResponse($"Category with ID {id} not found");
        }

        category.Deactivate();
        category.UpdatedBy = GetClientName();
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Category {CategoryId} deactivated by API client {ClientName}",
            category.Id,
            GetClientName());

        return OkResponse(MapToDto(category));
    }

    #region Private Helper Methods

    private static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = category.ParentCategory?.Name,
            ImageUrl = category.ImageUrl,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }

    private static CategoryTreeDto BuildCategoryTree(Category category, IEnumerable<Category> allCategories, bool activeOnly)
    {
        var subCategories = allCategories
            .Where(c => c.ParentCategoryId == category.Id)
            .Where(c => !activeOnly || c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => BuildCategoryTree(c, allCategories, activeOnly))
            .ToList();

        return new CategoryTreeDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            SubCategories = subCategories
        };
    }

    #endregion
}
