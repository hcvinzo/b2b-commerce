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

    /// <summary>
    /// Adds a discount rule directly
    /// </summary>
    public async Task AddDiscountRuleAsync(DiscountRule rule, CancellationToken cancellationToken = default)
    {
        await _context.DiscountRules.AddAsync(rule, cancellationToken);
    }

    /// <summary>
    /// Gets campaign status without tracking (for validation only)
    /// </summary>
    public async Task<CampaignStatus?> GetStatusAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(c => c.Id == id && !c.IsDeleted)
            .Select(c => (CampaignStatus?)c.Status)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a discount rule's campaign ID without tracking (for validation)
    /// </summary>
    public async Task<Guid?> GetRuleCampaignIdAsync(Guid ruleId, CancellationToken cancellationToken = default)
    {
        return await _context.DiscountRules
            .AsNoTracking()
            .Where(r => r.Id == ruleId && !r.IsDeleted)
            .Select(r => (Guid?)r.CampaignId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Gets existing category IDs for a discount rule
    /// </summary>
    public async Task<List<Guid>> GetRuleCategoryIdsAsync(Guid ruleId, CancellationToken cancellationToken = default)
    {
        return await _context.DiscountRuleCategories
            .AsNoTracking()
            .Where(c => c.DiscountRuleId == ruleId && !c.IsDeleted)
            .Select(c => c.CategoryId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets existing brand IDs for a discount rule
    /// </summary>
    public async Task<List<Guid>> GetRuleBrandIdsAsync(Guid ruleId, CancellationToken cancellationToken = default)
    {
        return await _context.DiscountRuleBrands
            .AsNoTracking()
            .Where(b => b.DiscountRuleId == ruleId && !b.IsDeleted)
            .Select(b => b.BrandId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets existing customer IDs for a discount rule
    /// </summary>
    public async Task<List<Guid>> GetRuleCustomerIdsAsync(Guid ruleId, CancellationToken cancellationToken = default)
    {
        return await _context.DiscountRuleCustomers
            .AsNoTracking()
            .Where(c => c.DiscountRuleId == ruleId && !c.IsDeleted)
            .Select(c => c.CustomerId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets existing product IDs for a discount rule
    /// </summary>
    public async Task<List<Guid>> GetRuleProductIdsAsync(Guid ruleId, CancellationToken cancellationToken = default)
    {
        return await _context.DiscountRuleProducts
            .AsNoTracking()
            .Where(p => p.DiscountRuleId == ruleId && !p.IsDeleted)
            .Select(p => p.ProductId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets existing customer tier values for a discount rule
    /// </summary>
    public async Task<List<PriceTier>> GetRuleCustomerTiersAsync(Guid ruleId, CancellationToken cancellationToken = default)
    {
        return await _context.DiscountRuleCustomerTiers
            .AsNoTracking()
            .Where(t => t.DiscountRuleId == ruleId && !t.IsDeleted)
            .Select(t => t.PriceTier)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Replaces category targets for a discount rule (removes existing, adds new)
    /// </summary>
    public async Task ReplaceRuleCategoriesAsync(Guid ruleId, IEnumerable<DiscountRuleCategory> categories, CancellationToken cancellationToken = default)
    {
        // Remove existing (hard delete for junction table)
        var existing = await _context.DiscountRuleCategories
            .Where(c => c.DiscountRuleId == ruleId)
            .ToListAsync(cancellationToken);
        _context.DiscountRuleCategories.RemoveRange(existing);

        // Add new
        await _context.DiscountRuleCategories.AddRangeAsync(categories, cancellationToken);
    }

    /// <summary>
    /// Replaces brand targets for a discount rule (removes existing, adds new)
    /// </summary>
    public async Task ReplaceRuleBrandsAsync(Guid ruleId, IEnumerable<DiscountRuleBrand> brands, CancellationToken cancellationToken = default)
    {
        // Remove existing (hard delete for junction table)
        var existing = await _context.DiscountRuleBrands
            .Where(b => b.DiscountRuleId == ruleId)
            .ToListAsync(cancellationToken);
        _context.DiscountRuleBrands.RemoveRange(existing);

        // Add new
        await _context.DiscountRuleBrands.AddRangeAsync(brands, cancellationToken);
    }

    /// <summary>
    /// Replaces customer targets for a discount rule (removes existing, adds new)
    /// </summary>
    public async Task ReplaceRuleCustomersAsync(Guid ruleId, IEnumerable<DiscountRuleCustomer> customers, CancellationToken cancellationToken = default)
    {
        // Remove existing (hard delete for junction table)
        var existing = await _context.DiscountRuleCustomers
            .Where(c => c.DiscountRuleId == ruleId)
            .ToListAsync(cancellationToken);
        _context.DiscountRuleCustomers.RemoveRange(existing);

        // Add new
        await _context.DiscountRuleCustomers.AddRangeAsync(customers, cancellationToken);
    }

    /// <summary>
    /// Replaces product targets for a discount rule (removes existing, adds new)
    /// </summary>
    public async Task ReplaceRuleProductsAsync(Guid ruleId, IEnumerable<DiscountRuleProduct> products, CancellationToken cancellationToken = default)
    {
        // Remove existing (hard delete for junction table)
        var existing = await _context.DiscountRuleProducts
            .Where(p => p.DiscountRuleId == ruleId)
            .ToListAsync(cancellationToken);
        _context.DiscountRuleProducts.RemoveRange(existing);

        // Add new
        await _context.DiscountRuleProducts.AddRangeAsync(products, cancellationToken);
    }

    /// <summary>
    /// Replaces customer tier targets for a discount rule (removes existing, adds new)
    /// </summary>
    public async Task ReplaceRuleCustomerTiersAsync(Guid ruleId, IEnumerable<DiscountRuleCustomerTier> tiers, CancellationToken cancellationToken = default)
    {
        // Remove existing (hard delete for junction table)
        var existing = await _context.DiscountRuleCustomerTiers
            .Where(t => t.DiscountRuleId == ruleId)
            .ToListAsync(cancellationToken);
        _context.DiscountRuleCustomerTiers.RemoveRange(existing);

        // Add new
        await _context.DiscountRuleCustomerTiers.AddRangeAsync(tiers, cancellationToken);
    }
}
