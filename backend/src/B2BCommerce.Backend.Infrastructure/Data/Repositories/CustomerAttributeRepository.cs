using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;
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
            .Where(ca => ca.CustomerId == customerId)
            .OrderBy(ca => ca.AttributeType)
            .ThenBy(ca => ca.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CustomerAttribute>> GetByCustomerIdAndTypeAsync(
        Guid customerId,
        CustomerAttributeType attributeType,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(ca => ca.CustomerId == customerId && ca.AttributeType == attributeType)
            .OrderBy(ca => ca.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteByCustomerIdAndTypeAsync(
        Guid customerId,
        CustomerAttributeType attributeType,
        CancellationToken cancellationToken = default)
    {
        var attributes = await _dbSet
            .Where(ca => ca.CustomerId == customerId && ca.AttributeType == attributeType)
            .ToListAsync(cancellationToken);

        foreach (var attribute in attributes)
        {
            attribute.MarkAsDeleted();
        }
    }
}
