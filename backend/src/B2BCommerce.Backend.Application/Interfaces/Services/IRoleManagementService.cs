using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Roles;

namespace B2BCommerce.Backend.Application.Interfaces.Services;

/// <summary>
/// Service interface for role and permission management operations
/// </summary>
public interface IRoleManagementService
{
    #region Role CRUD

    /// <summary>
    /// Gets all roles with pagination
    /// </summary>
    Task<Result<PagedResult<RoleListDto>>> GetAllRolesAsync(
        int page,
        int pageSize,
        string? search = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a role by ID with full details
    /// </summary>
    Task<Result<RoleDetailDto>> GetRoleByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new role
    /// </summary>
    Task<Result<RoleDetailDto>> CreateRoleAsync(
        CreateRoleDto dto,
        string createdBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing role
    /// </summary>
    Task<Result<RoleDetailDto>> UpdateRoleAsync(
        Guid id,
        UpdateRoleDto dto,
        string updatedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a role (fails if users are assigned or if it's a protected role)
    /// </summary>
    Task<Result> DeleteRoleAsync(
        Guid id,
        string deletedBy,
        CancellationToken cancellationToken = default);

    #endregion

    #region Claims Management

    /// <summary>
    /// Gets all available permissions grouped by category
    /// </summary>
    Task<Result<List<PermissionCategoryDto>>> GetAvailablePermissionsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all claims for a specific role
    /// </summary>
    Task<Result<List<string>>> GetRoleClaimsAsync(
        Guid roleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets all claims for a role (replaces existing claims)
    /// </summary>
    Task<Result> SetRoleClaimsAsync(
        Guid roleId,
        List<string> claims,
        string updatedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a single claim to a role
    /// </summary>
    Task<Result> AddClaimToRoleAsync(
        Guid roleId,
        string claimValue,
        string updatedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a single claim from a role
    /// </summary>
    Task<Result> RemoveClaimFromRoleAsync(
        Guid roleId,
        string claimValue,
        string updatedBy,
        CancellationToken cancellationToken = default);

    #endregion

    #region User-Role Management

    /// <summary>
    /// Gets all users assigned to a specific role
    /// </summary>
    Task<Result<PagedResult<RoleUserListDto>>> GetUsersInRoleAsync(
        Guid roleId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a user to a role
    /// </summary>
    Task<Result> AddUserToRoleAsync(
        Guid roleId,
        Guid userId,
        string updatedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a user from a role
    /// </summary>
    Task<Result> RemoveUserFromRoleAsync(
        Guid roleId,
        Guid userId,
        string updatedBy,
        CancellationToken cancellationToken = default);

    #endregion
}
