using System.Security.Claims;
using System.Text.Encodings.Web;
using B2BCommerce.Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace B2BCommerce.Backend.IntegrationAPI.Authentication;

/// <summary>
/// Authentication handler for API Key-based authentication
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private readonly IApiKeyService _apiKeyService;
    private readonly ApiKeySettings _apiKeySettings;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApiKeyService apiKeyService,
        IOptions<ApiKeySettings> apiKeySettings)
        : base(options, logger, encoder)
    {
        _apiKeyService = apiKeyService;
        _apiKeySettings = apiKeySettings.Value;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if API key header exists
        if (!Request.Headers.TryGetValue(_apiKeySettings.HeaderName, out var apiKeyHeaderValues))
        {
            return AuthenticateResult.NoResult();
        }

        var providedApiKey = apiKeyHeaderValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(providedApiKey))
        {
            return AuthenticateResult.NoResult();
        }

        // Get client IP address
        var ipAddress = GetClientIpAddress();

        // Validate the API key
        var validationResult = await _apiKeyService.ValidateKeyAsync(providedApiKey, ipAddress);

        if (!validationResult.IsValid)
        {
            Logger.LogWarning(
                "API key validation failed. Error: {ErrorCode} - {ErrorMessage}. IP: {IpAddress}",
                validationResult.ErrorCode,
                validationResult.ErrorMessage,
                ipAddress);

            return AuthenticateResult.Fail(validationResult.ErrorMessage ?? "Invalid API key");
        }

        // Create claims
        // Use UserId as NameIdentifier for audit trail (CreatedBy, UpdatedBy)
        // Falls back to ApiKeyId if UserId is not set
        var nameIdentifier = validationResult.UserId ?? validationResult.ApiKeyId.ToString()!;
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, nameIdentifier),
            new Claim("api_key_id", validationResult.ApiKeyId.ToString()!),
            new Claim("api_client_id", validationResult.ApiClientId.ToString()!),
            new Claim("client_name", validationResult.ClientName ?? string.Empty),
            new Claim("rate_limit", validationResult.RateLimitPerMinute.ToString())
        };

        // Add permission scopes as claims
        foreach (var permission in validationResult.Permissions)
        {
            claims.Add(new Claim("scope", permission));
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        // Store validation result in HttpContext for middleware use
        Context.Items["ApiKeyValidationResult"] = validationResult;

        return AuthenticateResult.Success(ticket);
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.Headers.Append("WWW-Authenticate", $"ApiKey realm=\"B2B Integration API\", header=\"{_apiKeySettings.HeaderName}\"");
        return Task.CompletedTask;
    }

    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    }

    private string GetClientIpAddress()
    {
        // Check for forwarded headers first (for proxies/load balancers)
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // Take the first IP if multiple are present
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return Context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
