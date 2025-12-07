namespace B2BCommerce.Backend.Application.DTOs.Integration;

/// <summary>
/// DTO for revoking an API key
/// </summary>
public class RevokeApiKeyDto
{
    public string Reason { get; set; } = string.Empty;
}
