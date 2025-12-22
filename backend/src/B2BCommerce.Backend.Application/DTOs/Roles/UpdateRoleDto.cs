namespace B2BCommerce.Backend.Application.DTOs.Roles;

/// <summary>
/// DTO for updating an existing role
/// </summary>
public class UpdateRoleDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}
