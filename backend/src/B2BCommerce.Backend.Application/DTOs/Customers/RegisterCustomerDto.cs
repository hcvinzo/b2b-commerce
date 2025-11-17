namespace B2BCommerce.Backend.Application.DTOs.Customers;

/// <summary>
/// Data transfer object for customer registration
/// </summary>
public class RegisterCustomerDto
{
    /// <summary>
    /// Company name
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Tax number (must be unique)
    /// </summary>
    public string TaxNumber { get; set; } = string.Empty;

    /// <summary>
    /// Email address (must be unique)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Phone number
    /// </summary>
    public string Phone { get; set; } = string.Empty;

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
    /// Requested credit limit amount
    /// </summary>
    public decimal CreditLimit { get; set; }

    /// <summary>
    /// Currency code (e.g., USD, EUR, TRY)
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Customer type (optional, defaults to Standard)
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Password for authentication
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
