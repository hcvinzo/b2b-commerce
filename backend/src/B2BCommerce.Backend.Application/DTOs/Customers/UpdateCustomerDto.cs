namespace B2BCommerce.Backend.Application.DTOs.Customers;

/// <summary>
/// Data transfer object for updating customer information
/// </summary>
public class UpdateCustomerDto
{
    /// <summary>
    /// Company name
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

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
    /// Preferred language
    /// </summary>
    public string PreferredLanguage { get; set; } = string.Empty;
}
