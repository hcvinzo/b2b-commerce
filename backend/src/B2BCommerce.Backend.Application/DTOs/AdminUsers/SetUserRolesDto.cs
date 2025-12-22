namespace B2BCommerce.Backend.Application.DTOs.AdminUsers;

/// <summary>
/// DTO for setting user roles
/// </summary>
public class SetUserRolesDto
{
    public List<string> Roles { get; set; } = new();
}
