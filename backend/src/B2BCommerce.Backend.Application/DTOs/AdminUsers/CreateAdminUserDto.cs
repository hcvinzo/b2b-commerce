namespace B2BCommerce.Backend.Application.DTOs.AdminUsers;

/// <summary>
/// DTO for creating a new admin user
/// </summary>
public class CreateAdminUserDto
{
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public List<string> Roles { get; set; } = new();
    public string? TemporaryPassword { get; set; }
    public bool SendWelcomeEmail { get; set; } = true;
}
