namespace B2BCommerce.Backend.Application.DTOs.Customers;

/// <summary>
/// Data transfer object for customer registration
/// </summary>
public class RegisterCustomerDto
{
    // Step 1: Company Information (Firma Bilgileri)

    /// <summary>
    /// Company title/name (Ünvan)
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Tax office (Vergi Dairesi)
    /// </summary>
    public string? TaxOffice { get; set; }

    /// <summary>
    /// Tax number (Vergi Numarası)
    /// </summary>
    public string? TaxNo { get; set; }

    /// <summary>
    /// Year of establishment (Kuruluş Yılı)
    /// </summary>
    public int? EstablishmentYear { get; set; }

    /// <summary>
    /// Website URL (Web Sitesi)
    /// </summary>
    public string? Website { get; set; }

    // Step 2: Primary Contact Information (İletişim Bilgileri)

    /// <summary>
    /// Contact first name (Ad)
    /// </summary>
    public string ContactFirstName { get; set; } = string.Empty;

    /// <summary>
    /// Contact last name (Soyad)
    /// </summary>
    public string ContactLastName { get; set; } = string.Empty;

    /// <summary>
    /// Contact email address (E-posta)
    /// </summary>
    public string ContactEmail { get; set; } = string.Empty;

    /// <summary>
    /// Contact position/title (Pozisyon)
    /// </summary>
    public string? ContactPosition { get; set; }

    /// <summary>
    /// Contact date of birth (Doğum Tarihi)
    /// </summary>
    public DateTime? ContactDateOfBirth { get; set; }

    /// <summary>
    /// Contact gender (Cinsiyet): Male, Female, Unknown
    /// </summary>
    public string? ContactGender { get; set; }

    /// <summary>
    /// Contact phone number (Telefon)
    /// </summary>
    public string? ContactPhone { get; set; }

    /// <summary>
    /// Contact phone extension (Dahili)
    /// </summary>
    public string? ContactPhoneExt { get; set; }

    /// <summary>
    /// Contact mobile phone (Mobil)
    /// </summary>
    public string? ContactGsm { get; set; }

    // Step 3: Primary Address Information (Adres Bilgileri)

    /// <summary>
    /// Address title (Adres Başlığı)
    /// </summary>
    public string AddressTitle { get; set; } = string.Empty;

    /// <summary>
    /// Full address (Adres)
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Geographic location ID (if using structured locations)
    /// </summary>
    public Guid? GeoLocationId { get; set; }

    /// <summary>
    /// Postal code (Posta Kodu)
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Address phone number
    /// </summary>
    public string? AddressPhone { get; set; }

    /// <summary>
    /// Address phone extension
    /// </summary>
    public string? AddressPhoneExt { get; set; }

    /// <summary>
    /// Address mobile phone
    /// </summary>
    public string? AddressGsm { get; set; }

    // Step 4: User Account Information (Üyelik Bilgileri)

    /// <summary>
    /// Email address for user account (E-posta) - must be unique
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Password for authentication (Şifre)
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Password confirmation (Şifre Tekrar)
    /// </summary>
    public string PasswordConfirmation { get; set; } = string.Empty;

    /// <summary>
    /// Accept terms and conditions (Kullanım koşullarını kabul ediyorum)
    /// </summary>
    public bool AcceptTerms { get; set; }

    /// <summary>
    /// Accept KVKK/GDPR consent (KVKK metnini okudum, onaylıyorum)
    /// </summary>
    public bool AcceptKvkk { get; set; }

    // Optional: Documents and Attributes

    /// <summary>
    /// Document URLs as JSON string (stored in Customer.DocumentUrls)
    /// </summary>
    public string? DocumentUrls { get; set; }

    /// <summary>
    /// Customer attributes to save during registration (grouped by definition)
    /// </summary>
    public List<UpsertCustomerAttributesByDefinitionDto>? Attributes { get; set; }
}
