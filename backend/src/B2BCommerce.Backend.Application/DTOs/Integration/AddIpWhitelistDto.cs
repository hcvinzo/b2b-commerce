namespace B2BCommerce.Backend.Application.DTOs.Integration;

/// <summary>
/// DTO for adding an IP address to the whitelist
/// </summary>
public class AddIpWhitelistDto
{
    public string IpAddress { get; set; } = string.Empty;
    public string? Description { get; set; }
}
