namespace B2BCommerce.Backend.Application.DTOs.Integration;

/// <summary>
/// Summary DTO for API client list views
/// </summary>
public class ApiClientListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int ActiveKeyCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
