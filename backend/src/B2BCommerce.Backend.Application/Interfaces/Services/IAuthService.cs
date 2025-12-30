using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Auth;
using B2BCommerce.Backend.Application.DTOs.Customers;

namespace B2BCommerce.Backend.Application.Interfaces.Services;

/// <summary>
/// Service interface for authentication operations
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticate user and return JWT token
    /// </summary>
    Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Register a new customer and create user account
    /// </summary>
    Task<Result<CustomerDto>> RegisterAsync(RegisterCustomerDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refresh JWT token using refresh token
    /// </summary>
    Task<Result<LoginResponseDto>> RefreshTokenAsync(RefreshTokenDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Change user password
    /// </summary>
    Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logout user and invalidate refresh token
    /// </summary>
    Task<Result> LogoutAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate if a token is still valid
    /// </summary>
    Task<Result<bool>> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if an email is available for registration
    /// </summary>
    Task<Result<bool>> IsEmailAvailableAsync(string email, CancellationToken cancellationToken = default);
}
