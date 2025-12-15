using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace B2BCommerce.Backend.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for attribute definition operations
/// </summary>
public class AttributeDefinitionRepository : GenericRepository<AttributeDefinition>, IAttributeDefinitionRepository
{
    public AttributeDefinitionRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets an attribute definition by its code
    /// </summary>
    public async Task<AttributeDefinition?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.PredefinedValues.Where(v => !v.IsDeleted))
            .FirstOrDefaultAsync(a => a.Code == code && !a.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets all filterable attribute definitions
    /// </summary>
    public async Task<IEnumerable<AttributeDefinition>> GetFilterableAttributesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.PredefinedValues.Where(v => !v.IsDeleted).OrderBy(v => v.DisplayOrder))
            .Where(a => a.IsFilterable && !a.IsDeleted)
            .OrderBy(a => a.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets attribute definition with predefined values loaded
    /// </summary>
    public async Task<AttributeDefinition?> GetWithPredefinedValuesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.PredefinedValues.Where(v => !v.IsDeleted).OrderBy(v => v.DisplayOrder))
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);
    }
}
