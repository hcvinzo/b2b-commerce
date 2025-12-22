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
    /// Address type (Billing, Shipping, Contact)
    /// </summary>
    public string AddressType { get; set; } = string.Empty;

    /// <summary>
    /// Street address
    /// </summary>
    public string Street { get; set; } = string.Empty;

    /// <summary>
    /// District (İlçe)
    /// </summary>
    public string? District { get; set; }

    /// <summary>
    /// Neighborhood (Mahalle)
    /// </summary>
    public string? Neighborhood { get; set; }

    /// <summary>
    /// City (İl)
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// State/Province (Bölge)
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Country (Ülke)
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Postal code (Posta Kodu)
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is the default address for the customer
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Whether this address is active
    /// </summary>
    public bool IsActive { get; set; }
}
