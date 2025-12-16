using System.Security.Claims;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.IntegrationAPI.Services;

/// <summary>
/// Implementation of ICurrentUserService for the Integration API.
/// Extracts user ID from the API client's associated user (via API key authentication).
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public string? UserId =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    /// <inheritdoc />
    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
