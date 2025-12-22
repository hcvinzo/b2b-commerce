namespace B2BCommerce.Backend.Application.DTOs.AdminUsers;

/// <summary>
/// DTO for user login history entry
/// </summary>
public class UserLoginDto
{
    public string Id { get; set; } = string.Empty;
    public string LoginProvider { get; set; } = string.Empty;
    public string ProviderDisplayName { get; set; } = string.Empty;
    public string? ProviderKey { get; set; }
}
