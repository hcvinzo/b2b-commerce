namespace B2BCommerce.Backend.Application.DTOs.Integration;

/// <summary>
/// Detailed DTO for API client with all information including keys
/// </summary>
public class ApiClientDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public List<ApiKeyListDto> ApiKeys { get; set; } = new();
}
