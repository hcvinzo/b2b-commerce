namespace B2BCommerce.Backend.Application.DTOs.CustomerUsers;

/// <summary>
/// DTO for updating a customer user
/// </summary>
public class UpdateCustomerUserDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
}
