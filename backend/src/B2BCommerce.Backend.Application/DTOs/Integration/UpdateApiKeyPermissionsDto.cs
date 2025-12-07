namespace B2BCommerce.Backend.Application.DTOs.Integration;

/// <summary>
/// DTO for updating API key permissions
/// </summary>
public class UpdateApiKeyPermissionsDto
{
    public List<string> Permissions { get; set; } = new();
}
