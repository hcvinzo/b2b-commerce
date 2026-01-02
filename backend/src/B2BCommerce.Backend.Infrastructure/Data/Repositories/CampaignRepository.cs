using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace B2BCommerce.Backend.Infrastructure.Data.Repositories;

/// <summary>
/// Campaign repository implementation for campaign-specific operations
/// </summary>
public class CampaignRepository : GenericRepository<Campaign>, ICampaignRepository
{
    public CampaignRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets a campaign by ID with discount rules loaded
    /// </summary>
    public override async Task<Campaign?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.DiscountRules.Where(r => !r.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets a campaign by its external ID
    /// </summary>
    public async Task<Campaign?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.DiscountRules.Where(r => !r.IsDeleted))
            .FirstOrDefaultAsync(c => c.ExternalId == externalId && !c.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Checks if a campaign exists by its external ID
    /// </summary>
    public async Task<bool> ExistsByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(c => c.ExternalId == externalId && !c.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets a campaign with all its discount rules loaded
    /// </summary>
    public async Task<Campaign?> GetWithRulesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.DiscountRules.Where(r => !r.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets a campaign with all its discount rules and their targets loaded
    /// </summary>
    public async Task<Campaign?> GetWithRulesAndTargetsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.DiscountRules.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.Products.Where(p => !p.IsDeleted))
                    .ThenInclude(p => p.Product)
            .Include(c => c.DiscountRules.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.Categories.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.Category)
            .Include(c => c.DiscountRules.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.Brands.Where(b => !b.IsDeleted))
                    .ThenInclude(b => b.Brand)
            .Include(c => c.DiscountRules.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.Customers.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.Customer)
            .Include(c => c.DiscountRules.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.CustomerTiers.Where(t => !t.IsDeleted))
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets all active campaigns (Status = Active and within date range)
    /// </summary>
    public async Task<IEnumerable<Campaign>> GetActiveCampaignsAsync(DateTime currentTime, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.DiscountRules.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.Products.Where(p => !p.IsDeleted))
            .Include(c => c.DiscountRules.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.Categories.Where(c => !c.IsDeleted))
            .Include(c => c.DiscountRules.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.Brands.Where(b => !b.IsDeleted))
            .Include(c => c.DiscountRules.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.Customers.Where(c => !c.IsDeleted))
            .Include(c => c.DiscountRules.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.CustomerTiers.Where(t => !t.IsDeleted))
            .Where(c => !c.IsDeleted &&
                       c.Status == CampaignStatus.Active &&
                       c.StartDate <= currentTime &&
                       c.EndDate >= currentTime)
            .OrderByDescending(c => c.Priority)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets campaigns by status
    /// </summary>
    public async Task<IEnumerable<Campaign>> GetByStatusAsync(CampaignStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.DiscountRules.Where(r => !r.IsDeleted))
            .Where(c => !c.IsDeleted && c.Status == status)
            .OrderByDescending(c => c.Priority)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets customer usage statistics for a campaign
    /// </summary>
    public async Task<(int usageCount, decimal totalDiscount)> GetCustomerUsageAsync(
        Guid campaignId,
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var usages = await _context.CampaignUsages
            .Where(u => u.CampaignId == campaignId &&
                       u.CustomerId == customerId &&
                       !u.IsReversed &&
                       !u.IsDeleted)
            .ToListAsync(cancellationToken);

        var usageCount = usages.Count;
        var totalDiscount = usages.Sum(u => u.DiscountAmount.Amount);

        return (usageCount, totalDiscount);
    }

    /// <summary>
    /// Gets all campaign usages for an order (for reversal)
    /// </summary>
    public async Task<IEnumerable<CampaignUsage>> GetUsagesByOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.CampaignUsages
            .Include(u => u.Campaign)
            .Where(u => u.OrderId == orderId && !u.IsReversed && !u.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Adds a campaign usage record
    /// </summary>
    public async Task AddUsageAsync(CampaignUsage usage, CancellationToken cancellationToken = default)
    {
        await _context.CampaignUsages.AddAsync(usage, cancellationToken);
    }

    /// <summary>
    /// Gets a discount rule by ID with its targets loaded
    /// </summary>
    public async Task<DiscountRule?> GetDiscountRuleWithTargetsAsync(Guid ruleId, CancellationToken cancellationToken = default)
    {
        return await _context.DiscountRules
            .Include(r => r.Products.Where(p => !p.IsDeleted))
                .ThenInclude(p => p.Product)
            .Include(r => r.Categories.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.Category)
            .Include(r => r.Brands.Where(b => !b.IsDeleted))
                .ThenInclude(b => b.Brand)
            .Include(r => r.Customers.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.Customer)
            .Include(r => r.CustomerTiers.Where(t => !t.IsDeleted))
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.Id == ruleId && !r.IsDeleted, cancellationToken);
    }
}
