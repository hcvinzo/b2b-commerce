namespace B2BCommerce.Backend.Application.DTOs.Roles;

/// <summary>
/// DTO for creating a new role
/// </summary>
public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string>? Claims { get; set; }
}
