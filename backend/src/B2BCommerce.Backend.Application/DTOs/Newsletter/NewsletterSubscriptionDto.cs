namespace B2BCommerce.Backend.Application.DTOs.Newsletter;

/// <summary>
/// DTO for newsletter subscription response
/// </summary>
public class NewsletterSubscriptionDto
{
    /// <summary>
    /// Subscription ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Subscribed email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when subscribed
    /// </summary>
    public DateTime SubscribedAt { get; set; }

    /// <summary>
    /// Whether the subscription is verified
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// Success message
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
