using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Categories;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Application.Interfaces.Services;
using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Infrastructure.Services;

/// <summary>
/// Category service implementation
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PagedResult<CategoryListDto>>> GetAllAsync(
        string? search,
        Guid? parentCategoryId,
        bool? isActive,
        int pageNumber,
        int pageSize,
        string sortBy,
        string sortDirection,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        pageNumber = Math.Max(1, pageNumber);

        var query = _categoryRepository.Query();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Name.Contains(search));
        }

        if (parentCategoryId.HasValue)
        {
            query = query.Where(c => c.ParentCategoryId == parentCategoryId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        // Apply sorting
        query = sortBy.ToLowerInvariant() switch
        {
            "name" => sortDirection.ToLowerInvariant() == "desc"
                ? query.OrderByDescending(c => c.Name)
                : query.OrderBy(c => c.Name),
            "createdat" => sortDirection.ToLowerInvariant() == "desc"
                ? query.OrderByDescending(c => c.CreatedAt)
                : query.OrderBy(c => c.CreatedAt),
            _ => sortDirection.ToLowerInvariant() == "desc"
                ? query.OrderByDescending(c => c.DisplayOrder)
                : query.OrderBy(c => c.DisplayOrder)
        };

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and includes
        var categories = await query
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .Include(c => c.Products)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

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

        var pagedResult = new PagedResult<CategoryListDto>(dtos, totalCount, pageNumber, pageSize);
        return Result<PagedResult<CategoryListDto>>.Success(pagedResult);
    }

    public async Task<Result<CategoryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.Query()
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category is null)
        {
            return Result<CategoryDto>.Failure($"Category with ID {id} not found", "NOT_FOUND");
        }

        return Result<CategoryDto>.Success(MapToDto(category));
    }

    public async Task<Result<List<CategoryTreeDto>>> GetCategoryTreeAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _categoryRepository.Query()
            .Include(c => c.SubCategories)
            .AsQueryable();

        if (activeOnly)
        {
            query = query.Where(c => c.IsActive);
        }

        var allCategories = await query.ToListAsync(cancellationToken);
        var rootCategories = allCategories.Where(c => c.ParentCategoryId == null);

        var tree = rootCategories
            .OrderBy(c => c.DisplayOrder)
            .Select(c => BuildCategoryTree(c, allCategories, activeOnly))
            .ToList();

        return Result<List<CategoryTreeDto>>.Success(tree);
    }

    public async Task<Result<List<CategoryListDto>>> GetRootCategoriesAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetByParentIdAsync(null, cancellationToken);

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

        return Result<List<CategoryListDto>>.Success(dtos);
    }

    public async Task<Result<List<CategoryListDto>>> GetSubCategoriesAsync(Guid parentId, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var parent = await _categoryRepository.GetByIdAsync(parentId, cancellationToken);
        if (parent is null)
        {
            return Result<List<CategoryListDto>>.Failure($"Category with ID {parentId} not found", "NOT_FOUND");
        }

        var categories = await _categoryRepository.GetByParentIdAsync(parentId, cancellationToken);

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

        return Result<List<CategoryListDto>>.Success(dtos);
    }

    public async Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto dto, string? createdBy = null, CancellationToken cancellationToken = default)
    {
        // Validate parent category if specified
        if (dto.ParentCategoryId.HasValue)
        {
            var parent = await _categoryRepository.GetByIdAsync(dto.ParentCategoryId.Value, cancellationToken);
            if (parent is null)
            {
                return Result<CategoryDto>.Failure($"Parent category with ID {dto.ParentCategoryId} not found", "INVALID_PARENT");
            }
        }

        // Check for duplicate name under same parent
        var existingCategory = await _categoryRepository.Query()
            .FirstOrDefaultAsync(c => c.Name == dto.Name && c.ParentCategoryId == dto.ParentCategoryId, cancellationToken);

        if (existingCategory is not null)
        {
            return Result<CategoryDto>.Failure($"A category with name '{dto.Name}' already exists under the same parent", "DUPLICATE_NAME");
        }

        var category = Category.Create(
            dto.Name,
            dto.Description ?? string.Empty,
            slug: null, // Will be auto-generated from name
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

        category.CreatedBy = createdBy;

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category {CategoryId} created by {CreatedBy}", category.Id, createdBy);

        // Reload with parent for response
        var createdCategory = await _categoryRepository.Query()
            .Include(c => c.ParentCategory)
            .FirstAsync(c => c.Id == category.Id, cancellationToken);

        return Result<CategoryDto>.Success(MapToDto(createdCategory));
    }

    public async Task<Result<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryDto dto, string? updatedBy = null, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.Query()
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category is null)
        {
            return Result<CategoryDto>.Failure($"Category with ID {id} not found", "NOT_FOUND");
        }

        // Check for duplicate name under same parent (excluding current category)
        var existingCategory = await _categoryRepository.Query()
            .FirstOrDefaultAsync(c => c.Name == dto.Name
                && c.ParentCategoryId == category.ParentCategoryId
                && c.Id != id, cancellationToken);

        if (existingCategory is not null)
        {
            return Result<CategoryDto>.Failure($"A category with name '{dto.Name}' already exists under the same parent", "DUPLICATE_NAME");
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

        category.UpdatedBy = updatedBy;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category {CategoryId} updated by {UpdatedBy}", category.Id, updatedBy);

        return Result<CategoryDto>.Success(MapToDto(category));
    }

    public async Task<Result> DeleteAsync(Guid id, string? deletedBy = null, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.Query()
            .Include(c => c.SubCategories)
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category is null)
        {
            return Result.Failure($"Category with ID {id} not found", "NOT_FOUND");
        }

        // Check for subcategories
        if (category.SubCategories.Any(sc => !sc.IsDeleted))
        {
            return Result.Failure(
                "Cannot delete category with subcategories. Delete subcategories first or move them to another parent.",
                "HAS_SUBCATEGORIES");
        }

        // Check for products
        if (category.Products.Any(p => !p.IsDeleted))
        {
            return Result.Failure(
                "Cannot delete category with products. Remove or reassign products first.",
                "HAS_PRODUCTS");
        }

        category.DeletedBy = deletedBy;
        await _categoryRepository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category {CategoryId} deleted by {DeletedBy}", id, deletedBy);

        return Result.Success();
    }

    public async Task<Result<CategoryDto>> ActivateAsync(Guid id, string? updatedBy = null, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.Query()
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category is null)
        {
            return Result<CategoryDto>.Failure($"Category with ID {id} not found", "NOT_FOUND");
        }

        category.Activate();
        category.UpdatedBy = updatedBy;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category {CategoryId} activated by {UpdatedBy}", category.Id, updatedBy);

        return Result<CategoryDto>.Success(MapToDto(category));
    }

    public async Task<Result<CategoryDto>> DeactivateAsync(Guid id, string? updatedBy = null, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.Query()
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category is null)
        {
            return Result<CategoryDto>.Failure($"Category with ID {id} not found", "NOT_FOUND");
        }

        category.Deactivate();
        category.UpdatedBy = updatedBy;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category {CategoryId} deactivated by {UpdatedBy}", category.Id, updatedBy);

        return Result<CategoryDto>.Success(MapToDto(category));
    }

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
}
