using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Categories;

namespace B2BCommerce.Backend.Application.Interfaces.Services;

/// <summary>
/// Category service interface for category operations
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Gets paginated categories with filtering
    /// </summary>
    Task<Result<PagedResult<CategoryListDto>>> GetAllAsync(
        string? search,
        Guid? parentCategoryId,
        bool? isActive,
        int pageNumber,
        int pageSize,
        string sortBy,
        string sortDirection,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a category by ID
    /// </summary>
    Task<Result<CategoryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets category tree (hierarchical structure)
    /// </summary>
    Task<Result<List<CategoryTreeDto>>> GetCategoryTreeAsync(bool activeOnly = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets root categories
    /// </summary>
    Task<Result<List<CategoryListDto>>> GetRootCategoriesAsync(bool activeOnly = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets subcategories of a parent
    /// </summary>
    Task<Result<List<CategoryListDto>>> GetSubCategoriesAsync(Guid parentId, bool activeOnly = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new category
    /// </summary>
    Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto dto, string? createdBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing category
    /// </summary>
    Task<Result<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryDto dto, string? updatedBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a category (soft delete)
    /// </summary>
    Task<Result> DeleteAsync(Guid id, string? deletedBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates a category
    /// </summary>
    Task<Result<CategoryDto>> ActivateAsync(Guid id, string? updatedBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a category
    /// </summary>
    Task<Result<CategoryDto>> DeactivateAsync(Guid id, string? updatedBy = null, CancellationToken cancellationToken = default);
}
