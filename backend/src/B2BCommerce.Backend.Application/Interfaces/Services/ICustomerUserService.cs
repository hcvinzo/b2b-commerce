using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.CustomerUsers;

namespace B2BCommerce.Backend.Application.Interfaces.Services;

/// <summary>
/// Service interface for managing customer (dealer) users
/// </summary>
public interface ICustomerUserService
{
    /// <summary>
    /// Gets a paginated list of users for a customer
    /// </summary>
    Task<Result<PagedResult<CustomerUserListDto>>> GetUsersAsync(
        Guid customerId,
        int page = 1,
        int pageSize = 10,
        string? search = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific user by ID for a customer
    /// </summary>
    Task<Result<CustomerUserDetailDto>> GetUserByIdAsync(
        Guid customerId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new user for a customer
    /// </summary>
    Task<Result<CustomerUserDetailDto>> CreateUserAsync(
        Guid customerId,
        CreateCustomerUserDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a customer user
    /// </summary>
    Task<Result<CustomerUserDetailDto>> UpdateUserAsync(
        Guid customerId,
        Guid userId,
        UpdateCustomerUserDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates a customer user
    /// </summary>
    Task<Result> ActivateUserAsync(
        Guid customerId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a customer user
    /// </summary>
    Task<Result> DeactivateUserAsync(
        Guid customerId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the roles for a customer user
    /// </summary>
    Task<Result> SetUserRolesAsync(
        Guid customerId,
        Guid userId,
        SetCustomerUserRolesDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available customer roles that can be assigned
    /// </summary>
    Task<Result<List<CustomerUserRoleDto>>> GetAvailableRolesAsync(
        CancellationToken cancellationToken = default);
}
