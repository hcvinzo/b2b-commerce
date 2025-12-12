using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Application.Interfaces.Repositories;

/// <summary>
/// Newsletter subscription repository interface
/// </summary>
public interface INewsletterSubscriptionRepository : IGenericRepository<NewsletterSubscription>
{
    /// <summary>
    /// Gets a newsletter subscription by email address
    /// </summary>
    /// <param name="email">Email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Newsletter subscription if found, null otherwise</returns>
    Task<NewsletterSubscription?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
