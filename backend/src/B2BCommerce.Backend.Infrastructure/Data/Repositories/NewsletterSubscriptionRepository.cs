using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace B2BCommerce.Backend.Infrastructure.Data.Repositories;

/// <summary>
/// Newsletter subscription repository implementation
/// </summary>
public class NewsletterSubscriptionRepository : GenericRepository<NewsletterSubscription>, INewsletterSubscriptionRepository
{
    public NewsletterSubscriptionRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets a newsletter subscription by email address
    /// </summary>
    /// <param name="email">Email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Newsletter subscription if found, null otherwise</returns>
    public async Task<NewsletterSubscription?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        // Create Email value object to leverage EF Core's value conversion
        var emailValue = new Email(email);
        return await _dbSet
            .FirstOrDefaultAsync(n => n.Email == emailValue && !n.IsDeleted, cancellationToken);
    }
}
