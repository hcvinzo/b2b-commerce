using System.Security.Claims;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Roles;
using B2BCommerce.Backend.Application.Interfaces.Services;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Infrastructure.Services;

/// <summary>
/// Service implementation for role and permission management
/// </summary>
public class RoleManagementService : IRoleManagementService
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<RoleManagementService> _logger;

    private const string PermissionClaimType = "permission";

    public RoleManagementService(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ILogger<RoleManagementService> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
    }

    #region Role CRUD

    public async Task<Result<PagedResult<RoleListDto>>> GetAllRolesAsync(
        int page,
        int pageSize,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = _roleManager.Roles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(r =>
                (r.Name != null && r.Name.ToLower().Contains(searchLower)) ||
                (r.Description != null && r.Description.ToLower().Contains(searchLower)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var roles = await query
            .OrderBy(r => r.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var roleDtos = new List<RoleListDto>();
        foreach (var role in roles)
        {
            var claims = await _roleManager.GetClaimsAsync(role);
            var userCount = (await _userManager.GetUsersInRoleAsync(role.Name!)).Count;

            roleDtos.Add(new RoleListDto
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty,
                Description = role.Description,
                UserCount = userCount,
                ClaimCount = claims.Count,
                IsProtected = AdminPermissionScopes.ProtectedRoles.Contains(role.Name!, StringComparer.OrdinalIgnoreCase),
                IsSystemRole = AdminPermissionScopes.SystemRoles.Contains(role.Name!, StringComparer.OrdinalIgnoreCase),
                CreatedAt = role.CreatedAt
            });
        }

        var pagedResult = new PagedResult<RoleListDto>(roleDtos, totalCount, page, pageSize);
        return Result<PagedResult<RoleListDto>>.Success(pagedResult);
    }

    public async Task<Result<RoleDetailDto>> GetRoleByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.Roles
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (role is null)
        {
            return Result<RoleDetailDto>.Failure("Role not found", "ROLE_NOT_FOUND");
        }

        var claims = await _roleManager.GetClaimsAsync(role);
        var userCount = (await _userManager.GetUsersInRoleAsync(role.Name!)).Count;

        var dto = new RoleDetailDto
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            Description = role.Description,
            Claims = claims
                .Where(c => c.Type == PermissionClaimType)
                .Select(c => c.Value)
                .ToList(),
            UserCount = userCount,
            IsProtected = AdminPermissionScopes.ProtectedRoles.Contains(role.Name!, StringComparer.OrdinalIgnoreCase),
            IsSystemRole = AdminPermissionScopes.SystemRoles.Contains(role.Name!, StringComparer.OrdinalIgnoreCase),
            CreatedAt = role.CreatedAt
        };

        return Result<RoleDetailDto>.Success(dto);
    }

    public async Task<Result<RoleDetailDto>> CreateRoleAsync(
        CreateRoleDto dto,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        // Validate role name format
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return Result<RoleDetailDto>.Failure("Role name is required", "NAME_REQUIRED");
        }

        if (!IsValidRoleName(dto.Name))
        {
            return Result<RoleDetailDto>.Failure(
                "Role name must start with a letter and contain only letters, numbers, underscores, and hyphens",
                "INVALID_NAME_FORMAT");
        }

        // Check if role already exists
        var existingRole = await _roleManager.FindByNameAsync(dto.Name);
        if (existingRole is not null)
        {
            return Result<RoleDetailDto>.Failure("A role with this name already exists", "ROLE_EXISTS");
        }

        // Validate claims if provided
        if (dto.Claims is not null && dto.Claims.Any())
        {
            var invalidClaims = dto.Claims.Where(c => !AdminPermissionScopes.IsValidScope(c)).ToList();
            if (invalidClaims.Any())
            {
                return Result<RoleDetailDto>.Failure(
                    $"Invalid permission scopes: {string.Join(", ", invalidClaims)}",
                    "INVALID_CLAIMS");
            }
        }

        // Create the role
        var role = new ApplicationRole(dto.Name, dto.Description);

        var createResult = await _roleManager.CreateAsync(role);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Role creation failed: {Errors}", errors);
            return Result<RoleDetailDto>.Failure($"Failed to create role: {errors}", "ROLE_CREATION_FAILED");
        }

        // Add claims if provided
        if (dto.Claims is not null && dto.Claims.Any())
        {
            foreach (var claim in dto.Claims.Distinct())
            {
                await _roleManager.AddClaimAsync(role, new Claim(PermissionClaimType, claim));
            }
        }

        _logger.LogInformation("Role {RoleName} created by {CreatedBy}", dto.Name, createdBy);

        return await GetRoleByIdAsync(role.Id, cancellationToken);
    }

    public async Task<Result<RoleDetailDto>> UpdateRoleAsync(
        Guid id,
        UpdateRoleDto dto,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.Roles
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (role is null)
        {
            return Result<RoleDetailDto>.Failure("Role not found", "ROLE_NOT_FOUND");
        }

        // Check if trying to rename a system role
        var isSystemRole = AdminPermissionScopes.SystemRoles.Contains(role.Name!, StringComparer.OrdinalIgnoreCase);
        if (isSystemRole && dto.Name is not null && !string.Equals(dto.Name, role.Name, StringComparison.OrdinalIgnoreCase))
        {
            return Result<RoleDetailDto>.Failure("Cannot rename system roles", "SYSTEM_ROLE_RENAME_FORBIDDEN");
        }

        // Update name if provided
        if (dto.Name is not null && !string.Equals(dto.Name, role.Name, StringComparison.OrdinalIgnoreCase))
        {
            if (!IsValidRoleName(dto.Name))
            {
                return Result<RoleDetailDto>.Failure(
                    "Role name must start with a letter and contain only letters, numbers, underscores, and hyphens",
                    "INVALID_NAME_FORMAT");
            }

            // Check if new name already exists
            var existingRole = await _roleManager.FindByNameAsync(dto.Name);
            if (existingRole is not null)
            {
                return Result<RoleDetailDto>.Failure("A role with this name already exists", "ROLE_EXISTS");
            }

            role.Name = dto.Name;
            role.NormalizedName = dto.Name.ToUpperInvariant();
        }

        // Update description
        if (dto.Description is not null)
        {
            role.Description = dto.Description;
        }

        var updateResult = await _roleManager.UpdateAsync(role);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            return Result<RoleDetailDto>.Failure($"Failed to update role: {errors}", "ROLE_UPDATE_FAILED");
        }

        _logger.LogInformation("Role {RoleId} updated by {UpdatedBy}", id, updatedBy);

        return await GetRoleByIdAsync(id, cancellationToken);
    }

    public async Task<Result> DeleteRoleAsync(
        Guid id,
        string deletedBy,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.Roles
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (role is null)
        {
            return Result.Failure("Role not found", "ROLE_NOT_FOUND");
        }

        // Check if protected role
        if (AdminPermissionScopes.ProtectedRoles.Contains(role.Name!, StringComparer.OrdinalIgnoreCase))
        {
            return Result.Failure("Cannot delete protected roles", "PROTECTED_ROLE");
        }

        // Check if users are assigned to this role
        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
        if (usersInRole.Any())
        {
            return Result.Failure(
                $"Cannot delete role with assigned users. {usersInRole.Count} user(s) are assigned to this role.",
                "ROLE_HAS_USERS");
        }

        var deleteResult = await _roleManager.DeleteAsync(role);
        if (!deleteResult.Succeeded)
        {
            var errors = string.Join(", ", deleteResult.Errors.Select(e => e.Description));
            return Result.Failure($"Failed to delete role: {errors}", "ROLE_DELETION_FAILED");
        }

        _logger.LogInformation("Role {RoleName} deleted by {DeletedBy}", role.Name, deletedBy);

        return Result.Success();
    }

    #endregion

    #region Claims Management

    public Task<Result<List<PermissionCategoryDto>>> GetAvailablePermissionsAsync(
        CancellationToken cancellationToken = default)
    {
        var categories = AdminPermissionScopes.GetCategories()
            .Select(c => new PermissionCategoryDto
            {
                Name = c.Name,
                Description = c.Description,
                Permissions = c.Permissions.Select(p => new AvailablePermissionDto
                {
                    Value = p.Value,
                    DisplayName = p.DisplayName,
                    Description = p.Description,
                    Category = c.Name
                }).ToList()
            })
            .ToList();

        return Task.FromResult(Result<List<PermissionCategoryDto>>.Success(categories));
    }

    public async Task<Result<List<string>>> GetRoleClaimsAsync(
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.Roles
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

        if (role is null)
        {
            return Result<List<string>>.Failure("Role not found", "ROLE_NOT_FOUND");
        }

        var claims = await _roleManager.GetClaimsAsync(role);
        var permissionClaims = claims
            .Where(c => c.Type == PermissionClaimType)
            .Select(c => c.Value)
            .ToList();

        return Result<List<string>>.Success(permissionClaims);
    }

    public async Task<Result> SetRoleClaimsAsync(
        Guid roleId,
        List<string> claims,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.Roles
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

        if (role is null)
        {
            return Result.Failure("Role not found", "ROLE_NOT_FOUND");
        }

        // Validate claims
        var invalidClaims = claims.Where(c => !AdminPermissionScopes.IsValidScope(c)).ToList();
        if (invalidClaims.Any())
        {
            return Result.Failure(
                $"Invalid permission scopes: {string.Join(", ", invalidClaims)}",
                "INVALID_CLAIMS");
        }

        // Get existing permission claims
        var existingClaims = await _roleManager.GetClaimsAsync(role);
        var existingPermissionClaims = existingClaims
            .Where(c => c.Type == PermissionClaimType)
            .ToList();

        // Remove all existing permission claims
        foreach (var claim in existingPermissionClaims)
        {
            await _roleManager.RemoveClaimAsync(role, claim);
        }

        // Add new claims
        foreach (var claim in claims.Distinct())
        {
            await _roleManager.AddClaimAsync(role, new Claim(PermissionClaimType, claim));
        }

        _logger.LogInformation("Claims updated for role {RoleId} by {UpdatedBy}", roleId, updatedBy);

        return Result.Success();
    }

    public async Task<Result> AddClaimToRoleAsync(
        Guid roleId,
        string claimValue,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.Roles
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

        if (role is null)
        {
            return Result.Failure("Role not found", "ROLE_NOT_FOUND");
        }

        // Validate claim
        if (!AdminPermissionScopes.IsValidScope(claimValue))
        {
            return Result.Failure($"Invalid permission scope: {claimValue}", "INVALID_CLAIM");
        }

        // Check if claim already exists
        var existingClaims = await _roleManager.GetClaimsAsync(role);
        if (existingClaims.Any(c => c.Type == PermissionClaimType && c.Value == claimValue))
        {
            return Result.Failure("Permission already assigned to this role", "CLAIM_EXISTS");
        }

        await _roleManager.AddClaimAsync(role, new Claim(PermissionClaimType, claimValue));

        _logger.LogInformation("Claim {ClaimValue} added to role {RoleId} by {UpdatedBy}", claimValue, roleId, updatedBy);

        return Result.Success();
    }

    public async Task<Result> RemoveClaimFromRoleAsync(
        Guid roleId,
        string claimValue,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.Roles
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

        if (role is null)
        {
            return Result.Failure("Role not found", "ROLE_NOT_FOUND");
        }

        var existingClaims = await _roleManager.GetClaimsAsync(role);
        var claimToRemove = existingClaims.FirstOrDefault(c => c.Type == PermissionClaimType && c.Value == claimValue);

        if (claimToRemove is null)
        {
            return Result.Failure("Permission not found on this role", "CLAIM_NOT_FOUND");
        }

        await _roleManager.RemoveClaimAsync(role, claimToRemove);

        _logger.LogInformation("Claim {ClaimValue} removed from role {RoleId} by {UpdatedBy}", claimValue, roleId, updatedBy);

        return Result.Success();
    }

    #endregion

    #region User-Role Management

    public async Task<Result<PagedResult<RoleUserListDto>>> GetUsersInRoleAsync(
        Guid roleId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.Roles
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

        if (role is null)
        {
            return Result<PagedResult<RoleUserListDto>>.Failure("Role not found", "ROLE_NOT_FOUND");
        }

        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
        var totalCount = usersInRole.Count;

        var pagedUsers = usersInRole
            .OrderBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new RoleUserListDto
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                FullName = $"{u.FirstName} {u.LastName}".Trim(),
                IsActive = u.IsActive
            })
            .ToList();

        var pagedResult = new PagedResult<RoleUserListDto>(pagedUsers, totalCount, page, pageSize);
        return Result<PagedResult<RoleUserListDto>>.Success(pagedResult);
    }

    public async Task<Result> AddUserToRoleAsync(
        Guid roleId,
        Guid userId,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.Roles
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

        if (role is null)
        {
            return Result.Failure("Role not found", "ROLE_NOT_FOUND");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure("User not found", "USER_NOT_FOUND");
        }

        // Check if user already has this role
        if (await _userManager.IsInRoleAsync(user, role.Name!))
        {
            return Result.Failure("User already has this role", "USER_ALREADY_IN_ROLE");
        }

        var result = await _userManager.AddToRoleAsync(user, role.Name!);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure($"Failed to add user to role: {errors}", "ADD_TO_ROLE_FAILED");
        }

        _logger.LogInformation("User {UserId} added to role {RoleId} by {UpdatedBy}", userId, roleId, updatedBy);

        return Result.Success();
    }

    public async Task<Result> RemoveUserFromRoleAsync(
        Guid roleId,
        Guid userId,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.Roles
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

        if (role is null)
        {
            return Result.Failure("Role not found", "ROLE_NOT_FOUND");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure("User not found", "USER_NOT_FOUND");
        }

        // Check if user has this role
        if (!await _userManager.IsInRoleAsync(user, role.Name!))
        {
            return Result.Failure("User does not have this role", "USER_NOT_IN_ROLE");
        }

        var result = await _userManager.RemoveFromRoleAsync(user, role.Name!);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure($"Failed to remove user from role: {errors}", "REMOVE_FROM_ROLE_FAILED");
        }

        _logger.LogInformation("User {UserId} removed from role {RoleId} by {UpdatedBy}", userId, roleId, updatedBy);

        return Result.Success();
    }

    #endregion

    #region Private Helpers

    private static bool IsValidRoleName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        // Must start with a letter
        if (!char.IsLetter(name[0]))
            return false;

        // Can only contain letters, numbers, underscores, and hyphens
        return name.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-');
    }

    #endregion
}
