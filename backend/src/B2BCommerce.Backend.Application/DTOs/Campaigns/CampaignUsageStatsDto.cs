namespace B2BCommerce.Backend.Application.DTOs.Campaigns;

/// <summary>
/// Campaign usage statistics
/// </summary>
public class CampaignUsageStatsDto
{
    /// <summary>
    /// Campaign ID
    /// </summary>
    public Guid CampaignId { get; set; }

    /// <summary>
    /// Campaign name
    /// </summary>
    public string CampaignName { get; set; } = string.Empty;

    /// <summary>
    /// Total number of times the campaign has been used
    /// </summary>
    public int TotalUsageCount { get; set; }

    /// <summary>
    /// Total discount amount used
    /// </summary>
    public decimal TotalDiscountUsedAmount { get; set; }

    /// <summary>
    /// Currency for discount amounts
    /// </summary>
    public string Currency { get; set; } = "TRY";

    /// <summary>
    /// Number of unique customers who used the campaign
    /// </summary>
    public int UniqueCustomerCount { get; set; }

    /// <summary>
    /// Number of unique orders with this campaign
    /// </summary>
    public int UniqueOrderCount { get; set; }

    /// <summary>
    /// Total budget limit (if set)
    /// </summary>
    public decimal? TotalBudgetLimit { get; set; }

    /// <summary>
    /// Total usage limit (if set)
    /// </summary>
    public int? TotalUsageLimit { get; set; }

    /// <summary>
    /// Remaining budget (if budget limit is set)
    /// </summary>
    public decimal? RemainingBudget { get; set; }

    /// <summary>
    /// Remaining usage count (if usage limit is set)
    /// </summary>
    public int? RemainingUsageCount { get; set; }

    /// <summary>
    /// Budget utilization percentage (0-100)
    /// </summary>
    public decimal? BudgetUtilizationPercentage => TotalBudgetLimit.HasValue && TotalBudgetLimit.Value > 0
        ? Math.Round((TotalDiscountUsedAmount / TotalBudgetLimit.Value) * 100, 2)
        : null;

    /// <summary>
    /// Usage utilization percentage (0-100)
    /// </summary>
    public decimal? UsageUtilizationPercentage => TotalUsageLimit.HasValue && TotalUsageLimit.Value > 0
        ? Math.Round((decimal)TotalUsageCount / TotalUsageLimit.Value * 100, 2)
        : null;

    /// <summary>
    /// Average discount per usage
    /// </summary>
    public decimal AverageDiscountPerUsage => TotalUsageCount > 0
        ? Math.Round(TotalDiscountUsedAmount / TotalUsageCount, 2)
        : 0;

    /// <summary>
    /// Top customers by discount amount
    /// </summary>
    public List<CustomerUsageDto> TopCustomers { get; set; } = new();

    /// <summary>
    /// Recent usages
    /// </summary>
    public List<RecentUsageDto> RecentUsages { get; set; } = new();
}

/// <summary>
/// Customer usage summary
/// </summary>
public class CustomerUsageDto
{
    public Guid CustomerId { get; set; }
    public string? CustomerTitle { get; set; }
    public int UsageCount { get; set; }
    public decimal TotalDiscountAmount { get; set; }
}

/// <summary>
/// Recent usage record
/// </summary>
public class RecentUsageDto
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerTitle { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTime UsedAt { get; set; }
    public bool IsReversed { get; set; }
}
