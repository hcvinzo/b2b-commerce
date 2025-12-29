namespace B2BCommerce.Backend.Domain.Enums;

/// <summary>
/// Status of a customer in the approval workflow
/// </summary>
public enum CustomerStatus
{
    /// <summary>
    /// Initial status when customer registers
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Customer has submitted application for review
    /// </summary>
    Applied = 2,

    /// <summary>
    /// Application rejected by admin
    /// </summary>
    Rejected = 3,

    /// <summary>
    /// Customer account suspended
    /// </summary>
    Suspended = 4,

    /// <summary>
    /// Customer is active and approved
    /// </summary>
    Active = 5
}
