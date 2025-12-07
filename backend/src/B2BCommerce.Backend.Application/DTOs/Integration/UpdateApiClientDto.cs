namespace B2BCommerce.Backend.Application.DTOs.Integration;

/// <summary>
/// DTO for updating an existing API client
/// </summary>
public class UpdateApiClientDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
}
