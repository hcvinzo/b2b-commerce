namespace B2BCommerce.Backend.Application.DTOs.CustomerUsers;

/// <summary>
/// DTO for creating a new customer user
/// </summary>
public class CreateCustomerUserDto
{
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public List<Guid> RoleIds { get; set; } = new();
    public string? TemporaryPassword { get; set; }
    public bool SendWelcomeEmail { get; set; } = true;
}
