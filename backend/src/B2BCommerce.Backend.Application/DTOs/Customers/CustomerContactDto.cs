namespace B2BCommerce.Backend.Application.DTOs.Customers;

/// <summary>
/// Customer contact data transfer object for output
/// </summary>
public class CustomerContactDto
{
    /// <summary>
    /// Contact identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Customer identifier
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Contact's first name (Adı)
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Contact's last name (Soyadı)
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Contact's full name
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Contact's email address (E-Posta)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Contact's position/job title (Görevi)
    /// </summary>
    public string? Position { get; set; }

    /// <summary>
    /// Contact's date of birth (Doğum Tarihi)
    /// </summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// Contact's gender (Cinsiyet)
    /// </summary>
    public string Gender { get; set; } = string.Empty;

    /// <summary>
    /// Business phone number (İş Telefon)
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Phone extension (İş Telefon Dahili)
    /// </summary>
    public string? PhoneExt { get; set; }

    /// <summary>
    /// Mobile phone number (Mobil)
    /// </summary>
    public string? Gsm { get; set; }

    /// <summary>
    /// Whether this is the primary contact for the customer
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Whether this contact is active
    /// </summary>
    public bool IsActive { get; set; }
}
