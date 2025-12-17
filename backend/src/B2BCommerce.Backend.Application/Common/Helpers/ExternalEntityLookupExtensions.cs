namespace B2BCommerce.Backend.Application.Common.Helpers;

/// <summary>
/// Extension methods for performing external entity lookups with consistent behavior.
/// Implements the standard lookup chain: ExternalId → Fallback Key.
/// </summary>
public static class ExternalEntityLookupExtensions
{
    /// <summary>
    /// Performs a standard external entity lookup using the priority: ExternalId → Fallback Key.
    /// This method encapsulates the common upsert lookup pattern for ExternalEntity types.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="externalId">The external system's identifier (optional)</param>
    /// <param name="fallbackKey">A fallback lookup key like Name, SKU, or Code (optional)</param>
    /// <param name="getByExternalId">Function to lookup entity by external ID</param>
    /// <param name="getByFallback">Function to lookup entity by fallback key (optional)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Lookup result containing entity (if found) and metadata for upsert decisions</returns>
    public static async Task<ExternalEntityLookupResult<T>> LookupExternalEntityAsync<T>(
        string? externalId,
        string? fallbackKey,
        Func<string, CancellationToken, Task<T?>> getByExternalId,
        Func<string, CancellationToken, Task<T?>>? getByFallback,
        CancellationToken ct) where T : class
    {
        T? entity = null;

        // 1. Try by ExternalId first
        if (!string.IsNullOrEmpty(externalId))
        {
            entity = await getByExternalId(externalId, ct);
        }

        // 2. Try by fallback key (if provided and still not found)
        if (entity is null && !string.IsNullOrEmpty(fallbackKey) && getByFallback is not null)
        {
            entity = await getByFallback(fallbackKey, ct);
        }

        // Resolve effective ExternalId
        var effectiveExternalId = ExternalEntityHelper.ResolveExternalId(externalId);

        return new ExternalEntityLookupResult<T>
        {
            Entity = entity,
            EffectiveExternalId = effectiveExternalId
        };
    }

    /// <summary>
    /// Performs a standard external entity lookup without a fallback key.
    /// Uses only ExternalId lookup.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="externalId">The external system's identifier (optional)</param>
    /// <param name="getByExternalId">Function to lookup entity by external ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Lookup result containing entity (if found) and metadata for upsert decisions</returns>
    public static Task<ExternalEntityLookupResult<T>> LookupExternalEntityAsync<T>(
        string? externalId,
        Func<string, CancellationToken, Task<T?>> getByExternalId,
        CancellationToken ct) where T : class
    {
        return LookupExternalEntityAsync(
            externalId,
            fallbackKey: null,
            getByExternalId,
            getByFallback: null,
            ct);
    }
}
