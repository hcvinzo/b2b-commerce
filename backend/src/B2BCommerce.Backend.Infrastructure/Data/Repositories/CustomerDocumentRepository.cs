using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace B2BCommerce.Backend.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for CustomerDocument entity
/// </summary>
public class CustomerDocumentRepository : GenericRepository<CustomerDocument>, ICustomerDocumentRepository
{
    public CustomerDocumentRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CustomerDocument>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(cd => cd.CustomerId == customerId)
            .OrderBy(cd => cd.DocumentType)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CustomerDocument?> GetByCustomerIdAndTypeAsync(
        Guid customerId,
        CustomerDocumentType documentType,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(cd => cd.CustomerId == customerId && cd.DocumentType == documentType, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByCustomerIdAndTypeAsync(
        Guid customerId,
        CustomerDocumentType documentType,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(cd => cd.CustomerId == customerId && cd.DocumentType == documentType, cancellationToken);
    }
}
