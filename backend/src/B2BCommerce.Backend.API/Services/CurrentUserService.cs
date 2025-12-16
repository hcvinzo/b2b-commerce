using System.Security.Claims;
using B2BCommerce.Backend.Application.Interfaces.Services;

namespace B2BCommerce.Backend.API.Services;

/// <summary>
/// Implementation of ICurrentUserService for the Main API.
/// Extracts user ID from JWT bearer token claims.
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
