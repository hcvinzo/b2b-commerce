using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Customers;

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
    /// Get customer by email
    /// </summary>
    Task<Result<CustomerDto>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all customers with pagination, search, and filtering
    /// </summary>
    Task<Result<PagedResult<CustomerDto>>> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        bool? isActive = null,
        bool? isApproved = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get customers pending approval
    /// </summary>
    Task<Result<IEnumerable<CustomerDto>>> GetUnapprovedCustomersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Update customer information
    /// </summary>
    Task<Result<CustomerDto>> UpdateAsync(Guid id, UpdateCustomerDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approve a customer account
    /// </summary>
    Task<Result<CustomerDto>> ApproveAsync(Guid id, string approvedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update customer credit limit
    /// </summary>
    Task<Result<CustomerDto>> UpdateCreditLimitAsync(Guid id, decimal newCreditLimit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get customer's available credit
    /// </summary>
    Task<Result<decimal>> GetAvailableCreditAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if customer's credit is near limit
    /// </summary>
    Task<Result<bool>> IsCreditNearLimitAsync(Guid id, decimal thresholdPercentage = 90, CancellationToken cancellationToken = default);

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
