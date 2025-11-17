namespace B2BCommerce.Backend.Application.DTOs.Auth;

/// <summary>
/// Data transfer object for refresh token requests
/// </summary>
public class RefreshTokenDto
{
    /// <summary>
    /// Refresh token
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}
