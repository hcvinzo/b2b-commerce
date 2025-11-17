namespace B2BCommerce.Backend.Application.DTOs.Auth;

/// <summary>
/// Data transfer object for login requests
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Password
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
