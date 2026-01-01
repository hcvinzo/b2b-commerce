namespace B2BCommerce.Backend.Application.DTOs.CustomerUsers;

/// <summary>
/// Detailed DTO for customer user
/// </summary>
public class CustomerUserDetailDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerTitle { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public List<CustomerUserRoleDto> Roles { get; set; } = new();
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
