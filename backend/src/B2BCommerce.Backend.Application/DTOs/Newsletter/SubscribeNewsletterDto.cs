namespace B2BCommerce.Backend.Application.DTOs.Newsletter;

/// <summary>
/// DTO for newsletter subscription request
/// </summary>
public class SubscribeNewsletterDto
{
    /// <summary>
    /// Email address to subscribe
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
