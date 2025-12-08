namespace B2BCommerce.Backend.IntegrationAPI.Authentication;

/// <summary>
/// API Key authentication settings
/// </summary>
public class ApiKeySettings
{
    /// <summary>
    /// Header name for the API key (default: X-API-Key)
    /// </summary>
    public string HeaderName { get; set; } = "X-API-Key";

    /// <summary>
    /// Enable rate limiting based on key configuration
    /// </summary>
    public bool EnableRateLimiting { get; set; } = true;

    /// <summary>
    /// Enable IP whitelist validation
    /// </summary>
    public bool EnableIpWhitelisting { get; set; } = false;
}
