namespace B2BCommerce.Backend.Application.DTOs.CustomerUsers;

/// <summary>
/// DTO for setting customer user roles
/// </summary>
public class SetCustomerUserRolesDto
{
    public List<Guid> RoleIds { get; set; } = new();
}
