using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.Interfaces.Repositories;

/// <summary>
/// Campaign repository interface for campaign-specific operations
/// </summary>
public interface ICampaignRepository : IGenericRepository<Campaign>
{
    /// <summary>
    /// Gets a campaign by its external ID
    /// </summary>
    /// <param name="externalId">External system ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Campaign if found, null otherwise</returns>
    Task<Campaign?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a campaign exists by its external ID
    /// </summary>
    /// <param name="externalId">External system ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> ExistsByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a campaign with all its discount rules loaded
    /// </summary>
    /// <param name="id">Campaign ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Campaign with rules</returns>
    Task<Campaign?> GetWithRulesAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a campaign with all its discount rules and their targets loaded
    /// </summary>
    /// <param name="id">Campaign ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Campaign with rules and targets</returns>
    Task<Campaign?> GetWithRulesAndTargetsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active campaigns (Status = Active and within date range)
    /// </summary>
    /// <param name="currentTime">Current time for date range check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active campaigns with their rules</returns>
    Task<IEnumerable<Campaign>> GetActiveCampaignsAsync(DateTime currentTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets campaigns by status
    /// </summary>
    /// <param name="status">Campaign status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of campaigns</returns>
    Task<IEnumerable<Campaign>> GetByStatusAsync(CampaignStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets customer usage statistics for a campaign
    /// </summary>
    /// <param name="campaignId">Campaign ID</param>
    /// <param name="customerId">Customer ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Usage count and total discount amount</returns>
    Task<(int usageCount, decimal totalDiscount)> GetCustomerUsageAsync(
        Guid campaignId,
        Guid customerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all campaign usages for an order (for reversal)
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of campaign usages</returns>
    Task<IEnumerable<CampaignUsage>> GetUsagesByOrderAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a campaign usage record
    /// </summary>
    /// <param name="usage">Campaign usage</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddUsageAsync(CampaignUsage usage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a discount rule by ID with its targets loaded
    /// </summary>
    /// <param name="ruleId">Discount rule ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Discount rule with targets</returns>
    Task<DiscountRule?> GetDiscountRuleWithTargetsAsync(Guid ruleId, CancellationToken cancellationToken = default);
}
