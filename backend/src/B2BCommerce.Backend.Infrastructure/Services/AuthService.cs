using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Auth;
using B2BCommerce.Backend.Application.DTOs.Customers;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Application.Interfaces.Services;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.ValueObjects;
using B2BCommerce.Backend.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace B2BCommerce.Backend.Infrastructure.Services;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            _logger.LogWarning("Login failed: User not found for email {Email}", request.Email);
            return Result<LoginResponseDto>.Failure("Invalid email or password", "INVALID_CREDENTIALS");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed: User account is inactive for email {Email}", request.Email);
            return Result<LoginResponseDto>.Failure("Account is inactive", "ACCOUNT_INACTIVE");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            _logger.LogWarning("Login failed: Invalid password for email {Email}", request.Email);
            return Result<LoginResponseDto>.Failure("Invalid email or password", "INVALID_CREDENTIALS");
        }

        // Get customer if linked
        Customer? customer = null;
        if (user.CustomerId.HasValue)
        {
            customer = await _unitOfWork.Customers.GetByIdAsync(user.CustomerId.Value, cancellationToken);
        }

        // Generate tokens
        var token = await GenerateJwtTokenAsync(user, customer);
        var refreshToken = GenerateRefreshToken();

        // Save refresh token
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(
            _configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 7));
        user.LastLoginAt = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);

        _logger.LogInformation("User {Email} logged in successfully", request.Email);

        return Result<LoginResponseDto>.Success(new LoginResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:ExpirationMinutes", 15)),
            CustomerId = customer?.Id ?? Guid.Empty,
            Email = user.Email!,
            CompanyName = customer?.CompanyName ?? string.Empty,
            IsApproved = customer?.IsApproved ?? true
        });
    }

    public async Task<Result<CustomerDto>> RegisterAsync(RegisterCustomerDto request, CancellationToken cancellationToken = default)
    {
        // Check if email already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return Result<CustomerDto>.Failure("Email already registered", "EMAIL_EXISTS");
        }

        // Check if tax number already exists
        var existingCustomer = await _unitOfWork.Customers.GetByTaxNumberAsync(request.TaxNumber, cancellationToken);
        if (existingCustomer is not null)
        {
            return Result<CustomerDto>.Failure("Tax number already registered", "TAX_NUMBER_EXISTS");
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // Parse customer type
            var customerType = CustomerType.Standard;
            if (!string.IsNullOrEmpty(request.Type) && Enum.TryParse<CustomerType>(request.Type, out var parsedType))
            {
                customerType = parsedType;
            }

            // Create customer
            var customer = new Customer(
                companyName: request.CompanyName,
                taxNumber: new TaxNumber(request.TaxNumber),
                email: new Email(request.Email),
                phone: new PhoneNumber(request.Phone),
                contactPersonName: request.ContactPersonName,
                contactPersonTitle: request.ContactPersonTitle,
                billingAddress: new Address(
                    request.BillingStreet,
                    request.BillingCity,
                    request.BillingState,
                    request.BillingCountry,
                    request.BillingPostalCode),
                shippingAddress: new Address(
                    request.ShippingStreet,
                    request.ShippingCity,
                    request.ShippingState,
                    request.ShippingCountry,
                    request.ShippingPostalCode),
                creditLimit: new Money(request.CreditLimit, request.Currency),
                type: customerType
            );

            await _unitOfWork.Customers.AddAsync(customer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Create user account
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.ContactPersonName.Split(' ').FirstOrDefault(),
                LastName = request.ContactPersonName.Split(' ').LastOrDefault(),
                CustomerId = customer.Id,
                IsActive = true
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                _logger.LogWarning("User creation failed: {Errors}", errors);
                return Result<CustomerDto>.Failure($"Failed to create user account: {errors}", "USER_CREATION_FAILED");
            }

            await _unitOfWork.CommitAsync(cancellationToken);

            _logger.LogInformation("Customer {CompanyName} registered successfully with ID {CustomerId}",
                customer.CompanyName, customer.Id);

            return Result<CustomerDto>.Success(MapToCustomerDto(customer));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error during customer registration for {Email}", request.Email);
            return Result<CustomerDto>.Failure("Registration failed. Please try again.", "REGISTRATION_FAILED");
        }
    }

    public async Task<Result<LoginResponseDto>> RefreshTokenAsync(RefreshTokenDto request, CancellationToken cancellationToken = default)
    {
        var users = _userManager.Users.Where(u => u.RefreshToken == request.RefreshToken);
        var user = users.FirstOrDefault();

        if (user is null)
        {
            return Result<LoginResponseDto>.Failure("Invalid refresh token", "INVALID_REFRESH_TOKEN");
        }

        if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return Result<LoginResponseDto>.Failure("Refresh token expired", "REFRESH_TOKEN_EXPIRED");
        }

        if (!user.IsActive)
        {
            return Result<LoginResponseDto>.Failure("Account is inactive", "ACCOUNT_INACTIVE");
        }

        // Get customer if linked
        Customer? customer = null;
        if (user.CustomerId.HasValue)
        {
            customer = await _unitOfWork.Customers.GetByIdAsync(user.CustomerId.Value, cancellationToken);
        }

        // Generate new tokens
        var token = await GenerateJwtTokenAsync(user, customer);
        var refreshToken = GenerateRefreshToken();

        // Save new refresh token
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(
            _configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 7));

        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Token refreshed for user {Email}", user.Email);

        return Result<LoginResponseDto>.Success(new LoginResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:ExpirationMinutes", 15)),
            CustomerId = customer?.Id ?? Guid.Empty,
            Email = user.Email!,
            CompanyName = customer?.CompanyName ?? string.Empty,
            IsApproved = customer?.IsApproved ?? true
        });
    }

    public async Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure("User not found", "USER_NOT_FOUND");
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure($"Password change failed: {errors}", "PASSWORD_CHANGE_FAILED");
        }

        // Invalidate refresh token to force re-login
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Password changed for user {UserId}", userId);

        return Result.Success();
    }

    public async Task<Result> LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure("User not found", "USER_NOT_FOUND");
        }

        // Invalidate refresh token
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("User {UserId} logged out", userId);

        return Result.Success();
    }

    public Task<Result<bool>> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return Task.FromResult(Result<bool>.Success(true));
        }
        catch (Exception)
        {
            return Task.FromResult(Result<bool>.Success(false));
        }
    }

    private async Task<string> GenerateJwtTokenAsync(ApplicationUser user, Customer? customer)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("userId", user.Id.ToString())
        };

        // Add user roles
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        if (user.CustomerId.HasValue)
        {
            claims.Add(new Claim("customerId", user.CustomerId.Value.ToString()));
        }

        if (customer is not null)
        {
            claims.Add(new Claim("companyName", customer.CompanyName));
            claims.Add(new Claim("priceTier", customer.PriceTier.ToString()));
            claims.Add(new Claim("isApproved", customer.IsApproved.ToString().ToLower()));
        }

        if (!string.IsNullOrEmpty(user.FirstName))
        {
            claims.Add(new Claim("firstName", user.FirstName));
        }

        if (!string.IsNullOrEmpty(user.LastName))
        {
            claims.Add(new Claim("lastName", user.LastName));
        }

        var expirationMinutes = _configuration.GetValue<int>("Jwt:ExpirationMinutes", 15);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private static CustomerDto MapToCustomerDto(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            CompanyName = customer.CompanyName,
            TaxNumber = customer.TaxNumber.Value,
            Email = customer.Email.Value,
            Phone = customer.Phone.Value,
            Type = customer.Type.ToString(),
            PriceTier = customer.PriceTier.ToString(),
            CreditLimit = customer.CreditLimit.Amount,
            UsedCredit = customer.UsedCredit.Amount,
            AvailableCredit = customer.GetAvailableCredit().Amount,
            Currency = customer.CreditLimit.Currency,
            IsApproved = customer.IsApproved,
            ApprovedAt = customer.ApprovedAt,
            ApprovedBy = customer.ApprovedBy,
            ContactPersonName = customer.ContactPersonName,
            ContactPersonTitle = customer.ContactPersonTitle,
            BillingStreet = customer.BillingAddress.Street,
            BillingCity = customer.BillingAddress.City,
            BillingState = customer.BillingAddress.State,
            BillingCountry = customer.BillingAddress.Country,
            BillingPostalCode = customer.BillingAddress.PostalCode,
            ShippingStreet = customer.ShippingAddress.Street,
            ShippingCity = customer.ShippingAddress.City,
            ShippingState = customer.ShippingAddress.State,
            ShippingCountry = customer.ShippingAddress.Country,
            ShippingPostalCode = customer.ShippingAddress.PostalCode,
            PreferredCurrency = customer.PreferredCurrency,
            PreferredLanguage = customer.PreferredLanguage,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt ?? customer.CreatedAt
        };
    }
}
