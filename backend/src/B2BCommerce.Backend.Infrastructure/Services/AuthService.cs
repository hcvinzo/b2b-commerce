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
using B2BCommerce.Backend.Infrastructure.Data;
using B2BCommerce.Backend.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly ICustomerAttributeService _customerAttributeService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<AuthService> logger,
        ICustomerAttributeService customerAttributeService)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _customerAttributeService = customerAttributeService;
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
            CustomerTitle = customer?.Title ?? string.Empty,
            Status = customer?.Status.ToString() ?? string.Empty
        });
    }

    public async Task<Result<CustomerDto>> RegisterAsync(RegisterCustomerDto request, CancellationToken cancellationToken = default)
    {
        // Check if email already exists (only check users if password will be created)
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser is not null)
            {
                return Result<CustomerDto>.Failure("Email already registered", "EMAIL_EXISTS");
            }
        }

        // Check if tax number already exists (if provided)
        if (!string.IsNullOrWhiteSpace(request.TaxNo))
        {
            var existingCustomer = await _unitOfWork.Customers.GetByTaxNoAsync(request.TaxNo, cancellationToken);
            if (existingCustomer is not null)
            {
                return Result<CustomerDto>.Failure("Tax number already registered", "TAX_NUMBER_EXISTS");
            }
        }

        try
        {
            // Use execution strategy to support retrying transactions with Npgsql
            var strategy = _context.Database.CreateExecutionStrategy();

            Customer? customer = null;
            string? userCreationError = null;

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    // Create customer using factory method
                    customer = Customer.Create(
                        title: request.Title,
                        taxOffice: request.TaxOffice,
                        taxNo: request.TaxNo,
                        establishmentYear: request.EstablishmentYear,
                        website: request.Website
                    );

                    // Set document URLs if provided
                    if (!string.IsNullOrWhiteSpace(request.DocumentUrls))
                    {
                        customer.UpdateDocumentUrls(request.DocumentUrls);
                    }

                    await _unitOfWork.Customers.AddAsync(customer, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    // Create primary contact if contact info is provided
                    if (!string.IsNullOrWhiteSpace(request.ContactFirstName) &&
                        !string.IsNullOrWhiteSpace(request.ContactLastName))
                    {
                        // Parse gender from string
                        var gender = Gender.Unknown;
                        if (!string.IsNullOrWhiteSpace(request.ContactGender))
                        {
                            Enum.TryParse<Gender>(request.ContactGender, ignoreCase: true, out gender);
                        }

                        // Parse date of birth
                        DateOnly? dateOfBirth = null;
                        if (request.ContactDateOfBirth.HasValue)
                        {
                            dateOfBirth = DateOnly.FromDateTime(request.ContactDateOfBirth.Value);
                        }

                        var contact = CustomerContact.Create(
                            customerId: customer.Id,
                            firstName: request.ContactFirstName,
                            lastName: request.ContactLastName,
                            email: request.ContactEmail,
                            position: request.ContactPosition,
                            dateOfBirth: dateOfBirth,
                            gender: gender,
                            phone: request.ContactPhone,
                            phoneExt: request.ContactPhoneExt,
                            gsm: request.ContactGsm,
                            isPrimary: true
                        );

                        await _unitOfWork.CustomerContacts.AddAsync(contact, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }

                    // Create address if address info is provided
                    if (!string.IsNullOrWhiteSpace(request.AddressTitle) &&
                        !string.IsNullOrWhiteSpace(request.Address))
                    {
                        var address = CustomerAddress.Create(
                            customerId: customer.Id,
                            title: request.AddressTitle,
                            addressType: CustomerAddressType.Billing,
                            address: request.Address,
                            geoLocationId: request.GeoLocationId,
                            postalCode: request.PostalCode,
                            phone: request.AddressPhone,
                            phoneExt: request.AddressPhoneExt,
                            gsm: request.AddressGsm,
                            isDefault: true
                        );

                        await _unitOfWork.CustomerAddresses.AddAsync(address, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }

                    // Only create user account if password is provided
                    // For dealer applications without password, user account is created after admin approval
                    if (!string.IsNullOrWhiteSpace(request.Password))
                    {
                        var user = new ApplicationUser
                        {
                            UserName = request.Email,
                            Email = request.Email,
                            FirstName = request.ContactFirstName,
                            LastName = request.ContactLastName,
                            CustomerId = customer.Id,
                            IsActive = true
                        };

                        var createResult = await _userManager.CreateAsync(user, request.Password);
                        if (!createResult.Succeeded)
                        {
                            userCreationError = string.Join(", ", createResult.Errors.Select(e => e.Description));
                            throw new InvalidOperationException($"User creation failed: {userCreationError}");
                        }

                        // Link user to customer
                        customer.SetUserId(user.Id);
                        _unitOfWork.Customers.Update(customer);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }

                    // Save customer attributes within the transaction
                    if (request.Attributes is not null && request.Attributes.Count > 0)
                    {
                        _logger.LogInformation(
                            "Processing {Count} attribute groups for customer {CustomerId}",
                            request.Attributes.Count, customer!.Id);

                        foreach (var attributeGroup in request.Attributes)
                        {
                            _logger.LogDebug(
                                "Saving attribute group {AttributeDefinitionId} with {ItemCount} items for customer {CustomerId}",
                                attributeGroup.AttributeDefinitionId, attributeGroup.Items?.Count ?? 0, customer!.Id);

                            var attributeResult = await _customerAttributeService.UpsertByDefinitionAsync(
                                customer!.Id,
                                attributeGroup,
                                cancellationToken);

                            if (!attributeResult.IsSuccess)
                            {
                                throw new InvalidOperationException(
                                    $"Failed to save attributes for definition {attributeGroup.AttributeDefinitionId}: {attributeResult.ErrorMessage}");
                            }
                        }

                        _logger.LogInformation(
                            "Successfully saved all {Count} attribute groups for customer {CustomerId}",
                            request.Attributes.Count, customer!.Id);
                    }

                    await transaction.CommitAsync(cancellationToken);
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });

            if (userCreationError is not null)
            {
                _logger.LogWarning("User creation failed: {Errors}", userCreationError);
                return Result<CustomerDto>.Failure($"Failed to create user account: {userCreationError}", "USER_CREATION_FAILED");
            }

            _logger.LogInformation("Customer {Title} registered successfully with ID {CustomerId}",
                customer!.Title, customer.Id);

            return Result<CustomerDto>.Success(MapToCustomerDto(customer));
        }
        catch (Exception ex)
        {
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
            CustomerTitle = customer?.Title ?? string.Empty,
            Status = customer?.Status.ToString() ?? string.Empty
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
            claims.Add(new Claim("customerTitle", customer.Title));
            claims.Add(new Claim("customerStatus", customer.Status.ToString()));
            claims.Add(new Claim("isActive", (customer.Status == CustomerStatus.Active).ToString().ToLower()));
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
            ExternalId = customer.ExternalId,
            ExternalCode = customer.ExternalCode,
            Title = customer.Title,
            TaxOffice = customer.TaxOffice,
            TaxNo = customer.TaxNo,
            EstablishmentYear = customer.EstablishmentYear,
            Website = customer.Website,
            Status = customer.Status.ToString(),
            UserId = customer.UserId,
            DocumentUrls = customer.DocumentUrls,
            Contacts = customer.Contacts?.Select(c => new CustomerContactDto
            {
                Id = c.Id,
                CustomerId = c.CustomerId,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                Position = c.Position,
                DateOfBirth = c.DateOfBirth,
                Gender = c.Gender.ToString(),
                Phone = c.Phone,
                PhoneExt = c.PhoneExt,
                Gsm = c.Gsm,
                IsPrimary = c.IsPrimary,
                IsActive = c.IsActive
            }).ToList() ?? new List<CustomerContactDto>(),
            Addresses = customer.Addresses?.Select(a => new CustomerAddressDto
            {
                Id = a.Id,
                CustomerId = a.CustomerId,
                Title = a.Title,
                FullName = a.FullName,
                AddressType = a.AddressType.ToString(),
                Address = a.Address,
                GeoLocationId = a.GeoLocationId,
                GeoLocationPathName = a.GeoLocation?.PathName,
                PostalCode = a.PostalCode,
                Phone = a.Phone,
                PhoneExt = a.PhoneExt,
                Gsm = a.Gsm,
                TaxNo = a.TaxNo,
                IsDefault = a.IsDefault,
                IsActive = a.IsActive
            }).ToList() ?? new List<CustomerAddressDto>(),
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt,
            LastSyncedAt = customer.LastSyncedAt
        };
    }
}
