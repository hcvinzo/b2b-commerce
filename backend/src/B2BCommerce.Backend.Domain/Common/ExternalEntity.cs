namespace B2BCommerce.Backend.Domain.Common;

/// <summary>
/// Base class for entities that are synced from external systems (ERP, etc.)
/// Extends BaseEntity with external system tracking properties.
/// </summary>
public abstract class ExternalEntity : BaseEntity, IExternalEntity
{
    /// <summary>
    /// The code/identifier used in the external system
    /// </summary>
    public string? ExternalCode { get; protected set; }

    /// <summary>
    /// The unique ID in the external system
    /// </summary>
    public string? ExternalId { get; protected set; }

    /// <summary>
    /// When the entity was last synchronized with the external system
    /// </summary>
    public DateTime? LastSyncedAt { get; protected set; }

    protected ExternalEntity() : base()
    {
    }

    /// <summary>
    /// Marks the entity as synchronized with the external system
    /// </summary>
    public void MarkAsSynced()
    {
        LastSyncedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the external system identifiers
    /// </summary>
    protected void SetExternalIdentifiers(string? externalCode, string? externalId = null)
    {
        ExternalCode = externalCode;
        ExternalId = externalId;
    }

    /// <summary>
    /// Initializes external entity properties during creation from external systems.
    /// Sets external identifiers and marks as synced.
    /// Call this method in CreateFromExternal factory methods to ensure consistent initialization.
    /// </summary>
    /// <typeparam name="T">The entity type inheriting from ExternalEntity</typeparam>
    /// <param name="entity">The entity to initialize</param>
    /// <param name="externalId">The external system's identifier (required)</param>
    /// <param name="externalCode">Optional additional reference code</param>
    protected static void InitializeFromExternal<T>(
        T entity,
        string externalId,
        string? externalCode) where T : ExternalEntity
    {
        entity.SetExternalIdentifiers(externalCode, externalId);
        entity.MarkAsSynced();
    }
}
