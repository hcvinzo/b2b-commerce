namespace B2BCommerce.Backend.Application.DTOs.Integration;

/// <summary>
/// DTO for creating a new API client
/// </summary>
public class CreateApiClientDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
}
