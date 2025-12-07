namespace B2BCommerce.Backend.Application.DTOs.Integration;

/// <summary>
/// Response DTO for creating an API key - includes the plain text key which is only shown once
/// </summary>
public class CreateApiKeyResponseDto
{
    public Guid Id { get; set; }
    public string KeyPrefix { get; set; } = string.Empty;

    /// <summary>
    /// The plain text API key - only returned once at creation time
    /// </summary>
    public string PlainTextKey { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public List<string> Permissions { get; set; } = new();

    /// <summary>
    /// Security warning about storing the key
    /// </summary>
    public string Warning { get; set; } = "Store this key securely. It will not be shown again.";
}
