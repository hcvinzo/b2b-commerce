namespace B2BCommerce.Backend.Application.Interfaces.Services;

/// <summary>
/// Service for accessing current user context.
/// Used by ApplicationDbContext to automatically set audit fields (CreatedBy, UpdatedBy).
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's ID.
    /// Returns null if no user is authenticated.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets whether the current request is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }
}
