namespace B2BCommerce.Backend.Application.DTOs.Integration;

/// <summary>
/// DTO for IP whitelist entries
/// </summary>
public class IpWhitelistDto
{
    public Guid Id { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string? Description { get; set; }
}
