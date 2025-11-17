namespace B2BCommerce.Backend.Application.DTOs.Auth;

/// <summary>
/// Data transfer object for login responses
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// JWT access token
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Refresh token for obtaining new access tokens
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration timestamp
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Customer identifier
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Customer email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Company name
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Whether customer is approved
    /// </summary>
    public bool IsApproved { get; set; }
}
