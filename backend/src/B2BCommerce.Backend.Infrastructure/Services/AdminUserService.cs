using System.Security.Claims;
using System.Security.Cryptography;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.AdminUsers;
using B2BCommerce.Backend.Application.Interfaces.Services;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Infrastructure.Data;
using B2BCommerce.Backend.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IdentityUserClaim = Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>;

namespace B2BCommerce.Backend.Infrastructure.Services;

/// <summary>
/// Service implementation for admin user management
/// </summary>
public class AdminUserService : IAdminUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<AdminUserService> _logger;

    // Admin-assignable roles (Customer role is not assignable through this interface)
    private static readonly string[] AdminAssignableRoles = { "Admin", "SalesRep" };

    public AdminUserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ApplicationDbContext dbContext,
        ILogger<AdminUserService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<PagedResult<AdminUserListDto>>> GetAllAsync(
        int page,
        int pageSize,
        bool? isActive = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        // Get all admin users (UserType.Admin)
        var query = _userManager.Users
            .Where(u => u.UserType == UserType.Admin);

        // Apply filters
        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(u =>
                (u.Email != null && u.Email.ToLower().Contains(searchLower)) ||
                (u.FirstName != null && u.FirstName.ToLower().Contains(searchLower)) ||
                (u.LastName != null && u.LastName.ToLower().Contains(searchLower)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Get roles for all users
        var userDtos = new List<AdminUserListDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new AdminUserListDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Roles = roles.ToList(),
                IsActive = user.IsActive,
                LastLoginAt = user.LastLoginAt,
                CreatedAt = user.CreatedAt
            });
        }

        var pagedResult = new PagedResult<AdminUserListDto>(userDtos, totalCount, page, pageSize);
        return Result<PagedResult<AdminUserListDto>>.Success(pagedResult);
    }

    public async Task<Result<AdminUserDetailDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.UserType == UserType.Admin, cancellationToken);

        if (user is null)
        {
            return Result<AdminUserDetailDto>.Failure("Admin user not found", "USER_NOT_FOUND");
        }

        var roles = await _userManager.GetRolesAsync(user);

        var dto = new AdminUserDetailDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Roles = roles.ToList(),
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        };

        return Result<AdminUserDetailDto>.Success(dto);
    }

    public async Task<Result<AdminUserDetailDto>> CreateAsync(
        CreateAdminUserDto dto,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        // Check if email already exists
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser is not null)
        {
            return Result<AdminUserDetailDto>.Failure("Email already registered", "EMAIL_EXISTS");
        }

        // Validate roles - only allow admin-assignable roles
        var invalidRoles = dto.Roles.Where(r => !AdminAssignableRoles.Contains(r, StringComparer.OrdinalIgnoreCase)).ToList();
        if (invalidRoles.Any())
        {
            return Result<AdminUserDetailDto>.Failure(
                $"Invalid roles: {string.Join(", ", invalidRoles)}. Only Admin and SalesRep roles are allowed.",
                "INVALID_ROLES");
        }

        // Generate password if not provided
        var password = !string.IsNullOrWhiteSpace(dto.TemporaryPassword)
            ? dto.TemporaryPassword
            : GenerateTemporaryPassword();

        // Create the user
        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            UserType = UserType.Admin,
            IsActive = true,
            EmailConfirmed = true // Admin-created users don't need email confirmation
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Admin user creation failed: {Errors}", errors);
            return Result<AdminUserDetailDto>.Failure($"Failed to create user: {errors}", "USER_CREATION_FAILED");
        }

        // Assign roles
        if (dto.Roles.Any())
        {
            var roleResult = await _userManager.AddToRolesAsync(user, dto.Roles);
            if (!roleResult.Succeeded)
            {
                // Rollback user creation
                await _userManager.DeleteAsync(user);
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                return Result<AdminUserDetailDto>.Failure($"Failed to assign roles: {errors}", "ROLE_ASSIGNMENT_FAILED");
            }
        }

        _logger.LogInformation("Admin user {Email} created by {CreatedBy}", dto.Email, createdBy);

        // TODO: Send welcome email if dto.SendWelcomeEmail is true

        return await GetByIdAsync(user.Id, cancellationToken);
    }

    public async Task<Result<AdminUserDetailDto>> UpdateAsync(
        Guid id,
        UpdateAdminUserDto dto,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.UserType == UserType.Admin, cancellationToken);

        if (user is null)
        {
            return Result<AdminUserDetailDto>.Failure("Admin user not found", "USER_NOT_FOUND");
        }

        // Update basic properties
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.PhoneNumber = dto.PhoneNumber;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            return Result<AdminUserDetailDto>.Failure($"Failed to update user: {errors}", "USER_UPDATE_FAILED");
        }

        // Update roles if provided
        if (dto.Roles is not null)
        {
            // Validate roles
            var invalidRoles = dto.Roles.Where(r => !AdminAssignableRoles.Contains(r, StringComparer.OrdinalIgnoreCase)).ToList();
            if (invalidRoles.Any())
            {
                return Result<AdminUserDetailDto>.Failure(
                    $"Invalid roles: {string.Join(", ", invalidRoles)}. Only Admin and SalesRep roles are allowed.",
                    "INVALID_ROLES");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            // Remove old admin-assignable roles
            var rolesToRemove = currentRoles.Where(r => AdminAssignableRoles.Contains(r, StringComparer.OrdinalIgnoreCase)).ToList();
            if (rolesToRemove.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            }

            // Add new roles
            if (dto.Roles.Any())
            {
                await _userManager.AddToRolesAsync(user, dto.Roles);
            }
        }

        _logger.LogInformation("Admin user {UserId} updated by {UpdatedBy}", id, updatedBy);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<Result> ActivateAsync(
        Guid id,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.UserType == UserType.Admin, cancellationToken);

        if (user is null)
        {
            return Result.Failure("Admin user not found", "USER_NOT_FOUND");
        }

        if (user.IsActive)
        {
            return Result.Failure("User is already active", "ALREADY_ACTIVE");
        }

        user.IsActive = true;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Admin user {UserId} activated by {UpdatedBy}", id, updatedBy);

        return Result.Success();
    }

    public async Task<Result> DeactivateAsync(
        Guid id,
        Guid currentUserId,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        // Prevent self-deactivation
        if (id == currentUserId)
        {
            return Result.Failure("Cannot deactivate your own account", "SELF_ACTION_FORBIDDEN");
        }

        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.UserType == UserType.Admin, cancellationToken);

        if (user is null)
        {
            return Result.Failure("Admin user not found", "USER_NOT_FOUND");
        }

        if (!user.IsActive)
        {
            return Result.Failure("User is already inactive", "ALREADY_INACTIVE");
        }

        user.IsActive = false;
        // Invalidate refresh token
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;

        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Admin user {UserId} deactivated by {UpdatedBy}", id, updatedBy);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(
        Guid id,
        Guid currentUserId,
        string deletedBy,
        CancellationToken cancellationToken = default)
    {
        // Prevent self-deletion
        if (id == currentUserId)
        {
            return Result.Failure("Cannot delete your own account", "SELF_ACTION_FORBIDDEN");
        }

        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.UserType == UserType.Admin, cancellationToken);

        if (user is null)
        {
            return Result.Failure("Admin user not found", "USER_NOT_FOUND");
        }

        var deleteResult = await _userManager.DeleteAsync(user);
        if (!deleteResult.Succeeded)
        {
            var errors = string.Join(", ", deleteResult.Errors.Select(e => e.Description));
            return Result.Failure($"Failed to delete user: {errors}", "USER_DELETION_FAILED");
        }

        _logger.LogInformation("Admin user {UserId} deleted by {DeletedBy}", id, deletedBy);

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(
        Guid id,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.UserType == UserType.Admin, cancellationToken);

        if (user is null)
        {
            return Result.Failure("Admin user not found", "USER_NOT_FOUND");
        }

        // Generate a password reset token and new temporary password
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var newPassword = GenerateTemporaryPassword();

        var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
        if (!resetResult.Succeeded)
        {
            var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
            return Result.Failure($"Failed to reset password: {errors}", "PASSWORD_RESET_FAILED");
        }

        // Invalidate refresh token to force re-login
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Password reset for admin user {UserId} by {UpdatedBy}", id, updatedBy);

        // TODO: Send email with new temporary password

        return Result.Success();
    }

    public async Task<Result<List<AvailableRoleDto>>> GetAvailableRolesAsync(
        CancellationToken cancellationToken = default)
    {
        // Get all roles except Customer (which is not assignable to admin users)
        var roles = await _roleManager.Roles
            .Where(r => r.Name != "Customer")
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        var dtos = roles.Select(r => new AvailableRoleDto
        {
            Name = r.Name ?? string.Empty,
            Description = r.Description
        }).ToList();

        return Result<List<AvailableRoleDto>>.Success(dtos);
    }

    public async Task<Result<List<string>>> GetUserRolesAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.UserType == UserType.Admin, cancellationToken);

        if (user is null)
        {
            return Result<List<string>>.Failure("Admin user not found", "USER_NOT_FOUND");
        }

        var roles = await _userManager.GetRolesAsync(user);
        return Result<List<string>>.Success(roles.ToList());
    }

    public async Task<Result> SetUserRolesAsync(
        Guid id,
        SetUserRolesDto dto,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.UserType == UserType.Admin, cancellationToken);

        if (user is null)
        {
            return Result.Failure("Admin user not found", "USER_NOT_FOUND");
        }

        // Validate roles (exclude Customer role)
        var invalidRoles = dto.Roles
            .Where(r => r.Equals("Customer", StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (invalidRoles.Any())
        {
            return Result.Failure("Customer role cannot be assigned to admin users", "INVALID_ROLES");
        }

        // Verify all roles exist
        foreach (var roleName in dto.Roles)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                return Result.Failure($"Role '{roleName}' does not exist", "ROLE_NOT_FOUND");
            }
        }

        // Get current roles and replace them
        var currentRoles = await _userManager.GetRolesAsync(user);

        // Remove all current roles (except Customer if somehow present)
        var rolesToRemove = currentRoles.Where(r => !r.Equals("Customer", StringComparison.OrdinalIgnoreCase)).ToList();
        if (rolesToRemove.Any())
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                return Result.Failure($"Failed to remove roles: {errors}", "ROLE_REMOVAL_FAILED");
            }
        }

        // Add new roles
        if (dto.Roles.Any())
        {
            var addResult = await _userManager.AddToRolesAsync(user, dto.Roles);
            if (!addResult.Succeeded)
            {
                var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                return Result.Failure($"Failed to add roles: {errors}", "ROLE_ASSIGNMENT_FAILED");
            }
        }

        _logger.LogInformation("Roles for admin user {UserId} updated by {UpdatedBy}", id, updatedBy);
        return Result.Success();
    }

    public async Task<Result<List<UserLoginDto>>> GetUserLoginsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.UserType == UserType.Admin, cancellationToken);

        if (user is null)
        {
            return Result<List<UserLoginDto>>.Failure("Admin user not found", "USER_NOT_FOUND");
        }

        var logins = await _userManager.GetLoginsAsync(user);
        var dtos = logins.Select(l => new UserLoginDto
        {
            Id = $"{l.LoginProvider}_{l.ProviderKey}",
            LoginProvider = l.LoginProvider,
            ProviderDisplayName = l.ProviderDisplayName ?? l.LoginProvider,
            ProviderKey = l.ProviderKey
        }).ToList();

        return Result<List<UserLoginDto>>.Success(dtos);
    }

    public async Task<Result<List<UserClaimDto>>> GetUserClaimsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.UserType == UserType.Admin, cancellationToken);

        if (user is null)
        {
            return Result<List<UserClaimDto>>.Failure("Admin user not found", "USER_NOT_FOUND");
        }

        // Get claims from database with IDs
        var claims = await _dbContext.Set<IdentityUserClaim<Guid>>()
            .Where(c => c.UserId == id)
            .OrderBy(c => c.ClaimType)
            .ThenBy(c => c.ClaimValue)
            .ToListAsync(cancellationToken);

        var dtos = claims.Select(c => new UserClaimDto
        {
            Id = c.Id,
            Type = c.ClaimType ?? string.Empty,
            Value = c.ClaimValue ?? string.Empty
        }).ToList();

        return Result<List<UserClaimDto>>.Success(dtos);
    }

    public async Task<Result> AddUserClaimAsync(
        Guid id,
        AddUserClaimDto dto,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.UserType == UserType.Admin, cancellationToken);

        if (user is null)
        {
            return Result.Failure("Admin user not found", "USER_NOT_FOUND");
        }

        if (string.IsNullOrWhiteSpace(dto.Type) || string.IsNullOrWhiteSpace(dto.Value))
        {
            return Result.Failure("Claim type and value are required", "INVALID_CLAIM");
        }

        // Check if claim already exists
        var existingClaims = await _userManager.GetClaimsAsync(user);
        if (existingClaims.Any(c => c.Type == dto.Type && c.Value == dto.Value))
        {
            return Result.Failure("Claim already exists for this user", "CLAIM_EXISTS");
        }

        var claim = new Claim(dto.Type, dto.Value);
        var result = await _userManager.AddClaimAsync(user, claim);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure($"Failed to add claim: {errors}", "CLAIM_ADD_FAILED");
        }

        _logger.LogInformation("Claim {ClaimType}:{ClaimValue} added to admin user {UserId} by {UpdatedBy}",
            dto.Type, dto.Value, id, updatedBy);

        return Result.Success();
    }

    public async Task<Result> RemoveUserClaimAsync(
        Guid id,
        int claimId,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.UserType == UserType.Admin, cancellationToken);

        if (user is null)
        {
            return Result.Failure("Admin user not found", "USER_NOT_FOUND");
        }

        // Find the claim by ID
        var claimEntity = await _dbContext.Set<IdentityUserClaim<Guid>>()
            .FirstOrDefaultAsync(c => c.Id == claimId && c.UserId == id, cancellationToken);

        if (claimEntity is null)
        {
            return Result.Failure("Claim not found", "CLAIM_NOT_FOUND");
        }

        var claim = new Claim(claimEntity.ClaimType ?? string.Empty, claimEntity.ClaimValue ?? string.Empty);
        var result = await _userManager.RemoveClaimAsync(user, claim);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure($"Failed to remove claim: {errors}", "CLAIM_REMOVE_FAILED");
        }

        _logger.LogInformation("Claim {ClaimId} removed from admin user {UserId} by {UpdatedBy}",
            claimId, id, updatedBy);

        return Result.Success();
    }

    private static string GenerateTemporaryPassword()
    {
        // Generate a secure temporary password
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string special = "!@#$%^&*";

        var random = RandomNumberGenerator.Create();
        var password = new char[12];
        var allChars = uppercase + lowercase + digits + special;

        // Ensure at least one character from each category
        password[0] = uppercase[GetRandomIndex(random, uppercase.Length)];
        password[1] = lowercase[GetRandomIndex(random, lowercase.Length)];
        password[2] = digits[GetRandomIndex(random, digits.Length)];
        password[3] = special[GetRandomIndex(random, special.Length)];

        // Fill the rest randomly
        for (int i = 4; i < password.Length; i++)
        {
            password[i] = allChars[GetRandomIndex(random, allChars.Length)];
        }

        // Shuffle the password
        for (int i = password.Length - 1; i > 0; i--)
        {
            int j = GetRandomIndex(random, i + 1);
            (password[i], password[j]) = (password[j], password[i]);
        }

        return new string(password);
    }

    private static int GetRandomIndex(RandomNumberGenerator rng, int max)
    {
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        return (int)(BitConverter.ToUInt32(bytes, 0) % (uint)max);
    }
}
