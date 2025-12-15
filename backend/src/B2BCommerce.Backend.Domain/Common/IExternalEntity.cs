namespace B2BCommerce.Backend.Domain.Common;

/// <summary>
/// Marker interface for entities that are synced from external systems (ERP, etc.)
/// </summary>
public interface IExternalEntity
{
    /// <summary>
    /// The code/identifier used in the external system
    /// </summary>
    string? ExternalCode { get; }

    /// <summary>
    /// The unique ID in the external system
    /// </summary>
    string? ExternalId { get; }

    /// <summary>
    /// When the entity was last synchronized with the external system
    /// </summary>
    DateTime? LastSyncedAt { get; }

    /// <summary>
    /// Marks the entity as synchronized with the external system
    /// </summary>
    void MarkAsSynced();
}
