using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.Exceptions;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Customer address entity supporting multiple addresses per customer
/// </summary>
public class CustomerAddress : BaseEntity
{
    /// <summary>
    /// FK to the customer
    /// </summary>
    public Guid CustomerId { get; private set; }

    /// <summary>
    /// Address type (Billing, Shipping, Contact)
    /// </summary>
    public CustomerAddressType AddressType { get; private set; }

    /// <summary>
    /// Address title/label (Adres Başlığı)
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Full name for this address (Ad Soyad)
    /// </summary>
    public string? FullName { get; private set; }

    /// <summary>
    /// Full address text (Adres)
    /// </summary>
    public string Address { get; private set; }

    /// <summary>
    /// FK to geographic location
    /// </summary>
    public Guid? GeoLocationId { get; private set; }

    /// <summary>
    /// Postal/ZIP code (Posta Kodu)
    /// </summary>
    public string? PostalCode { get; private set; }

    /// <summary>
    /// Business phone number (İş Telefon)
    /// </summary>
    public string? Phone { get; private set; }

    /// <summary>
    /// Phone extension (İş Telefon Dahili)
    /// </summary>
    public string? PhoneExt { get; private set; }

    /// <summary>
    /// Mobile phone number (Mobil)
    /// </summary>
    public string? Gsm { get; private set; }

    /// <summary>
    /// Tax number for billing address (Vergi No)
    /// </summary>
    public string? TaxNo { get; private set; }

    /// <summary>
    /// Whether this is the default address for its type
    /// </summary>
    public bool IsDefault { get; private set; }

    /// <summary>
    /// Whether this address is active
    /// </summary>
    public bool IsActive { get; private set; }

    // Navigation properties
    public Customer Customer { get; private set; } = null!;
    public GeoLocation? GeoLocation { get; private set; }

    private CustomerAddress() // For EF Core
    {
        Title = string.Empty;
        Address = string.Empty;
    }

    /// <summary>
    /// Creates a new CustomerAddress instance
    /// </summary>
    public static CustomerAddress Create(
        Guid customerId,
        string title,
        CustomerAddressType addressType,
        string address,
        string? fullName = null,
        Guid? geoLocationId = null,
        string? postalCode = null,
        string? phone = null,
        string? phoneExt = null,
        string? gsm = null,
        string? taxNo = null,
        bool isDefault = false)
    {
        if (customerId == Guid.Empty)
        {
            throw new DomainException("CustomerId is required");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Address title is required");
        }

        if (string.IsNullOrWhiteSpace(address))
        {
            throw new DomainException("Address is required");
        }

        return new CustomerAddress
        {
            CustomerId = customerId,
            Title = title.Trim(),
            AddressType = addressType,
            Address = address.Trim(),
            FullName = fullName?.Trim(),
            GeoLocationId = geoLocationId,
            PostalCode = postalCode?.Trim(),
            Phone = phone?.Trim(),
            PhoneExt = phoneExt?.Trim(),
            Gsm = gsm?.Trim(),
            TaxNo = taxNo?.Trim(),
            IsDefault = isDefault,
            IsActive = true
        };
    }

    /// <summary>
    /// Updates the address details
    /// </summary>
    public void Update(
        string title,
        string address,
        string? fullName,
        Guid? geoLocationId,
        string? postalCode,
        string? phone,
        string? phoneExt,
        string? gsm,
        string? taxNo)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Address title is required");
        }

        if (string.IsNullOrWhiteSpace(address))
        {
            throw new DomainException("Address is required");
        }

        Title = title.Trim();
        Address = address.Trim();
        FullName = fullName?.Trim();
        GeoLocationId = geoLocationId;
        PostalCode = postalCode?.Trim();
        Phone = phone?.Trim();
        PhoneExt = phoneExt?.Trim();
        Gsm = gsm?.Trim();
        TaxNo = taxNo?.Trim();
    }

    /// <summary>
    /// Updates the address type
    /// </summary>
    public void UpdateAddressType(CustomerAddressType addressType)
    {
        AddressType = addressType;
    }

    /// <summary>
    /// Sets this address as the default for its type
    /// </summary>
    public void SetAsDefault()
    {
        IsDefault = true;
    }

    /// <summary>
    /// Removes the default status from this address
    /// </summary>
    public void UnsetDefault()
    {
        IsDefault = false;
    }

    /// <summary>
    /// Activates the address
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates the address
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }
}
