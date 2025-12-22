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
    /// Company name (Firma Ünvanı)
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Trade name / Short company name (Firma Kısa Adı)
    /// </summary>
    public string TradeName { get; set; } = string.Empty;

    /// <summary>
    /// Tax number (Vergi Numarası)
    /// </summary>
    public string TaxNumber { get; set; } = string.Empty;

    /// <summary>
    /// Tax office (Vergi Dairesi)
    /// </summary>
    public string TaxOffice { get; set; } = string.Empty;

    /// <summary>
    /// MERSIS number (Mersis No)
    /// </summary>
    public string? MersisNo { get; set; }

    /// <summary>
    /// Identity number (TC Kimlik No / Kimlik Numarası)
    /// </summary>
    public string? IdentityNo { get; set; }

    /// <summary>
    /// Trade registry number (Ticaret Sicil No)
    /// </summary>
    public string? TradeRegistryNo { get; set; }

    /// <summary>
    /// Email address (E-posta)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Phone number (Telefon)
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Mobile phone number (Cep Telefonu)
    /// </summary>
    public string? MobilePhone { get; set; }

    /// <summary>
    /// Fax number (Faks)
    /// </summary>
    public string? Fax { get; set; }

    /// <summary>
    /// Website URL (Web Sitesi)
    /// </summary>
    public string? Website { get; set; }

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
    /// Contact person name (Yetkili Adı Soyadı)
    /// </summary>
    public string ContactPersonName { get; set; } = string.Empty;

    /// <summary>
    /// Contact person title (Yetkili Ünvanı)
    /// </summary>
    public string ContactPersonTitle { get; set; } = string.Empty;

    /// <summary>
    /// Customer addresses
    /// </summary>
    public List<CustomerAddressDto> Addresses { get; set; } = new();

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
