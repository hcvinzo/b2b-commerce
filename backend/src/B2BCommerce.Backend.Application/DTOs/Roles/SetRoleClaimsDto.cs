namespace B2BCommerce.Backend.Application.DTOs.Roles;

/// <summary>
/// DTO for setting role claims
/// </summary>
public class SetRoleClaimsDto
{
    public List<string> Claims { get; set; } = new();
}
