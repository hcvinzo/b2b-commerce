using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace B2BCommerce.Backend.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for product category relationship operations
/// </summary>
public class ProductCategoryRepository : GenericRepository<ProductCategory>, IProductCategoryRepository
{
    public ProductCategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets all category assignments for a product
    /// </summary>
    public async Task<IEnumerable<ProductCategory>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pc => pc.Category)
            .Where(pc => pc.ProductId == productId && !pc.IsDeleted)
            .OrderBy(pc => pc.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all product assignments for a category
    /// </summary>
    public async Task<IEnumerable<ProductCategory>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pc => pc.Product)
            .Where(pc => pc.CategoryId == categoryId && !pc.IsDeleted)
            .OrderBy(pc => pc.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the primary category for a product
    /// </summary>
    public async Task<ProductCategory?> GetPrimaryCategoryAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pc => pc.Category)
            .FirstOrDefaultAsync(
                pc => pc.ProductId == productId && pc.IsPrimary && !pc.IsDeleted,
                cancellationToken);
    }

    /// <summary>
    /// Checks if a product is assigned to a category
    /// </summary>
    public async Task<bool> ExistsAsync(Guid productId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(
                pc => pc.ProductId == productId && pc.CategoryId == categoryId && !pc.IsDeleted,
                cancellationToken);
    }

    /// <summary>
    /// Gets product IDs for products in a category (including subcategories)
    /// </summary>
    public async Task<IEnumerable<Guid>> GetProductIdsInCategoryAsync(
        Guid categoryId,
        bool includeSubcategories = false,
        CancellationToken cancellationToken = default)
    {
        if (!includeSubcategories)
        {
            return await _dbSet
                .Where(pc => pc.CategoryId == categoryId && !pc.IsDeleted)
                .Select(pc => pc.ProductId)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        // Get all category IDs including subcategories
        var categoryIds = new List<Guid> { categoryId };
        await GetSubcategoryIdsRecursiveAsync(categoryId, categoryIds, cancellationToken);

        return await _dbSet
            .Where(pc => categoryIds.Contains(pc.CategoryId) && !pc.IsDeleted)
            .Select(pc => pc.ProductId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private async Task GetSubcategoryIdsRecursiveAsync(
        Guid parentId,
        List<Guid> categoryIds,
        CancellationToken cancellationToken)
    {
        var subcategoryIds = await _context.Categories
            .Where(c => c.ParentCategoryId == parentId && !c.IsDeleted)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        foreach (var subcategoryId in subcategoryIds)
        {
            categoryIds.Add(subcategoryId);
            await GetSubcategoryIdsRecursiveAsync(subcategoryId, categoryIds, cancellationToken);
        }
    }

    /// <summary>
    /// Sets the primary category for a product (clears other primary flags)
    /// </summary>
    public async Task SetPrimaryCategoryAsync(Guid productId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        // Clear existing primary flag
        var existingPrimary = await _dbSet
            .Where(pc => pc.ProductId == productId && pc.IsPrimary && !pc.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var pc in existingPrimary)
        {
            pc.RemovePrimaryStatus();
        }

        // Set new primary
        var newPrimary = await _dbSet
            .FirstOrDefaultAsync(
                pc => pc.ProductId == productId && pc.CategoryId == categoryId && !pc.IsDeleted,
                cancellationToken);

        if (newPrimary != null)
        {
            newPrimary.SetAsPrimary();
        }
    }
}
