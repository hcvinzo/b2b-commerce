namespace B2BCommerce.Backend.Application.DTOs.Auth;

/// <summary>
/// Data transfer object for password change requests
/// </summary>
public class ChangePasswordDto
{
    /// <summary>
    /// Current password
    /// </summary>
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// New password
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Confirm new password (must match NewPassword)
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;
}
