using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.Exceptions;
using B2BCommerce.Backend.Domain.ValueObjects;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Discount campaign entity - aggregate root for campaign-based discounts.
/// Campaigns have time-limited promotional periods with discount rules.
/// </summary>
public class Campaign : ExternalEntity, IAggregateRoot
{
    /// <summary>
    /// Campaign name
    /// </summary>
    public string Name { get; protected set; } = null!;

    /// <summary>
    /// Campaign description
    /// </summary>
    public string? Description { get; protected set; }

    /// <summary>
    /// When the campaign starts
    /// </summary>
    public DateTime StartDate { get; protected set; }

    /// <summary>
    /// When the campaign ends
    /// </summary>
    public DateTime EndDate { get; protected set; }

    /// <summary>
    /// Current status of the campaign
    /// </summary>
    public CampaignStatus Status { get; protected set; }

    /// <summary>
    /// Priority for conflict resolution (higher = more priority)
    /// </summary>
    public int Priority { get; protected set; }

    /// <summary>
    /// Maximum total discount amount for the entire campaign (optional)
    /// </summary>
    public Money? TotalBudgetLimit { get; protected set; }

    /// <summary>
    /// Maximum number of times the campaign can be used (optional)
    /// </summary>
    public int? TotalUsageLimit { get; protected set; }

    /// <summary>
    /// Maximum discount amount per customer (optional)
    /// </summary>
    public Money? PerCustomerBudgetLimit { get; protected set; }

    /// <summary>
    /// Maximum number of uses per customer (optional)
    /// </summary>
    public int? PerCustomerUsageLimit { get; protected set; }

    /// <summary>
    /// Total discount amount used so far
    /// </summary>
    public Money TotalDiscountUsed { get; protected set; } = null!;

    /// <summary>
    /// Total number of times the campaign has been used
    /// </summary>
    public int TotalUsageCount { get; protected set; }

    /// <summary>
    /// Discount rules for this campaign
    /// </summary>
    public ICollection<DiscountRule> DiscountRules { get; protected set; } = new List<DiscountRule>();

    /// <summary>
    /// Usage records for this campaign
    /// </summary>
    public ICollection<CampaignUsage> Usages { get; protected set; } = new List<CampaignUsage>();

    private Campaign()
    {
    }

    /// <summary>
    /// Creates a new campaign in Draft status
    /// </summary>
    public static Campaign Create(
        string name,
        DateTime startDate,
        DateTime endDate,
        string? description = null,
        int priority = 0,
        Money? totalBudgetLimit = null,
        int? totalUsageLimit = null,
        Money? perCustomerBudgetLimit = null,
        int? perCustomerUsageLimit = null,
        string currency = "TRY")
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Campaign name is required");
        }

        if (endDate <= startDate)
        {
            throw new DomainException("End date must be after start date");
        }

        var campaign = new Campaign
        {
            Name = name,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            Status = CampaignStatus.Draft,
            Priority = priority,
            TotalBudgetLimit = totalBudgetLimit,
            TotalUsageLimit = totalUsageLimit,
            PerCustomerBudgetLimit = perCustomerBudgetLimit,
            PerCustomerUsageLimit = perCustomerUsageLimit,
            TotalDiscountUsed = Money.Zero(currency),
            TotalUsageCount = 0
        };

        return campaign;
    }

    /// <summary>
    /// Creates a campaign from external system (ERP sync)
    /// </summary>
    public static Campaign CreateFromExternal(
        string externalId,
        string name,
        DateTime startDate,
        DateTime endDate,
        string? description = null,
        int priority = 0,
        Money? totalBudgetLimit = null,
        int? totalUsageLimit = null,
        Money? perCustomerBudgetLimit = null,
        int? perCustomerUsageLimit = null,
        string? externalCode = null,
        Guid? specificId = null,
        string currency = "TRY")
    {
        var campaign = Create(
            name,
            startDate,
            endDate,
            description,
            priority,
            totalBudgetLimit,
            totalUsageLimit,
            perCustomerBudgetLimit,
            perCustomerUsageLimit,
            currency);

        if (specificId.HasValue)
        {
            campaign.Id = specificId.Value;
        }

        InitializeFromExternal(campaign, externalId, externalCode);

        return campaign;
    }

    /// <summary>
    /// Updates campaign details (only allowed in Draft status)
    /// </summary>
    public void Update(
        string name,
        DateTime startDate,
        DateTime endDate,
        string? description = null,
        int? priority = null,
        Money? totalBudgetLimit = null,
        int? totalUsageLimit = null,
        Money? perCustomerBudgetLimit = null,
        int? perCustomerUsageLimit = null)
    {
        if (Status != CampaignStatus.Draft && Status != CampaignStatus.Scheduled)
        {
            throw new InvalidOperationDomainException($"Cannot update campaign in {Status} status");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Campaign name is required");
        }

        if (endDate <= startDate)
        {
            throw new DomainException("End date must be after start date");
        }

        Name = name;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;

        if (priority.HasValue)
        {
            Priority = priority.Value;
        }

        TotalBudgetLimit = totalBudgetLimit;
        TotalUsageLimit = totalUsageLimit;
        PerCustomerBudgetLimit = perCustomerBudgetLimit;
        PerCustomerUsageLimit = perCustomerUsageLimit;
    }

    /// <summary>
    /// Schedules the campaign (Draft → Scheduled)
    /// </summary>
    public void Schedule()
    {
        if (Status != CampaignStatus.Draft)
        {
            throw new InvalidOperationDomainException($"Cannot schedule campaign in {Status} status. Campaign must be in Draft status.");
        }

        if (!DiscountRules.Any())
        {
            throw new InvalidOperationDomainException("Cannot schedule campaign without discount rules");
        }

        Status = CampaignStatus.Scheduled;
    }

    /// <summary>
    /// Activates the campaign (Scheduled or Paused → Active)
    /// </summary>
    public void Activate()
    {
        if (Status != CampaignStatus.Scheduled && Status != CampaignStatus.Paused)
        {
            throw new InvalidOperationDomainException($"Cannot activate campaign in {Status} status. Campaign must be in Scheduled or Paused status.");
        }

        Status = CampaignStatus.Active;
    }

    /// <summary>
    /// Pauses the campaign (Scheduled or Active → Paused)
    /// </summary>
    public void Pause()
    {
        if (Status != CampaignStatus.Scheduled && Status != CampaignStatus.Active)
        {
            throw new InvalidOperationDomainException($"Cannot pause campaign in {Status} status. Campaign must be in Scheduled or Active status.");
        }

        Status = CampaignStatus.Paused;
    }

    /// <summary>
    /// Ends the campaign (Active → Ended)
    /// </summary>
    public void End()
    {
        if (Status != CampaignStatus.Active)
        {
            throw new InvalidOperationDomainException($"Cannot end campaign in {Status} status. Campaign must be Active.");
        }

        Status = CampaignStatus.Ended;
    }

    /// <summary>
    /// Cancels the campaign (any status except Ended → Cancelled)
    /// </summary>
    public void Cancel()
    {
        if (Status == CampaignStatus.Ended)
        {
            throw new InvalidOperationDomainException("Cannot cancel a campaign that has already ended");
        }

        if (Status == CampaignStatus.Cancelled)
        {
            throw new InvalidOperationDomainException("Campaign is already cancelled");
        }

        Status = CampaignStatus.Cancelled;
    }

    /// <summary>
    /// Checks if the campaign is currently applicable (active and within date range)
    /// </summary>
    public bool IsApplicable(DateTime currentTime)
    {
        return Status == CampaignStatus.Active &&
               currentTime >= StartDate &&
               currentTime <= EndDate;
    }

    /// <summary>
    /// Checks if the campaign has budget for the given discount amount
    /// </summary>
    public bool HasBudgetFor(Money discountAmount)
    {
        // Check total budget limit
        if (TotalBudgetLimit is not null)
        {
            var projectedTotal = TotalDiscountUsed + discountAmount;
            if (projectedTotal > TotalBudgetLimit)
            {
                return false;
            }
        }

        // Check total usage limit
        if (TotalUsageLimit.HasValue && TotalUsageCount >= TotalUsageLimit.Value)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if the campaign has budget for a specific customer
    /// </summary>
    public bool HasCustomerBudgetFor(Money discountAmount, int customerUsageCount, Money customerTotalDiscount)
    {
        // Check per-customer budget limit
        if (PerCustomerBudgetLimit is not null)
        {
            var projectedCustomerTotal = customerTotalDiscount + discountAmount;
            if (projectedCustomerTotal > PerCustomerBudgetLimit)
            {
                return false;
            }
        }

        // Check per-customer usage limit
        if (PerCustomerUsageLimit.HasValue && customerUsageCount >= PerCustomerUsageLimit.Value)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Records usage of the campaign
    /// </summary>
    public void RecordUsage(Money discountAmount)
    {
        TotalDiscountUsed = TotalDiscountUsed + discountAmount;
        TotalUsageCount++;
    }

    /// <summary>
    /// Reverses a usage record (e.g., when an order is cancelled)
    /// </summary>
    public void ReverseUsage(Money discountAmount)
    {
        if (TotalUsageCount > 0)
        {
            TotalUsageCount--;
        }

        if (TotalDiscountUsed.Amount >= discountAmount.Amount)
        {
            TotalDiscountUsed = TotalDiscountUsed - discountAmount;
        }
        else
        {
            TotalDiscountUsed = Money.Zero(TotalDiscountUsed.Currency);
        }
    }

    /// <summary>
    /// Adds a discount rule to the campaign
    /// </summary>
    public void AddDiscountRule(DiscountRule rule)
    {
        if (Status != CampaignStatus.Draft)
        {
            throw new InvalidOperationDomainException($"Cannot add rules to campaign in {Status} status. Campaign must be in Draft status.");
        }

        DiscountRules.Add(rule);
    }

    /// <summary>
    /// Removes a discount rule from the campaign
    /// </summary>
    public void RemoveDiscountRule(DiscountRule rule)
    {
        if (Status != CampaignStatus.Draft)
        {
            throw new InvalidOperationDomainException($"Cannot remove rules from campaign in {Status} status. Campaign must be in Draft status.");
        }

        DiscountRules.Remove(rule);
    }

    /// <summary>
    /// Gets the remaining budget for the campaign
    /// </summary>
    public Money? GetRemainingBudget()
    {
        if (TotalBudgetLimit is null)
        {
            return null;
        }

        return TotalBudgetLimit - TotalDiscountUsed;
    }
}
