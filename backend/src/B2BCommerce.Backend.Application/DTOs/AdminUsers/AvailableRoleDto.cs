namespace B2BCommerce.Backend.Application.DTOs.AdminUsers;

/// <summary>
/// DTO for available roles that can be assigned to admin users
/// </summary>
public class AvailableRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
