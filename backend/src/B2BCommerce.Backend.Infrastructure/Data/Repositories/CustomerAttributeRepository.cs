using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace B2BCommerce.Backend.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for CustomerAttribute entity
/// </summary>
public class CustomerAttributeRepository : GenericRepository<CustomerAttribute>, ICustomerAttributeRepository
{
    public CustomerAttributeRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CustomerAttribute>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(ca => ca.AttributeDefinition)
            .Where(ca => ca.CustomerId == customerId)
            .OrderBy(ca => ca.AttributeDefinition.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CustomerAttribute?> GetByCustomerIdAndDefinitionIdAsync(
        Guid customerId,
        Guid attributeDefinitionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(ca => ca.AttributeDefinition)
            .FirstOrDefaultAsync(ca => ca.CustomerId == customerId && ca.AttributeDefinitionId == attributeDefinitionId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CustomerAttribute?> GetByCustomerIdAndDefinitionCodeAsync(
        Guid customerId,
        string definitionCode,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(ca => ca.AttributeDefinition)
            .FirstOrDefaultAsync(ca => ca.CustomerId == customerId && ca.AttributeDefinition.Code == definitionCode, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteByCustomerIdAndDefinitionIdAsync(
        Guid customerId,
        Guid attributeDefinitionId,
        CancellationToken cancellationToken = default)
    {
        var attributes = await _dbSet
            .Where(ca => ca.CustomerId == customerId && ca.AttributeDefinitionId == attributeDefinitionId)
            .ToListAsync(cancellationToken);

        foreach (var attribute in attributes)
        {
            _dbSet.Remove(attribute);
        }
    }
}
