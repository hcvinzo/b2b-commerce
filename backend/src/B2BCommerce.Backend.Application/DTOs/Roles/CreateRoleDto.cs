using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.DTOs.Roles;

/// <summary>
/// DTO for creating a new role
/// </summary>
public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public UserType UserType { get; set; } = UserType.Admin;
    public List<string>? Claims { get; set; }
}
