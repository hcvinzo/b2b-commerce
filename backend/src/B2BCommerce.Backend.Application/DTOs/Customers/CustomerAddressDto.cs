namespace B2BCommerce.Backend.Application.DTOs.Customers;

/// <summary>
/// Customer address data transfer object for output
/// </summary>
public class CustomerAddressDto
{
    /// <summary>
    /// Address identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Customer identifier
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Address title/name
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Full name of the person at this address
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Address type (Billing, Shipping, Contact)
    /// </summary>
    public string AddressType { get; set; } = string.Empty;

    /// <summary>
    /// Full address text
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// GeoLocation reference ID
    /// </summary>
    public Guid? GeoLocationId { get; set; }

    /// <summary>
    /// GeoLocation path name (e.g., "Türkiye/İstanbul/Kadıköy")
    /// </summary>
    public string? GeoLocationPathName { get; set; }

    /// <summary>
    /// Postal code (Posta Kodu)
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Business phone number
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Phone extension
    /// </summary>
    public string? PhoneExt { get; set; }

    /// <summary>
    /// Mobile phone number
    /// </summary>
    public string? Gsm { get; set; }

    /// <summary>
    /// Tax number for this address
    /// </summary>
    public string? TaxNo { get; set; }

    /// <summary>
    /// Whether this is the default address for the customer
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Whether this address is active
    /// </summary>
    public bool IsActive { get; set; }
}
