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
    /// Gets all attribute definitions with predefined values loaded
    /// </summary>
    public async Task<IEnumerable<AttributeDefinition>> GetAllWithValuesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.PredefinedValues.Where(v => !v.IsDeleted).OrderBy(v => v.DisplayOrder))
            .Where(a => !a.IsDeleted)
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

    /// <summary>
    /// Gets an attribute definition by its external ID (primary key for LOGO ERP integration)
    /// </summary>
    public async Task<AttributeDefinition?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(a => a.ExternalId == externalId && !a.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets attribute definition with predefined values by external ID
    /// </summary>
    public async Task<AttributeDefinition?> GetWithPredefinedValuesByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.PredefinedValues.Where(v => !v.IsDeleted).OrderBy(v => v.DisplayOrder))
            .FirstOrDefaultAsync(a => a.ExternalId == externalId && !a.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Checks if an attribute definition exists by its external ID
    /// </summary>
    public async Task<bool> ExistsByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(a => a.ExternalId == externalId && !a.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Adds a predefined value to the context for tracking
    /// </summary>
    public async Task AddAttributeValueAsync(AttributeValue value, CancellationToken cancellationToken = default)
    {
        await _context.Set<AttributeValue>().AddAsync(value, cancellationToken);
    }
}
