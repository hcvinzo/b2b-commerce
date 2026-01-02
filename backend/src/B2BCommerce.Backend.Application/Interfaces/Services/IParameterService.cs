using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Parameters;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.Interfaces.Services;

/// <summary>
/// Service interface for system configuration parameter management
/// </summary>
public interface IParameterService
{
    #region CRUD Operations

    /// <summary>
    /// Gets all parameters with pagination and filtering
    /// </summary>
    Task<Result<PagedResult<ParameterListDto>>> GetAllAsync(
        int page,
        int pageSize,
        string? search = null,
        ParameterType? parameterType = null,
        string? category = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a parameter by ID
    /// </summary>
    Task<Result<ParameterDetailDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a parameter by key
    /// </summary>
    Task<Result<ParameterDetailDto>> GetByKeyAsync(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new parameter
    /// </summary>
    Task<Result<ParameterDetailDto>> CreateAsync(
        CreateParameterDto dto,
        string createdBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing parameter
    /// </summary>
    Task<Result<ParameterDetailDto>> UpdateAsync(
        Guid id,
        UpdateParameterDto dto,
        string updatedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a parameter (soft delete)
    /// </summary>
    Task<Result> DeleteAsync(
        Guid id,
        string deletedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all distinct categories
    /// </summary>
    Task<Result<List<string>>> GetCategoriesAsync(
        CancellationToken cancellationToken = default);

    #endregion

    #region Typed Value Getters

    /// <summary>
    /// Gets a parameter value as a specific type
    /// </summary>
    Task<T?> GetValueAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a parameter value as string
    /// </summary>
    Task<string?> GetStringAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a parameter value as integer
    /// </summary>
    Task<int?> GetIntAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a parameter value as boolean
    /// </summary>
    Task<bool?> GetBoolAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a parameter value as decimal
    /// </summary>
    Task<decimal?> GetDecimalAsync(string key, CancellationToken cancellationToken = default);

    #endregion
}
