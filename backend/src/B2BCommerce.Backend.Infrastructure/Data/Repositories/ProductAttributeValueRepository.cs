using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace B2BCommerce.Backend.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for product attribute value operations
/// </summary>
public class ProductAttributeValueRepository : GenericRepository<ProductAttributeValue>, IProductAttributeValueRepository
{
    public ProductAttributeValueRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets all attribute values for a product
    /// </summary>
    public async Task<IEnumerable<ProductAttributeValue>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pav => pav.AttributeDefinition)
            .Include(pav => pav.SelectedValue)
            .Where(pav => pav.ProductId == productId && !pav.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a specific attribute value for a product
    /// </summary>
    public async Task<ProductAttributeValue?> GetByProductAndAttributeAsync(
        Guid productId,
        Guid attributeDefinitionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pav => pav.AttributeDefinition)
            .Include(pav => pav.SelectedValue)
            .FirstOrDefaultAsync(
                pav => pav.ProductId == productId &&
                       pav.AttributeDefinitionId == attributeDefinitionId &&
                       !pav.IsDeleted,
                cancellationToken);
    }

    /// <summary>
    /// Gets products with a specific attribute value (for filtering)
    /// </summary>
    public async Task<IEnumerable<Guid>> GetProductIdsByAttributeValueAsync(
        Guid attributeDefinitionId,
        Guid attributeValueId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(pav =>
                pav.AttributeDefinitionId == attributeDefinitionId &&
                pav.AttributeValueId == attributeValueId &&
                !pav.IsDeleted)
            .Select(pav => pav.ProductId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets products with numeric attribute in range (for filtering)
    /// </summary>
    public async Task<IEnumerable<Guid>> GetProductIdsByNumericRangeAsync(
        Guid attributeDefinitionId,
        decimal? minValue,
        decimal? maxValue,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(pav =>
                pav.AttributeDefinitionId == attributeDefinitionId &&
                pav.NumericValue.HasValue &&
                !pav.IsDeleted);

        if (minValue.HasValue)
        {
            query = query.Where(pav => pav.NumericValue >= minValue.Value);
        }

        if (maxValue.HasValue)
        {
            query = query.Where(pav => pav.NumericValue <= maxValue.Value);
        }

        return await query
            .Select(pav => pav.ProductId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Deletes all attribute values for a product
    /// </summary>
    public async Task DeleteByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var values = await _dbSet
            .Where(pav => pav.ProductId == productId && !pav.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var value in values)
        {
            value.MarkAsDeleted();
        }
    }
}
