using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.DTOs.Roles;

/// <summary>
/// DTO for role detail view
/// </summary>
public class RoleDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Claims { get; set; } = new();
    public int UserCount { get; set; }
    public bool IsProtected { get; set; }
    public bool IsSystemRole { get; set; }
    public UserType UserType { get; set; }
    public DateTime CreatedAt { get; set; }
}
