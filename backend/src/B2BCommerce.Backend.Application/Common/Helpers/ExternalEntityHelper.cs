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
    /// Resolves the effective ExternalId from either provided ExternalId or Id.
    /// If ExternalId is provided, it takes precedence. Otherwise, falls back to Id.ToString().
    /// Returns null if neither is provided.
    /// </summary>
    /// <param name="externalId">The external system's identifier</param>
    /// <param name="id">The internal database identifier</param>
    /// <returns>The resolved external identifier, or null if neither is provided</returns>
    public static string? ResolveExternalId(string? externalId, Guid? id)
    {
        if (!string.IsNullOrEmpty(externalId))
        {
            return externalId;
        }

        if (id.HasValue)
        {
            return id.Value.ToString();
        }

        return null;
    }

    /// <summary>
    /// Determines if entity should be created with a specific ID.
    /// Returns true if ID was provided but entity wasn't found during lookup.
    /// This allows external systems to specify the internal database ID.
    /// </summary>
    /// <param name="providedId">The ID provided in the request</param>
    /// <param name="existingEntity">The entity found during lookup (null if not found)</param>
    /// <returns>True if the entity should be created with the specific ID</returns>
    public static bool ShouldCreateWithSpecificId(Guid? providedId, object? existingEntity)
    {
        return providedId.HasValue && existingEntity is null;
    }
}
