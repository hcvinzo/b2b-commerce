using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.ValueObjects;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Entity representing a newsletter subscription
/// </summary>
public class NewsletterSubscription : BaseEntity, IAggregateRoot
{
    public Email Email { get; private set; } = null!;
    public DateTime SubscribedAt { get; private set; }
    public bool IsVerified { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public DateTime? UnsubscribedAt { get; private set; }

    // Private constructor for EF Core
    private NewsletterSubscription() { }

    public NewsletterSubscription(Email email)
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
        SubscribedAt = DateTime.UtcNow;
        IsVerified = false;
    }

    /// <summary>
    /// Factory method to create a new newsletter subscription
    /// </summary>
    public static NewsletterSubscription Create(string email)
    {
        return new NewsletterSubscription(new Email(email));
    }

    /// <summary>
    /// Marks the subscription as verified
    /// </summary>
    public void Verify()
    {
        if (!IsVerified)
        {
            IsVerified = true;
            VerifiedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Marks the subscription as unsubscribed
    /// </summary>
    public void Unsubscribe()
    {
        if (UnsubscribedAt is null)
        {
            UnsubscribedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Reactivates a previously unsubscribed subscription
    /// </summary>
    public void Resubscribe()
    {
        UnsubscribedAt = null;
        SubscribedAt = DateTime.UtcNow;
    }
}
