using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace B2BCommerce.Backend.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for product type operations
/// </summary>
public class ProductTypeRepository : GenericRepository<ProductType>, IProductTypeRepository
{
    public ProductTypeRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets a product type by its code
    /// </summary>
    public async Task<ProductType?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pt => pt.Attributes.Where(a => !a.IsDeleted))
                .ThenInclude(a => a.AttributeDefinition)
            .FirstOrDefaultAsync(pt => pt.Code == code && !pt.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets a product type with its attribute definitions loaded
    /// </summary>
    public async Task<ProductType?> GetWithAttributesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pt => pt.Attributes.Where(a => !a.IsDeleted).OrderBy(a => a.DisplayOrder))
                .ThenInclude(a => a.AttributeDefinition)
                    .ThenInclude(ad => ad.PredefinedValues.Where(v => !v.IsDeleted).OrderBy(v => v.DisplayOrder))
            .FirstOrDefaultAsync(pt => pt.Id == id && !pt.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets a product type by code with its attribute definitions loaded
    /// </summary>
    public async Task<ProductType?> GetByCodeWithAttributesAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pt => pt.Attributes.Where(a => !a.IsDeleted).OrderBy(a => a.DisplayOrder))
                .ThenInclude(a => a.AttributeDefinition)
                    .ThenInclude(ad => ad.PredefinedValues.Where(v => !v.IsDeleted).OrderBy(v => v.DisplayOrder))
            .FirstOrDefaultAsync(pt => pt.Code == code && !pt.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets all active product types
    /// </summary>
    public async Task<IEnumerable<ProductType>> GetActiveProductTypesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pt => pt.Attributes.Where(a => !a.IsDeleted).OrderBy(a => a.DisplayOrder))
                .ThenInclude(a => a.AttributeDefinition)
            .Where(pt => pt.IsActive && !pt.IsDeleted)
            .OrderBy(pt => pt.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets product types with pagination
    /// </summary>
    public async Task<(IEnumerable<ProductType> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(pt => !pt.IsDeleted);

        if (isActive.HasValue)
        {
            query = query.Where(pt => pt.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(pt => pt.Attributes.Where(a => !a.IsDeleted))
            .OrderBy(pt => pt.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
