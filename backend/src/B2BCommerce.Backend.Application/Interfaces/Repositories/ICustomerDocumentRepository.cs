using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for CustomerDocument entity operations
/// </summary>
public interface ICustomerDocumentRepository : IGenericRepository<CustomerDocument>
{
    /// <summary>
    /// Gets all documents for a specific customer
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of customer documents</returns>
    Task<IEnumerable<CustomerDocument>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific document by customer ID and document type
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <param name="documentType">The document type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The document if found, null otherwise</returns>
    Task<CustomerDocument?> GetByCustomerIdAndTypeAsync(
        Guid customerId,
        CustomerDocumentType documentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a document exists for a customer with a specific type
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <param name="documentType">The document type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if document exists, false otherwise</returns>
    Task<bool> ExistsByCustomerIdAndTypeAsync(
        Guid customerId,
        CustomerDocumentType documentType,
        CancellationToken cancellationToken = default);
}
