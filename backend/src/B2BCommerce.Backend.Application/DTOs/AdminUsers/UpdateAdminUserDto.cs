namespace B2BCommerce.Backend.Application.DTOs.AdminUsers;

/// <summary>
/// DTO for updating an admin user
/// </summary>
public class UpdateAdminUserDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public List<string>? Roles { get; set; }
}
