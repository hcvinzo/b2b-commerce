namespace B2BCommerce.Backend.Application.Common.Helpers;

/// <summary>
/// Static helper methods for ExternalEntity upsert operations.
/// Provides consistent behavior for external system synchronization.
/// </summary>
/// <remarks>
/// Note: Audit fields (CreatedBy, UpdatedBy, CreatedAt, UpdatedAt) are automatically
/// handled by ApplicationDbContext.SaveChangesAsync via ICurrentUserService.
/// No manual setting is required in handlers.
/// </remarks>
public static class ExternalEntityHelper
{
    /// <summary>
    /// Resolves the effective ExternalId from the provided ExternalId.
    /// Returns null if not provided (entity will be created with auto-generated ExternalId).
    /// </summary>
    /// <param name="externalId">The external system's identifier</param>
    /// <returns>The external identifier, or null if not provided</returns>
    public static string? ResolveExternalId(string? externalId)
    {
        return string.IsNullOrEmpty(externalId) ? null : externalId;
    }
}
