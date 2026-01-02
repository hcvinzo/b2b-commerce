namespace B2BCommerce.Backend.Domain.Enums;

/// <summary>
/// Status of a discount campaign
/// </summary>
public enum CampaignStatus
{
    /// <summary>
    /// Campaign is being created/edited, not yet active
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Campaign is ready and scheduled to start in the future
    /// </summary>
    Scheduled = 1,

    /// <summary>
    /// Campaign is currently active and applying discounts
    /// </summary>
    Active = 2,

    /// <summary>
    /// Campaign is temporarily paused
    /// </summary>
    Paused = 3,

    /// <summary>
    /// Campaign has ended (past end date)
    /// </summary>
    Ended = 4,

    /// <summary>
    /// Campaign was manually cancelled
    /// </summary>
    Cancelled = 5
}
