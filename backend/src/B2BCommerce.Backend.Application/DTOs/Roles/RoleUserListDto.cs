namespace B2BCommerce.Backend.Application.DTOs.Roles;

/// <summary>
/// DTO for users in a role list view
/// </summary>
public class RoleUserListDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public bool IsActive { get; set; }
}
