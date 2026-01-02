using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.DTOs.Campaigns;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.Interfaces.Services;

/// <summary>
/// Campaign service interface for campaign management operations
/// </summary>
public interface ICampaignService
{
    /// <summary>
    /// Gets paginated campaigns with filtering
    /// </summary>
    Task<Result<PagedResult<CampaignListDto>>> GetAllAsync(
        string? search,
        CampaignStatus? status,
        DateTime? startDateFrom,
        DateTime? startDateTo,
        int pageNumber,
        int pageSize,
        string sortBy,
        string sortDirection,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a campaign by ID with all details
    /// </summary>
    Task<Result<CampaignDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a campaign by external ID
    /// </summary>
    Task<Result<CampaignDto>> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new campaign
    /// </summary>
    Task<Result<CampaignDto>> CreateAsync(CreateCampaignDto dto, string? createdBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing campaign
    /// </summary>
    Task<Result<CampaignDto>> UpdateAsync(Guid id, UpdateCampaignDto dto, string? updatedBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a campaign (soft delete)
    /// </summary>
    Task<Result> DeleteAsync(Guid id, string? deletedBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedules a campaign (Draft → Scheduled)
    /// </summary>
    Task<Result<CampaignDto>> ScheduleAsync(Guid id, string? updatedBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates a campaign (Scheduled/Paused → Active)
    /// </summary>
    Task<Result<CampaignDto>> ActivateAsync(Guid id, string? updatedBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pauses a campaign (Scheduled/Active → Paused)
    /// </summary>
    Task<Result<CampaignDto>> PauseAsync(Guid id, string? updatedBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a campaign
    /// </summary>
    Task<Result<CampaignDto>> CancelAsync(Guid id, string? updatedBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a discount rule to a campaign
    /// </summary>
    Task<Result<DiscountRuleDto>> AddDiscountRuleAsync(Guid campaignId, CreateDiscountRuleDto dto, string? createdBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a discount rule
    /// </summary>
    Task<Result<DiscountRuleDto>> UpdateDiscountRuleAsync(Guid campaignId, Guid ruleId, UpdateDiscountRuleDto dto, string? updatedBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a discount rule from a campaign
    /// </summary>
    Task<Result> RemoveDiscountRuleAsync(Guid campaignId, Guid ruleId, string? deletedBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds products to a discount rule
    /// </summary>
    Task<Result> AddProductsToRuleAsync(Guid campaignId, Guid ruleId, IEnumerable<Guid> productIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds categories to a discount rule
    /// </summary>
    Task<Result> AddCategoriesToRuleAsync(Guid campaignId, Guid ruleId, IEnumerable<Guid> categoryIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds brands to a discount rule
    /// </summary>
    Task<Result> AddBrandsToRuleAsync(Guid campaignId, Guid ruleId, IEnumerable<Guid> brandIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds customers to a discount rule
    /// </summary>
    Task<Result> AddCustomersToRuleAsync(Guid campaignId, Guid ruleId, IEnumerable<Guid> customerIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds customer tiers to a discount rule
    /// </summary>
    Task<Result> AddCustomerTiersToRuleAsync(Guid campaignId, Guid ruleId, IEnumerable<PriceTier> tiers, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets usage statistics for a campaign
    /// </summary>
    Task<Result<CampaignUsageStatsDto>> GetUsageStatsAsync(Guid campaignId, CancellationToken cancellationToken = default);
}
