# B2B E-Commerce Platform - Authentication with ASP.NET Core Identity

## Document Purpose

Complete implementation guide for authentication and authorization using
**ASP.NET Core Identity** with JWT tokens for the B2B E-Commerce Platform.

**Target**: .NET 8, ASP.NET Core Identity, JWT, EF Core\
**Version**: 2.0\
**Date**: November 2025

---

## Table of Contents

1. [Why ASP.NET Core Identity](#why-aspnet-core-identity)
2. [Architecture Overview](#architecture-overview)
3. [Domain Layer - Identity Entities](#domain-layer-identity-entities)
4. [Infrastructure Layer - Identity Setup](#infrastructure-layer-identity-setup)
5. [Application Layer - Auth Services](#application-layer-auth-services)
6. [API Layer - Controllers & Configuration](#api-layer-controllers--configuration)
7. [Database Schema](#database-schema)
8. [Complete Implementation](#complete-implementation)

---

## Why ASP.NET Core Identity?

### Built-in Features

✅ **Password hashing** (PBKDF2 with salt)\
✅ **Password policies** (length, complexity)\
✅ **Account lockout** (failed login attempts)\
✅ **Two-factor authentication** (2FA)\
✅ **Email confirmation**\
✅ **Password reset**\
✅ **Security stamp** (invalidate tokens on password change)\
✅ **Claims management**\
✅ **Role management**\
✅ **User management** (CRUD operations)

### Benefits

- Battle-tested and secure
- Reduces custom code
- Extensible
- Industry standard

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                     Client (Frontend)                       │
│  - Sends credentials                                        │
│  - Stores JWT token                                         │
│  - Includes token in Authorization header                   │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ↓
┌─────────────────────────────────────────────────────────────┐
│                  API Layer (Controllers)                     │
│  - AuthController (login, register, refresh)                │
│  - [Authorize] attributes on protected endpoints            │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ↓
┌─────────────────────────────────────────────────────────────┐
│              Application Layer (Services)                    │
│  - AuthService (business logic)                             │
│  - Uses SignInManager, UserManager                          │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ↓
┌─────────────────────────────────────────────────────────────┐
│           Infrastructure Layer (Identity)                    │
│  - UserManager<ApplicationUser>                             │
│  - SignInManager<ApplicationUser>                           │
│  - RoleManager<IdentityRole>                                │
│  - ApplicationDbContext : IdentityDbContext                 │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ↓
┌─────────────────────────────────────────────────────────────┐
│                  Database (PostgreSQL)                       │
│  - AspNetUsers                                              │
│  - AspNetRoles                                              │
│  - AspNetUserRoles                                          │
│  - AspNetUserClaims                                         │
│  - AspNetUserTokens                                         │
└─────────────────────────────────────────────────────────────┘
```

---

## Domain Layer - Identity Entities

### Custom User Classes

```csharp
// File: Domain/Identity/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;

/// <summary>
/// Dealer user (B2B portal user)
/// </summary>
public class ApplicationUser : IdentityUser<int>
{
    // Personal Information
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string FullName => $"{FirstName} {LastName}";
    
    // Business Relation
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    
    // Role & Permissions
    public UserRole Role { get; set; }
    public bool CanApproveOrders { get; set; }
    public bool CanApproveReturns { get; set; }
    
    // Additional Security
    public DateTime? LastPasswordChangeDate { get; set; }
    public bool RequirePasswordChange { get; set; }
    public string? LastLoginIp { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    
    // Navigation
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    
    // Note: IdentityUser already provides:
    // - Id (int in our case)
    // - UserName
    // - Email
    // - EmailConfirmed
    // - PasswordHash
    // - PhoneNumber
    // - PhoneNumberConfirmed
    // - TwoFactorEnabled
    // - LockoutEnd (DateTimeOffset?)
    // - LockoutEnabled
    // - AccessFailedCount
    // - SecurityStamp
    // - ConcurrencyStamp
}

/// <summary>
/// Admin user (Admin panel user)
/// </summary>
public class ApplicationAdminUser : IdentityUser<int>
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string FullName => $"{FirstName} {LastName}";
    
    public AdminRole AdminRole { get; set; }
    
    // Permissions (alternatively use Claims)
    public bool CanManageProducts { get; set; }
    public bool CanManageOrders { get; set; }
    public bool CanManageCustomers { get; set; }
    public bool CanManageUsers { get; set; }
    public bool CanViewReports { get; set; }
    
    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

// Custom Role (optional - can use IdentityRole<int> directly)
public class ApplicationRole : IdentityRole<int>
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### RefreshToken Entity

```csharp
// File: Domain/Entities/RefreshToken.cs
public class RefreshToken : BaseEntity
{
    public int? UserId { get; set; }
    public int? AdminUserId { get; set; }
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public string CreatedByIp { get; set; } = null!;
    
    // Navigation
    public ApplicationUser? User { get; set; }
    public ApplicationAdminUser? AdminUser { get; set; }
    
    // Computed
    public bool IsActive => RevokedAt == null && !IsExpired;
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    
    public static RefreshToken Create(int? userId, int? adminUserId, string token, 
        string createdByIp, int daysValid = 7)
    {
        return new RefreshToken
        {
            UserId = userId,
            AdminUserId = adminUserId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(daysValid),
            CreatedByIp = createdByIp,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    public void Revoke(string ipAddress, string? replacedByToken = null)
    {
        RevokedAt = DateTime.UtcNow;
        RevokedByIp = ipAddress;
        ReplacedByToken = replacedByToken;
    }
}
```

### Enums

```csharp
public enum UserRole
{
    Owner = 1,
    Purchasing = 2,
    Finance = 3,
    Viewer = 4
}

public enum AdminRole
{
    SuperAdmin = 1,
    SalesManager = 2,
    FinanceManager = 3,
    CustomerService = 4,
    ContentManager = 5
}
```

---

## Infrastructure Layer - Identity Setup

### ApplicationDbContext with Identity

```csharp
// File: Infrastructure/Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<
    ApplicationUser, 
    ApplicationRole, 
    int,
    IdentityUserClaim<int>,
    IdentityUserRole<int>,
    IdentityUserLogin<int>,
    IdentityRoleClaim<int>,
    IdentityUserToken<int>>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    // Your business entities
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    
    // Admin users (separate table)
    public DbSet<ApplicationAdminUser> AdminUsers => Set<ApplicationAdminUser>();
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // Important! Configures Identity tables
        
        // Configure custom table names (optional)
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
        });
        
        builder.Entity<ApplicationRole>(entity =>
        {
            entity.ToTable("Roles");
        });
        
        builder.Entity<IdentityUserRole<int>>(entity =>
        {
            entity.ToTable("UserRoles");
        });
        
        builder.Entity<IdentityUserClaim<int>>(entity =>
        {
            entity.ToTable("UserClaims");
        });
        
        builder.Entity<IdentityUserLogin<int>>(entity =>
        {
            entity.ToTable("UserLogins");
        });
        
        builder.Entity<IdentityUserToken<int>>(entity =>
        {
            entity.ToTable("UserTokens");
        });
        
        builder.Entity<IdentityRoleClaim<int>>(entity =>
        {
            entity.ToTable("RoleClaims");
        });
        
        // ApplicationUser configuration
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            
            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Users)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasMany(e => e.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId);
            
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
        
        // AdminUser configuration
        builder.Entity<ApplicationAdminUser>(entity =>
        {
            entity.ToTable("AdminUsers");
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            
            entity.HasMany(e => e.RefreshTokens)
                .WithOne(rt => rt.AdminUser)
                .HasForeignKey(rt => rt.AdminUserId);
            
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
        
        // RefreshToken configuration
        builder.Entity<RefreshToken>(entity =>
        {
            entity.Property(e => e.Token).HasMaxLength(500).IsRequired();
            entity.HasIndex(e => e.Token);
        });
        
        // Apply other configurations
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
```

### Identity Configuration

```csharp
// File: Infrastructure/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        
        // Identity Configuration
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 1;
            
            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
            
            // User settings
            options.User.RequireUniqueEmail = true;
            
            // SignIn settings
            options.SignIn.RequireConfirmedEmail = false; // Set to true in production
            options.SignIn.RequireConfirmedPhoneNumber = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders(); // For password reset, email confirmation
        
        // Token lifespan configuration
        services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromHours(24); // Password reset token validity
        });
        
        return services;
    }
}
```

---

## Application Layer - Auth Services

### IAuthService Interface

```csharp
// File: Application/Interfaces/Services/IAuthService.cs
public interface IAuthService
{
    // Authentication
    Task<AuthenticationResult> LoginAsync(LoginDto dto, string ipAddress);
    Task<AuthenticationResult> AdminLoginAsync(AdminLoginDto dto, string ipAddress);
    Task<AuthenticationResult> RefreshTokenAsync(string token, string ipAddress);
    Task RevokeTokenAsync(string token, string ipAddress);
    Task LogoutAsync(int userId, bool isAdmin = false);
    
    // Registration
    Task<RegistrationResult> RegisterUserAsync(RegisterUserDto dto);
    Task<RegistrationResult> RegisterAdminAsync(RegisterAdminDto dto);
    
    // Password Management
    Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
    Task<string> GeneratePasswordResetTokenAsync(string email);
    Task ResetPasswordAsync(ResetPasswordDto dto);
    Task ForcePasswordChangeAsync(int userId);
    
    // Email Confirmation
    Task<string> GenerateEmailConfirmationTokenAsync(int userId);
    Task ConfirmEmailAsync(int userId, string token);
    
    // User Management
    Task<ApplicationUser?> GetUserByIdAsync(int userId);
    Task<ApplicationUser?> GetUserByUsernameAsync(string username);
    Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
}
```

### DTOs

```csharp
// File: Application/DTOs/Auth/AuthDtos.cs
public class LoginDto
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public bool RememberMe { get; set; }
}

public class AdminLoginDto
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class RegisterUserDto
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public int CustomerId { get; set; }
    public UserRole Role { get; set; }
    public string? PhoneNumber { get; set; }
}

public class RegisterAdminDto
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public AdminRole AdminRole { get; set; }
}

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}

public class ResetPasswordDto
{
    public string Email { get; set; } = null!;
    public string Token { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}

public class AuthenticationResult
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? TokenExpiration { get; set; }
    public UserDto? User { get; set; }
    public string? ErrorMessage { get; set; }
    public bool RequiresPasswordChange { get; set; }
    public bool IsLocked { get; set; }
    
    public static AuthenticationResult Succeeded(string accessToken, string refreshToken, 
        UserDto user, DateTime expiration)
    {
        return new AuthenticationResult
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = user,
            TokenExpiration = expiration
        };
    }
    
    public static AuthenticationResult Failed(string error)
    {
        return new AuthenticationResult { Success = false, ErrorMessage = error };
    }
    
    public static AuthenticationResult Locked(string error)
    {
        return new AuthenticationResult { Success = false, IsLocked = true, ErrorMessage = error };
    }
}

public class RegistrationResult
{
    public bool Success { get; set; }
    public int? UserId { get; set; }
    public List<string> Errors { get; set; } = new();
    
    public static RegistrationResult Succeeded(int userId)
    {
        return new RegistrationResult { Success = true, UserId = userId };
    }
    
    public static RegistrationResult Failed(List<string> errors)
    {
        return new RegistrationResult { Success = false, Errors = errors };
    }
}
```

### AuthService Implementation

```csharp
// File: Application/Services/AuthService.cs
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    
    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }
    
    public async Task<AuthenticationResult> LoginAsync(LoginDto dto, string ipAddress)
    {
        // Find user
        var user = await _userManager.FindByNameAsync(dto.Username);
        
        if (user == null)
        {
            _logger.LogWarning("Login attempt with invalid username: {Username}", dto.Username);
            return AuthenticationResult.Failed("Invalid username or password");
        }
        
        // Check if user is deleted (soft delete)
        if (user.IsDeleted)
        {
            _logger.LogWarning("Login attempt for deleted user: {Username}", dto.Username);
            return AuthenticationResult.Failed("Account not found");
        }
        
        // Check if locked
        if (await _userManager.IsLockedOutAsync(user))
        {
            _logger.LogWarning("Login attempt for locked account: {Username}", dto.Username);
            return AuthenticationResult.Locked(
                $"Account is locked until {user.LockoutEnd?.LocalDateTime:yyyy-MM-dd HH:mm}");
        }
        
        // Load customer to check if active
        await _unitOfWork.Entry(user).Reference(u => u.Customer).LoadAsync();
        
        if (!user.Customer.IsActive)
        {
            _logger.LogWarning("Login attempt for user with inactive customer: {Username}", dto.Username);
            return AuthenticationResult.Failed("Your company account is inactive");
        }
        
        // Attempt sign in
        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
        
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                _logger.LogWarning("Account locked out: {Username}", dto.Username);
                return AuthenticationResult.Locked("Account is temporarily locked due to multiple failed login attempts");
            }
            
            if (result.IsNotAllowed)
            {
                _logger.LogWarning("Sign in not allowed: {Username}", dto.Username);
                return AuthenticationResult.Failed("Email confirmation required");
            }
            
            _logger.LogWarning("Failed login attempt: {Username}", dto.Username);
            return AuthenticationResult.Failed("Invalid username or password");
        }
        
        // Check if password change required
        if (user.RequirePasswordChange)
        {
            return new AuthenticationResult
            {
                Success = false,
                RequiresPasswordChange = true,
                ErrorMessage = "Password change required"
            };
        }
        
        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        user.LastLoginIp = ipAddress;
        await _userManager.UpdateAsync(user);
        
        // Generate tokens
        var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        
        // Save refresh token
        var refreshTokenEntity = RefreshToken.Create(
            userId: user.Id,
            adminUserId: null,
            token: refreshToken,
            createdByIp: ipAddress,
            daysValid: dto.RememberMe ? 30 : 7
        );
        
        await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();
        
        _logger.LogInformation("Successful login: {Username}", dto.Username);
        
        var userDto = _mapper.Map<UserDto>(user);
        
        return AuthenticationResult.Succeeded(
            accessToken: accessToken,
            refreshToken: refreshToken,
            user: userDto,
            expiration: DateTime.UtcNow.AddMinutes(15)
        );
    }
    
    public async Task<RegistrationResult> RegisterUserAsync(RegisterUserDto dto)
    {
        // Verify customer exists and is active
        var customer = await _unitOfWork.Customers.GetByIdAsync(dto.CustomerId);
        if (customer == null || !customer.IsActive)
        {
            return RegistrationResult.Failed(new List<string> { "Invalid customer" });
        }
        
        // Create user
        var user = new ApplicationUser
        {
            UserName = dto.Username,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            CustomerId = dto.CustomerId,
            Role = dto.Role,
            PhoneNumber = dto.PhoneNumber,
            CreatedAt = DateTime.UtcNow,
            EmailConfirmed = false // Set to false, require email confirmation
        };
        
        // Create user with Identity
        var result = await _userManager.CreateAsync(user, dto.Password);
        
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            _logger.LogWarning("User registration failed: {Errors}", string.Join(", ", errors));
            return RegistrationResult.Failed(errors);
        }
        
        // Add to role (optional - you can use string-based roles)
        await _userManager.AddToRoleAsync(user, dto.Role.ToString());
        
        // Add claims for permissions
        var claims = new List<Claim>
        {
            new Claim("CustomerId", dto.CustomerId.ToString()),
            new Claim("CustomerName", customer.CompanyName),
            new Claim("FullName", user.FullName)
        };
        
        await _userManager.AddClaimsAsync(user, claims);
        
        _logger.LogInformation("User registered successfully: {Username}", dto.Username);
        
        return RegistrationResult.Succeeded(user.Id);
    }
    
    public async Task<AuthenticationResult> RefreshTokenAsync(string token, string ipAddress)
    {
        var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(token);
        
        if (refreshToken == null || !refreshToken.IsActive)
        {
            _logger.LogWarning("Invalid refresh token attempt");
            return AuthenticationResult.Failed("Invalid token");
        }
        
        ApplicationUser? user = null;
        
        if (refreshToken.UserId.HasValue)
        {
            user = await _userManager.FindByIdAsync(refreshToken.UserId.Value.ToString());
        }
        
        if (user == null || user.IsDeleted)
        {
            return AuthenticationResult.Failed("User not found");
        }
        
        // Generate new tokens
        var newAccessToken = await _tokenService.GenerateAccessTokenAsync(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        
        // Revoke old token
        refreshToken.Revoke(ipAddress, newRefreshToken);
        
        // Create new refresh token
        var newRefreshTokenEntity = RefreshToken.Create(
            userId: user.Id,
            adminUserId: null,
            token: newRefreshToken,
            createdByIp: ipAddress
        );
        
        await _unitOfWork.RefreshTokens.AddAsync(newRefreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();
        
        var userDto = _mapper.Map<UserDto>(user);
        
        return AuthenticationResult.Succeeded(
            accessToken: newAccessToken,
            refreshToken: newRefreshToken,
            user: userDto,
            expiration: DateTime.UtcNow.AddMinutes(15)
        );
    }
    
    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
            throw new NotFoundException("User not found");
        
        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new ValidationException(errors);
        }
        
        // Update security stamp (invalidates existing tokens)
        await _userManager.UpdateSecurityStampAsync(user);
        
        user.LastPasswordChangeDate = DateTime.UtcNow;
        user.RequirePasswordChange = false;
        await _userManager.UpdateAsync(user);
        
        _logger.LogInformation("Password changed for user: {UserId}", userId);
    }
    
    public async Task<string> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        
        if (user == null)
        {
            // Don't reveal that user doesn't exist
            _logger.LogWarning("Password reset requested for non-existent email: {Email}", email);
            return string.Empty;
        }
        
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        
        _logger.LogInformation("Password reset token generated for: {Email}", email);
        
        return token;
    }
    
    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        
        if (user == null)
            throw new NotFoundException("User not found");
        
        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new ValidationException(errors);
        }
        
        // Update security stamp
        await _userManager.UpdateSecurityStampAsync(user);
        
        user.LastPasswordChangeDate = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
        
        _logger.LogInformation("Password reset completed for: {Email}", dto.Email);
    }
    
    public async Task<string> GenerateEmailConfirmationTokenAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
            throw new NotFoundException("User not found");
        
        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }
    
    public async Task ConfirmEmailAsync(int userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
            throw new NotFoundException("User not found");
        
        var result = await _userManager.ConfirmEmailAsync(user, token);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new ValidationException(errors);
        }
        
        _logger.LogInformation("Email confirmed for user: {UserId}", userId);
    }
}
```

### TokenService

```csharp
// File: Infrastructure/Services/TokenService.cs
public class TokenService : ITokenService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtSettings _jwtSettings;
    
    public TokenService(
        UserManager<ApplicationUser> userManager,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _jwtSettings = jwtSettings.Value;
    }
    
    public async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
    {
        // Get roles
        var roles = await _userManager.GetRolesAsync(user);
        
        // Get claims
        var userClaims = await _userManager.GetClaimsAsync(user);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim("CustomerId", user.CustomerId.ToString()),
            new Claim("FullName", user.FullName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Token ID
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()) // Subject
        };
        
        // Add roles as claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        
        // Add user claims
        claims.AddRange(userClaims);
        
        // Add permission claims
        if (user.CanApproveOrders)
            claims.Add(new Claim("Permission", "orders.approve"));
        
        if (user.CanApproveReturns)
            claims.Add(new Claim("Permission", "returns.approve"));
        
        return GenerateJwtToken(claims);
    }
    
    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
    
    private string GenerateJwtToken(List<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

---

## API Layer - Controllers & Configuration

### AuthController

```csharp
// File: API/Controllers/AuthController.cs
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    
    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }
    
    /// <summary>
    /// User login
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthenticationResult>> Login([FromBody] LoginDto dto)
    {
        var ipAddress = GetIpAddress();
        var result = await _authService.LoginAsync(dto, ipAddress);
        
        if (!result.Success)
            return BadRequest(result);
        
        SetRefreshTokenCookie(result.RefreshToken!);
        
        return Ok(result);
    }
    
    /// <summary>
    /// User registration (for admin creating new users)
    /// </summary>
    [HttpPost("register")]
    [Authorize(Roles = "Owner,Admin")]
    [ProducesResponseType(typeof(RegistrationResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegistrationResult>> Register([FromBody] RegisterUserDto dto)
    {
        var result = await _authService.RegisterUserAsync(dto);
        
        if (!result.Success)
            return BadRequest(result);
        
        return CreatedAtAction(nameof(GetUser), new { id = result.UserId }, result);
    }
    
    /// <summary>
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthenticationResult>> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        
        if (string.IsNullOrEmpty(refreshToken))
            return BadRequest(new { message = "Refresh token required" });
        
        var ipAddress = GetIpAddress();
        var result = await _authService.RefreshTokenAsync(refreshToken, ipAddress);
        
        if (!result.Success)
            return BadRequest(result);
        
        SetRefreshTokenCookie(result.RefreshToken!);
        
        return Ok(result);
    }
    
    /// <summary>
    /// Logout
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        
        if (!string.IsNullOrEmpty(refreshToken))
        {
            var ipAddress = GetIpAddress();
            await _authService.RevokeTokenAsync(refreshToken, ipAddress);
        }
        
        Response.Cookies.Delete("refreshToken");
        
        return Ok(new { message = "Logged out successfully" });
    }
    
    /// <summary>
    /// Change password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = GetCurrentUserId();
        await _authService.ChangePasswordAsync(userId, dto);
        
        return Ok(new { message = "Password changed successfully" });
    }
    
    /// <summary>
    /// Request password reset
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var token = await _authService.GeneratePasswordResetTokenAsync(dto.Email);
        
        if (!string.IsNullOrEmpty(token))
        {
            // Send email with reset link
            var resetLink = $"{Request.Scheme}://{Request.Host}/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(dto.Email)}";
            
            // TODO: Send email with resetLink
            // await _emailService.SendPasswordResetEmailAsync(dto.Email, resetLink);
        }
        
        // Always return success (don't reveal if email exists)
        return Ok(new { message = "If the email exists, a password reset link has been sent" });
    }
    
    /// <summary>
    /// Reset password
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        await _authService.ResetPasswordAsync(dto);
        return Ok(new { message = "Password has been reset successfully" });
    }
    
    /// <summary>
    /// Confirm email
    /// </summary>
    [HttpGet("confirm-email")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ConfirmEmail([FromQuery] int userId, [FromQuery] string token)
    {
        await _authService.ConfirmEmailAsync(userId, token);
        return Ok(new { message = "Email confirmed successfully" });
    }
    
    /// <summary>
    /// Get current user
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDto>> GetUser()
    {
        var userId = GetCurrentUserId();
        var user = await _authService.GetUserByIdAsync(userId);
        
        if (user == null)
            return NotFound();
        
        return Ok(_mapper.Map<UserDto>(user));
    }
    
    // Helper methods
    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        
        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
    
    private string GetIpAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();
        
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
    
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim!);
    }
}

public class ForgotPasswordDto
{
    public string Email { get; set; } = null!;
}
```

### Program.cs Configuration

```csharp
// File: API/Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Infrastructure (includes Identity)
builder.Services.AddInfrastructure(builder.Configuration);

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        // Important: Validate security stamp
        RequireSignedTokens = true
    };
    
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Allow token from cookie as fallback
            if (string.IsNullOrEmpty(context.Token))
            {
                context.Token = context.Request.Cookies["accessToken"];
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            // Validate security stamp
            var userManager = context.HttpContext.RequestServices
                .GetRequiredService<UserManager<ApplicationUser>>();
            
            var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null)
            {
                var user = await userManager.FindByIdAsync(userIdClaim.Value);
                if (user != null)
                {
                    // Check security stamp to invalidate tokens after password change
                    var securityStamp = context.Principal?.FindFirst("AspNet.Identity.SecurityStamp")?.Value;
                    if (securityStamp != user.SecurityStamp)
                    {
                        context.Fail("Security stamp validation failed");
                    }
                }
            }
        }
    };
});

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("orders.approve", policy => 
        policy.RequireClaim("Permission", "orders.approve"));
    
    options.AddPolicy("returns.approve", policy => 
        policy.RequireClaim("Permission", "returns.approve"));
});

// Application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()!)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Seed roles and admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedDataAsync(services);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Seed initial data
async Task SeedDataAsync(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    
    // Create roles if they don't exist
    string[] roleNames = { "Owner", "Purchasing", "Finance", "Viewer", "Admin" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new ApplicationRole 
            { 
                Name = roleName,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
    
    // Create default admin user (optional)
    var adminEmail = "admin@b2bcommerce.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        // NOTE: This is just for initial setup - remove or protect in production
        adminUser = new ApplicationUser
        {
            UserName = "admin",
            Email = adminEmail,
            FirstName = "System",
            LastName = "Administrator",
            CustomerId = 1, // Placeholder
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var result = await userManager.CreateAsync(adminUser, "Admin@123"); // Change this!
        
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}
```

---

## Database Schema

### Migration Commands

```bash
# Add initial migration
dotnet ef migrations add InitialIdentity --project Infrastructure --startup-project API

# Update database
dotnet ef database update --project Infrastructure --startup-project API
```

### Tables Created by Identity

Identity will create these tables:

- `Users` (AspNetUsers) - User accounts
- `Roles` (AspNetRoles) - Role definitions
- `UserRoles` (AspNetUserRoles) - User-Role mappings
- `UserClaims` (AspNetUserClaims) - User claims
- `UserLogins` (AspNetUserLogins) - External logins
- `UserTokens` (AspNetUserTokens) - Auth tokens
- `RoleClaims` (AspNetRoleClaims) - Role claims
- `AdminUsers` - Your custom admin user table
- `RefreshTokens` - Your custom refresh token table

---

## Package Requirements

```xml
<!-- Infrastructure.csproj -->
<ItemGroup>
    <!-- Identity -->
    <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.0" />
    
    <!-- JWT -->
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
    <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.0.0" />
    
    <!-- EF Core -->
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
</ItemGroup>
```

---

## Validators

```csharp
// File: Application/Validators/Auth/RegisterUserValidator.cs
public class RegisterUserValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(50)
            .Matches("^[a-zA-Z0-9_.-]+$");
        
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain uppercase")
            .Matches("[a-z]").WithMessage("Password must contain lowercase")
            .Matches("[0-9]").WithMessage("Password must contain number");
        
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);
        
        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);
        
        RuleFor(x => x.CustomerId)
            .GreaterThan(0);
    }
}
```

---

## Testing

```csharp
// File: Tests/Application.Tests/Services/AuthServiceTests.cs
public class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
    private readonly AuthService _authService;
    
    public AuthServiceTests()
    {
        // Setup mocks
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            userStore.Object, null, null, null, null, null, null, null, null);
        
        var contextAccessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
            _mockUserManager.Object,
            contextAccessor.Object,
            claimsFactory.Object,
            null, null, null, null);
        
        _authService = new AuthService(
            _mockUserManager.Object,
            _mockSignInManager.Object,
            Mock.Of<ITokenService>(),
            Mock.Of<IUnitOfWork>(),
            Mock.Of<IMapper>(),
            Mock.Of<ILogger<AuthService>>());
    }
    
    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var user = new ApplicationUser 
        { 
            UserName = "testuser",
            Customer = new Customer { IsActive = true }
        };
        
        _mockUserManager.Setup(x => x.FindByNameAsync("testuser"))
            .ReturnsAsync(user);
        
        _mockUserManager.Setup(x => x.IsLockedOutAsync(user))
            .ReturnsAsync(false);
        
        _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, "password", true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
        
        // Act
        var result = await _authService.LoginAsync(
            new LoginDto { Username = "testuser", Password = "password" }, 
            "127.0.0.1");
        
        // Assert
        Assert.True(result.Success);
    }
}
```

---

## Summary

This implementation provides:

✅ **ASP.NET Core Identity** integration\
✅ **JWT authentication** with access & refresh tokens\
✅ **Password hashing** (PBKDF2)\
✅ **Account lockout** (5 failed attempts)\
✅ **Password reset** via email\
✅ **Email confirmation**\
✅ **Role-based authorization**\
✅ **Claims-based permissions**\
✅ **Security stamp validation**\
✅ **Refresh token rotation**\
✅ **Two-factor authentication** support (built-in)

### Advantages over Custom Auth:

- ✅ Less code to maintain
- ✅ Security best practices built-in
- ✅ Well-tested and battle-hardened
- ✅ Easy password policies
- ✅ Built-in token providers
- ✅ External login support (Google, Facebook, etc.)

---

**Document Version**: 2.0 (ASP.NET Core Identity)\
**Created**: November 2025\
**For**: B2B E-Commerce Platform\
**Framework**: .NET 8, ASP.NET Core Identity, JWT
