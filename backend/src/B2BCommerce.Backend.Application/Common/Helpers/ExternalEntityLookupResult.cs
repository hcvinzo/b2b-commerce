namespace B2BCommerce.Backend.Application.Common.Helpers;

/// <summary>
/// Result of an external entity lookup operation.
/// Contains the found entity (if any) along with metadata for upsert decisions.
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public record ExternalEntityLookupResult<T> where T : class
{
    /// <summary>
    /// The found entity, or null if not found.
    /// </summary>
    public T? Entity { get; init; }

    /// <summary>
    /// True if the entity should be created with a specific ID (ID was provided but entity wasn't found).
    /// </summary>
    public bool CreateWithSpecificId { get; init; }

    /// <summary>
    /// The resolved ExternalId to use (either from request.ExternalId or request.Id.ToString()).
    /// </summary>
    public string? EffectiveExternalId { get; init; }

    /// <summary>
    /// True if this is a new entity creation (entity was not found).
    /// </summary>
    public bool IsNew => Entity is null;

    /// <summary>
    /// True if this is an update operation (entity was found).
    /// </summary>
    public bool IsUpdate => Entity is not null;
}
