namespace B2BCommerce.Backend.Application.DTOs.Roles;

/// <summary>
/// DTO for a single available permission
/// </summary>
public class AvailablePermissionDto
{
    public string Value { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
}

/// <summary>
/// DTO for a category of permissions
/// </summary>
public class PermissionCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<AvailablePermissionDto> Permissions { get; set; } = new();
}
