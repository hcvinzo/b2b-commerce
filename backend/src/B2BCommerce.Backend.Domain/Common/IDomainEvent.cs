namespace B2BCommerce.Backend.Domain.Common;

/// <summary>
/// Marker interface for domain events.
/// Domain events are dispatched via MediatR INotification - the mapping happens in Infrastructure layer.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// The date and time when the event occurred
    /// </summary>
    DateTime OccurredOn { get; }
}

/// <summary>
/// Base record for domain events with common properties
/// </summary>
public abstract record DomainEventBase : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
