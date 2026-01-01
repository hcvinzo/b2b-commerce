namespace B2BCommerce.Backend.Application.DTOs.CustomerUsers;

/// <summary>
/// Summary DTO for customer user list views
/// </summary>
public class CustomerUserListDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string FullName { get; set; } = string.Empty;
    public List<CustomerUserRoleDto> Roles { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Customer user role information
/// </summary>
public class CustomerUserRoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
