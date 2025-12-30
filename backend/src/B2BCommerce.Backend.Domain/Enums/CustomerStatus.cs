namespace B2BCommerce.Backend.Domain.Enums;

/// <summary>
/// Status of a customer in the approval workflow
/// </summary>
public enum CustomerStatus
{
    /// <summary>
    /// Initial status - customer has applied and is waiting for approval
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Customer is approved and active
    /// </summary>
    Active = 2,

    /// <summary>
    /// Application rejected by admin
    /// </summary>
    Rejected = 3,

    /// <summary>
    /// Customer account suspended (any active customer can be suspended)
    /// </summary>
    Suspended = 4
}
