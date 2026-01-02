using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.DTOs.Campaigns;

/// <summary>
/// Full campaign data transfer object with all details
/// </summary>
public class CampaignDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public CampaignStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public int Priority { get; set; }

    // Budget limits
    public decimal? TotalBudgetLimitAmount { get; set; }
    public string? TotalBudgetLimitCurrency { get; set; }
    public int? TotalUsageLimit { get; set; }
    public decimal? PerCustomerBudgetLimitAmount { get; set; }
    public string? PerCustomerBudgetLimitCurrency { get; set; }
    public int? PerCustomerUsageLimit { get; set; }

    // Usage tracking
    public decimal TotalDiscountUsedAmount { get; set; }
    public string TotalDiscountUsedCurrency { get; set; } = "TRY";
    public int TotalUsageCount { get; set; }

    // Discount rules
    public List<DiscountRuleDto> DiscountRules { get; set; } = new();

    // External entity fields
    public string? ExternalCode { get; set; }
    public string? ExternalId { get; set; }
    public DateTime? LastSyncedAt { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
