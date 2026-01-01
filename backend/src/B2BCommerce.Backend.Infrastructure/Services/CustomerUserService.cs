using System.Security.Cryptography;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.CustomerUsers;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Application.Interfaces.Services;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Infrastructure.Services;

/// <summary>
/// Service implementation for managing customer (dealer) users
/// </summary>
public class CustomerUserService : ICustomerUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CustomerUserService> _logger;

    public CustomerUserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IUnitOfWork unitOfWork,
        ILogger<CustomerUserService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PagedResult<CustomerUserListDto>>> GetUsersAsync(
        Guid customerId,
        int page = 1,
        int pageSize = 10,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        // Verify customer exists
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
        {
            return Result<PagedResult<CustomerUserListDto>>.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        // Get all users for this customer
        var query = _userManager.Users
            .Where(u => u.CustomerId == customerId && u.UserType == UserType.Customer);

        // Apply search filter
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

        // Get customer roles for role lookup
        var customerRoles = await _roleManager.Roles
            .Where(r => r.UserType == UserType.Customer)
            .ToDictionaryAsync(r => r.Name ?? string.Empty, r => r, cancellationToken);

        // Map to DTOs with roles
        var userDtos = new List<CustomerUserListDto>();
        foreach (var user in users)
        {
            var roleNames = await _userManager.GetRolesAsync(user);
            var roles = roleNames
                .Where(rn => customerRoles.ContainsKey(rn))
                .Select(rn => new CustomerUserRoleDto
                {
                    Id = customerRoles[rn].Id,
                    Name = rn,
                    Description = customerRoles[rn].Description
                })
                .ToList();

            userDtos.Add(new CustomerUserListDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Roles = roles,
                IsActive = user.IsActive,
                LastLoginAt = user.LastLoginAt,
                CreatedAt = user.CreatedAt
            });
        }

        var pagedResult = new PagedResult<CustomerUserListDto>(userDtos, totalCount, page, pageSize);
        return Result<PagedResult<CustomerUserListDto>>.Success(pagedResult);
    }

    public async Task<Result<CustomerUserDetailDto>> GetUserByIdAsync(
        Guid customerId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Get user and verify they belong to the customer
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.CustomerId == customerId && u.UserType == UserType.Customer, cancellationToken);

        if (user is null)
        {
            return Result<CustomerUserDetailDto>.Failure("Customer user not found", "USER_NOT_FOUND");
        }

        // Get customer for title
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId, cancellationToken);

        return Result<CustomerUserDetailDto>.Success(await MapToDetailDtoAsync(user, customer?.Title ?? string.Empty, cancellationToken));
    }

    public async Task<Result<CustomerUserDetailDto>> CreateUserAsync(
        Guid customerId,
        CreateCustomerUserDto dto,
        CancellationToken cancellationToken = default)
    {
        // Verify customer exists
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
        {
            return Result<CustomerUserDetailDto>.Failure("Customer not found", "CUSTOMER_NOT_FOUND");
        }

        // Check if email already exists
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser is not null)
        {
            return Result<CustomerUserDetailDto>.Failure("Email already registered", "EMAIL_EXISTS");
        }

        // Validate roles - only allow customer roles
        var customerRoles = await _roleManager.Roles
            .Where(r => r.UserType == UserType.Customer)
            .ToDictionaryAsync(r => r.Id, r => r, cancellationToken);

        var invalidRoleIds = dto.RoleIds.Where(id => !customerRoles.ContainsKey(id)).ToList();
        if (invalidRoleIds.Any())
        {
            return Result<CustomerUserDetailDto>.Failure(
                $"Invalid role IDs: {string.Join(", ", invalidRoleIds)}. Only customer roles are allowed.",
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
            UserType = UserType.Customer,
            CustomerId = customerId,
            IsActive = true,
            EmailConfirmed = true // Admin-created users don't need email confirmation
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Customer user creation failed: {Errors}", errors);
            return Result<CustomerUserDetailDto>.Failure($"Failed to create user: {errors}", "USER_CREATION_FAILED");
        }

        // Assign roles
        if (dto.RoleIds.Any())
        {
            var roleNames = dto.RoleIds
                .Where(id => customerRoles.ContainsKey(id))
                .Select(id => customerRoles[id].Name!)
                .ToList();

            var roleResult = await _userManager.AddToRolesAsync(user, roleNames);
            if (!roleResult.Succeeded)
            {
                // Rollback user creation
                await _userManager.DeleteAsync(user);
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                return Result<CustomerUserDetailDto>.Failure($"Failed to assign roles: {errors}", "ROLE_ASSIGNMENT_FAILED");
            }
        }

        _logger.LogInformation("Customer user {Email} created for customer {CustomerId}", dto.Email, customerId);

        // TODO: Send welcome email if dto.SendWelcomeEmail is true

        return Result<CustomerUserDetailDto>.Success(await MapToDetailDtoAsync(user, customer.Title, cancellationToken));
    }

    public async Task<Result<CustomerUserDetailDto>> UpdateUserAsync(
        Guid customerId,
        Guid userId,
        UpdateCustomerUserDto dto,
        CancellationToken cancellationToken = default)
    {
        // Get user and verify they belong to the customer
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.CustomerId == customerId && u.UserType == UserType.Customer, cancellationToken);

        if (user is null)
        {
            return Result<CustomerUserDetailDto>.Failure("Customer user not found", "USER_NOT_FOUND");
        }

        // Get customer for response
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId, cancellationToken);

        // Update basic properties
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.PhoneNumber = dto.PhoneNumber;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            return Result<CustomerUserDetailDto>.Failure($"Failed to update user: {errors}", "USER_UPDATE_FAILED");
        }

        _logger.LogInformation("Customer user {UserId} updated for customer {CustomerId}", userId, customerId);

        return Result<CustomerUserDetailDto>.Success(await MapToDetailDtoAsync(user, customer?.Title ?? string.Empty, cancellationToken));
    }

    public async Task<Result> ActivateUserAsync(
        Guid customerId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Get user and verify they belong to the customer
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.CustomerId == customerId && u.UserType == UserType.Customer, cancellationToken);

        if (user is null)
        {
            return Result.Failure("Customer user not found", "USER_NOT_FOUND");
        }

        if (user.IsActive)
        {
            return Result.Failure("User is already active", "ALREADY_ACTIVE");
        }

        user.IsActive = true;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Customer user {UserId} activated for customer {CustomerId}", userId, customerId);

        return Result.Success();
    }

    public async Task<Result> DeactivateUserAsync(
        Guid customerId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Get user and verify they belong to the customer
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.CustomerId == customerId && u.UserType == UserType.Customer, cancellationToken);

        if (user is null)
        {
            return Result.Failure("Customer user not found", "USER_NOT_FOUND");
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

        _logger.LogInformation("Customer user {UserId} deactivated for customer {CustomerId}", userId, customerId);

        return Result.Success();
    }

    public async Task<Result> SetUserRolesAsync(
        Guid customerId,
        Guid userId,
        SetCustomerUserRolesDto dto,
        CancellationToken cancellationToken = default)
    {
        // Get user and verify they belong to the customer
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.CustomerId == customerId && u.UserType == UserType.Customer, cancellationToken);

        if (user is null)
        {
            return Result.Failure("Customer user not found", "USER_NOT_FOUND");
        }

        // Get customer roles
        var customerRoles = await _roleManager.Roles
            .Where(r => r.UserType == UserType.Customer)
            .ToDictionaryAsync(r => r.Id, r => r, cancellationToken);

        // Validate all role IDs are customer roles
        var invalidRoleIds = dto.RoleIds.Where(id => !customerRoles.ContainsKey(id)).ToList();
        if (invalidRoleIds.Any())
        {
            return Result.Failure(
                $"Invalid role IDs: {string.Join(", ", invalidRoleIds)}. Only customer roles are allowed.",
                "INVALID_ROLES");
        }

        // Get current roles
        var currentRoleNames = await _userManager.GetRolesAsync(user);

        // Remove all current customer roles
        var customerRoleNames = customerRoles.Values.Select(r => r.Name!).ToHashSet();
        var rolesToRemove = currentRoleNames.Where(r => customerRoleNames.Contains(r)).ToList();
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
        if (dto.RoleIds.Any())
        {
            var newRoleNames = dto.RoleIds
                .Where(id => customerRoles.ContainsKey(id))
                .Select(id => customerRoles[id].Name!)
                .ToList();

            var addResult = await _userManager.AddToRolesAsync(user, newRoleNames);
            if (!addResult.Succeeded)
            {
                var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                return Result.Failure($"Failed to add roles: {errors}", "ROLE_ASSIGNMENT_FAILED");
            }
        }

        _logger.LogInformation("Roles for customer user {UserId} updated for customer {CustomerId}", userId, customerId);
        return Result.Success();
    }

    public async Task<Result<List<CustomerUserRoleDto>>> GetAvailableRolesAsync(
        CancellationToken cancellationToken = default)
    {
        // Get all customer roles (UserType.Customer)
        var roles = await _roleManager.Roles
            .Where(r => r.UserType == UserType.Customer)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        var dtos = roles.Select(r => new CustomerUserRoleDto
        {
            Id = r.Id,
            Name = r.Name ?? string.Empty,
            Description = r.Description
        }).ToList();

        return Result<List<CustomerUserRoleDto>>.Success(dtos);
    }

    private async Task<CustomerUserDetailDto> MapToDetailDtoAsync(
        ApplicationUser user,
        string customerTitle,
        CancellationToken cancellationToken)
    {
        // Get customer roles for role lookup
        var customerRoles = await _roleManager.Roles
            .Where(r => r.UserType == UserType.Customer)
            .ToDictionaryAsync(r => r.Name ?? string.Empty, r => r, cancellationToken);

        var roleNames = await _userManager.GetRolesAsync(user);
        var roles = roleNames
            .Where(rn => customerRoles.ContainsKey(rn))
            .Select(rn => new CustomerUserRoleDto
            {
                Id = customerRoles[rn].Id,
                Name = rn,
                Description = customerRoles[rn].Description
            })
            .ToList();

        return new CustomerUserDetailDto
        {
            Id = user.Id,
            CustomerId = user.CustomerId ?? Guid.Empty,
            CustomerTitle = customerTitle,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = $"{user.FirstName} {user.LastName}".Trim(),
            PhoneNumber = user.PhoneNumber,
            Roles = roles,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        };
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
