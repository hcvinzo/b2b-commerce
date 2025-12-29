using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Customers;

namespace B2BCommerce.Backend.Application.Interfaces.Services;

/// <summary>
/// Service interface for customer attribute operations
/// </summary>
public interface ICustomerAttributeService
{
    /// <summary>
    /// Get all attributes for a customer
    /// </summary>
    Task<Result<IEnumerable<CustomerAttributeDto>>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get attributes for a customer by attribute definition
    /// </summary>
    Task<Result<IEnumerable<CustomerAttributeDto>>> GetByCustomerIdAndDefinitionIdAsync(
        Guid customerId,
        Guid attributeDefinitionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a single attribute by ID
    /// </summary>
    Task<Result<CustomerAttributeDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new customer attribute
    /// </summary>
    Task<Result<CustomerAttributeDto>> CreateAsync(
        Guid customerId,
        CreateCustomerAttributeDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing customer attribute
    /// </summary>
    Task<Result<CustomerAttributeDto>> UpdateAsync(
        Guid id,
        UpdateCustomerAttributeDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a customer attribute
    /// </summary>
    Task<Result> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upsert attributes by definition (replaces all existing attributes of the specified definition)
    /// </summary>
    Task<Result<IEnumerable<CustomerAttributeDto>>> UpsertByDefinitionAsync(
        Guid customerId,
        UpsertCustomerAttributesByDefinitionDto dto,
        CancellationToken cancellationToken = default);
}
