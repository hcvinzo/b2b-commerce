using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Customers;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.Interfaces.Services;

/// <summary>
/// Service interface for customer operations
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// Get customer by ID
    /// </summary>
    Task<Result<CustomerDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get customer by tax number
    /// </summary>
    Task<Result<CustomerDto>> GetByTaxNoAsync(string taxNo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all customers with pagination, search, and filtering
    /// </summary>
    Task<Result<PagedResult<CustomerDto>>> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        bool? isActive = null,
        CustomerStatus? status = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get customers pending approval
    /// </summary>
    Task<Result<IEnumerable<CustomerDto>>> GetPendingCustomersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Update customer information
    /// </summary>
    Task<Result<CustomerDto>> UpdateAsync(Guid id, UpdateCustomerDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approve a customer account (set status to Active)
    /// </summary>
    Task<Result<CustomerDto>> ApproveAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reject a customer application
    /// </summary>
    Task<Result<CustomerDto>> RejectAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Suspend a customer account
    /// </summary>
    Task<Result<CustomerDto>> SuspendAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set customer status
    /// </summary>
    Task<Result<CustomerDto>> SetStatusAsync(Guid id, CustomerStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activate a customer account
    /// </summary>
    Task<Result> ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivate a customer account
    /// </summary>
    Task<Result> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a customer (soft delete)
    /// </summary>
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
