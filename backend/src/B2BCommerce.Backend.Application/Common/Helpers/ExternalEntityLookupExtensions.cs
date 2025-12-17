namespace B2BCommerce.Backend.Application.Common.Helpers;

/// <summary>
/// Extension methods for performing external entity lookups with consistent behavior.
/// Implements the standard lookup chain: Id → ExternalId → Fallback Key.
/// </summary>
public static class ExternalEntityLookupExtensions
{
    /// <summary>
    /// Performs a standard external entity lookup using the priority: Id → ExternalId → Fallback Key.
    /// This method encapsulates the common upsert lookup pattern for ExternalEntity types.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="id">The internal database ID (optional)</param>
    /// <param name="externalId">The external system's identifier (optional)</param>
    /// <param name="fallbackKey">A fallback lookup key like Name, SKU, or Code (optional)</param>
    /// <param name="getById">Function to lookup entity by internal ID</param>
    /// <param name="getByExternalId">Function to lookup entity by external ID</param>
    /// <param name="getByFallback">Function to lookup entity by fallback key (optional)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Lookup result containing entity (if found) and metadata for upsert decisions</returns>
    public static async Task<ExternalEntityLookupResult<T>> LookupExternalEntityAsync<T>(
        Guid? id,
        string? externalId,
        string? fallbackKey,
        Func<Guid, CancellationToken, Task<T?>> getById,
        Func<string, CancellationToken, Task<T?>> getByExternalId,
        Func<string, CancellationToken, Task<T?>>? getByFallback,
        CancellationToken ct) where T : class
    {
        T? entity = null;
        bool createWithSpecificId = false;

        // 1. Try by Id first
        if (id.HasValue)
        {
            entity = await getById(id.Value, ct);

            // If Id is provided but not found, we'll create with that specific Id
            if (entity is null)
            {
                createWithSpecificId = true;
            }
        }

        // 2. Try by ExternalId (if not found by Id and not creating with specific Id)
        if (entity is null && !createWithSpecificId && !string.IsNullOrEmpty(externalId))
        {
            entity = await getByExternalId(externalId, ct);
        }

        // 3. Try by fallback key (if provided and still not found)
        if (entity is null && !createWithSpecificId && !string.IsNullOrEmpty(fallbackKey) && getByFallback is not null)
        {
            entity = await getByFallback(fallbackKey, ct);
        }

        // Resolve effective ExternalId
        var effectiveExternalId = ExternalEntityHelper.ResolveExternalId(externalId, id);

        return new ExternalEntityLookupResult<T>
        {
            Entity = entity,
            CreateWithSpecificId = createWithSpecificId,
            EffectiveExternalId = effectiveExternalId
        };
    }

    /// <summary>
    /// Performs a standard external entity lookup without a fallback key.
    /// Uses only Id → ExternalId lookup chain.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="id">The internal database ID (optional)</param>
    /// <param name="externalId">The external system's identifier (optional)</param>
    /// <param name="getById">Function to lookup entity by internal ID</param>
    /// <param name="getByExternalId">Function to lookup entity by external ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Lookup result containing entity (if found) and metadata for upsert decisions</returns>
    public static Task<ExternalEntityLookupResult<T>> LookupExternalEntityAsync<T>(
        Guid? id,
        string? externalId,
        Func<Guid, CancellationToken, Task<T?>> getById,
        Func<string, CancellationToken, Task<T?>> getByExternalId,
        CancellationToken ct) where T : class
    {
        return LookupExternalEntityAsync(
            id,
            externalId,
            fallbackKey: null,
            getById,
            getByExternalId,
            getByFallback: null,
            ct);
    }
}
