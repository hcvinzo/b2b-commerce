using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.Exceptions;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Represents a contact person for a customer
/// </summary>
public class CustomerContact : BaseEntity
{
    /// <summary>
    /// FK to the customer
    /// </summary>
    public Guid CustomerId { get; private set; }

    /// <summary>
    /// Contact's first name (Adı)
    /// </summary>
    public string FirstName { get; private set; }

    /// <summary>
    /// Contact's last name (Soyadı)
    /// </summary>
    public string LastName { get; private set; }

    /// <summary>
    /// Contact's email address (E-Posta)
    /// </summary>
    public string? Email { get; private set; }

    /// <summary>
    /// Contact's position/job title (Görevi)
    /// </summary>
    public string? Position { get; private set; }

    /// <summary>
    /// Contact's date of birth (Doğum Tarihi)
    /// </summary>
    public DateOnly? DateOfBirth { get; private set; }

    /// <summary>
    /// Contact's gender (Cinsiyet)
    /// </summary>
    public Gender Gender { get; private set; }

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
    /// Whether this is the primary contact for the customer
    /// </summary>
    public bool IsPrimary { get; private set; }

    /// <summary>
    /// Whether this contact is active
    /// </summary>
    public bool IsActive { get; private set; }

    // Navigation property
    public Customer Customer { get; private set; } = null!;

    private CustomerContact() // For EF Core
    {
        FirstName = string.Empty;
        LastName = string.Empty;
    }

    /// <summary>
    /// Creates a new CustomerContact instance
    /// </summary>
    public static CustomerContact Create(
        Guid customerId,
        string firstName,
        string lastName,
        string? email = null,
        string? position = null,
        DateOnly? dateOfBirth = null,
        Gender gender = Gender.Unknown,
        string? phone = null,
        string? phoneExt = null,
        string? gsm = null,
        bool isPrimary = false)
    {
        if (customerId == Guid.Empty)
        {
            throw new DomainException("CustomerId is required");
        }

        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new DomainException("First name is required");
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new DomainException("Last name is required");
        }

        return new CustomerContact
        {
            CustomerId = customerId,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email?.Trim(),
            Position = position?.Trim(),
            DateOfBirth = dateOfBirth,
            Gender = gender,
            Phone = phone?.Trim(),
            PhoneExt = phoneExt?.Trim(),
            Gsm = gsm?.Trim(),
            IsPrimary = isPrimary,
            IsActive = true
        };
    }

    /// <summary>
    /// Updates the contact details
    /// </summary>
    public void Update(
        string firstName,
        string lastName,
        string? email,
        string? position,
        DateOnly? dateOfBirth,
        Gender gender,
        string? phone,
        string? phoneExt,
        string? gsm)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new DomainException("First name is required");
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new DomainException("Last name is required");
        }

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email?.Trim();
        Position = position?.Trim();
        DateOfBirth = dateOfBirth;
        Gender = gender;
        Phone = phone?.Trim();
        PhoneExt = phoneExt?.Trim();
        Gsm = gsm?.Trim();
    }

    /// <summary>
    /// Gets the full name of the contact
    /// </summary>
    public string GetFullName() => $"{FirstName} {LastName}";

    /// <summary>
    /// Sets this contact as the primary contact
    /// </summary>
    public void SetAsPrimary()
    {
        IsPrimary = true;
    }

    /// <summary>
    /// Removes the primary status from this contact
    /// </summary>
    public void UnsetPrimary()
    {
        IsPrimary = false;
    }

    /// <summary>
    /// Activates the contact
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates the contact
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }
}
