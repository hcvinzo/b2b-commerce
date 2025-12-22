namespace B2BCommerce.Backend.Application.DTOs.Customers;

/// <summary>
/// Data transfer object for updating customer information
/// </summary>
public class UpdateCustomerDto
{
    // Company Information

    /// <summary>
    /// Company name (Firma Ünvanı)
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Trade name / Short company name (Firma Kısa Adı)
    /// </summary>
    public string TradeName { get; set; } = string.Empty;

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

    // Contact Information

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
    /// Contact person name (Yetkili Adı Soyadı)
    /// </summary>
    public string ContactPersonName { get; set; } = string.Empty;

    /// <summary>
    /// Contact person title (Yetkili Ünvanı)
    /// </summary>
    public string ContactPersonTitle { get; set; } = string.Empty;

    // Preferences

    /// <summary>
    /// Preferred language
    /// </summary>
    public string PreferredLanguage { get; set; } = string.Empty;
}
