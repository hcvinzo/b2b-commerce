namespace B2BCommerce.Backend.Application.DTOs.Campaigns;

/// <summary>
/// DTO for updating an existing campaign
/// </summary>
public class UpdateCampaignDto
{
    /// <summary>
    /// Campaign name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Campaign description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// When the campaign starts
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// When the campaign ends
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Priority for conflict resolution (higher = more priority)
    /// </summary>
    public int? Priority { get; set; }

    /// <summary>
    /// Maximum total discount amount (optional)
    /// </summary>
    public decimal? TotalBudgetLimitAmount { get; set; }

    /// <summary>
    /// Currency for budget limits
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Maximum total usage count (optional)
    /// </summary>
    public int? TotalUsageLimit { get; set; }

    /// <summary>
    /// Maximum discount per customer (optional)
    /// </summary>
    public decimal? PerCustomerBudgetLimitAmount { get; set; }

    /// <summary>
    /// Maximum usage count per customer (optional)
    /// </summary>
    public int? PerCustomerUsageLimit { get; set; }
}
