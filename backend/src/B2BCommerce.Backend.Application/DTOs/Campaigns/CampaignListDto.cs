using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.DTOs.Campaigns;

/// <summary>
/// Campaign list item for paginated responses
/// </summary>
public class CampaignListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public CampaignStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public int Priority { get; set; }

    // Budget summary
    public decimal? TotalBudgetLimitAmount { get; set; }
    public string? TotalBudgetLimitCurrency { get; set; }
    public decimal TotalDiscountUsedAmount { get; set; }
    public string TotalDiscountUsedCurrency { get; set; } = "TRY";
    public int TotalUsageCount { get; set; }
    public int? TotalUsageLimit { get; set; }

    // Counts
    public int DiscountRuleCount { get; set; }

    // External entity fields
    public string? ExternalCode { get; set; }
    public string? ExternalId { get; set; }
    public DateTime? LastSyncedAt { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
}
