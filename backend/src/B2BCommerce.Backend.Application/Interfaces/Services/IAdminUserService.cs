using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.AdminUsers;

namespace B2BCommerce.Backend.Application.Interfaces.Services;

/// <summary>
/// Service interface for admin user management operations
/// </summary>
public interface IAdminUserService
{
    /// <summary>
    /// Gets all admin users with pagination
    /// </summary>
    Task<Result<PagedResult<AdminUserListDto>>> GetAllAsync(
        int page,
        int pageSize,
        bool? isActive = null,
        string? search = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an admin user by ID with details
    /// </summary>
    Task<Result<AdminUserDetailDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new admin user
    /// </summary>
    Task<Result<AdminUserDetailDto>> CreateAsync(
        CreateAdminUserDto dto,
        string createdBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing admin user
    /// </summary>
    Task<Result<AdminUserDetailDto>> UpdateAsync(
        Guid id,
        UpdateAdminUserDto dto,
        string updatedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates an admin user
    /// </summary>
    Task<Result> ActivateAsync(
        Guid id,
        string updatedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates an admin user (cannot deactivate self)
    /// </summary>
    Task<Result> DeactivateAsync(
        Guid id,
        Guid currentUserId,
        string updatedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an admin user (cannot delete self)
    /// </summary>
    Task<Result> DeleteAsync(
        Guid id,
        Guid currentUserId,
        string deletedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets the password for an admin user
    /// </summary>
    Task<Result> ResetPasswordAsync(
        Guid id,
        string updatedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available roles for admin users
    /// </summary>
    Task<Result<List<AvailableRoleDto>>> GetAvailableRolesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user roles
    /// </summary>
    Task<Result<List<string>>> GetUserRolesAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets user roles (replaces all existing roles)
    /// </summary>
    Task<Result> SetUserRolesAsync(
        Guid id,
        SetUserRolesDto dto,
        string updatedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user login history (external providers)
    /// </summary>
    Task<Result<List<UserLoginDto>>> GetUserLoginsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user claims
    /// </summary>
    Task<Result<List<UserClaimDto>>> GetUserClaimsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a claim to user
    /// </summary>
    Task<Result> AddUserClaimAsync(
        Guid id,
        AddUserClaimDto dto,
        string updatedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a claim from user
    /// </summary>
    Task<Result> RemoveUserClaimAsync(
        Guid id,
        int claimId,
        string updatedBy,
        CancellationToken cancellationToken = default);
}
