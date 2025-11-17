namespace B2BCommerce.Backend.Application.DTOs.Customers;

/// <summary>
/// Customer data transfer object for output
/// </summary>
public class CustomerDto
{
    /// <summary>
    /// Customer identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Company name
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Tax number
    /// </summary>
    public string TaxNumber { get; set; } = string.Empty;

    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Phone number
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Customer type
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Price tier
    /// </summary>
    public string PriceTier { get; set; } = string.Empty;

    /// <summary>
    /// Credit limit amount
    /// </summary>
    public decimal CreditLimit { get; set; }

    /// <summary>
    /// Used credit amount
    /// </summary>
    public decimal UsedCredit { get; set; }

    /// <summary>
    /// Available credit amount
    /// </summary>
    public decimal AvailableCredit { get; set; }

    /// <summary>
    /// Currency code
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Whether customer is approved
    /// </summary>
    public bool IsApproved { get; set; }

    /// <summary>
    /// Date customer was approved
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Who approved the customer
    /// </summary>
    public string? ApprovedBy { get; set; }

    /// <summary>
    /// Contact person name
    /// </summary>
    public string ContactPersonName { get; set; } = string.Empty;

    /// <summary>
    /// Contact person title
    /// </summary>
    public string ContactPersonTitle { get; set; } = string.Empty;

    /// <summary>
    /// Billing address street
    /// </summary>
    public string BillingStreet { get; set; } = string.Empty;

    /// <summary>
    /// Billing address city
    /// </summary>
    public string BillingCity { get; set; } = string.Empty;

    /// <summary>
    /// Billing address state
    /// </summary>
    public string BillingState { get; set; } = string.Empty;

    /// <summary>
    /// Billing address country
    /// </summary>
    public string BillingCountry { get; set; } = string.Empty;

    /// <summary>
    /// Billing address postal code
    /// </summary>
    public string BillingPostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Shipping address street
    /// </summary>
    public string ShippingStreet { get; set; } = string.Empty;

    /// <summary>
    /// Shipping address city
    /// </summary>
    public string ShippingCity { get; set; } = string.Empty;

    /// <summary>
    /// Shipping address state
    /// </summary>
    public string ShippingState { get; set; } = string.Empty;

    /// <summary>
    /// Shipping address country
    /// </summary>
    public string ShippingCountry { get; set; } = string.Empty;

    /// <summary>
    /// Shipping address postal code
    /// </summary>
    public string ShippingPostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Preferred currency
    /// </summary>
    public string PreferredCurrency { get; set; } = string.Empty;

    /// <summary>
    /// Preferred language
    /// </summary>
    public string PreferredLanguage { get; set; } = string.Empty;

    /// <summary>
    /// Whether customer is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Date created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
