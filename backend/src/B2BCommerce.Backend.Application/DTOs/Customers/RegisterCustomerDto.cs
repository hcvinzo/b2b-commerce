namespace B2BCommerce.Backend.Application.DTOs.Customers;

/// <summary>
/// Data transfer object for customer registration
/// </summary>
public class RegisterCustomerDto
{
    // Step 1: Company Information (Firma Bilgileri)

    /// <summary>
    /// Company name (Firma Ünvanı)
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Trade name / Short company name (Firma Kısa Adı)
    /// </summary>
    public string TradeName { get; set; } = string.Empty;

    /// <summary>
    /// Tax number (Vergi Numarası) - must be unique
    /// </summary>
    public string TaxNumber { get; set; } = string.Empty;

    /// <summary>
    /// Tax office (Vergi Dairesi)
    /// </summary>
    public string TaxOffice { get; set; } = string.Empty;

    /// <summary>
    /// MERSIS number (Mersis No) - optional
    /// </summary>
    public string? MersisNo { get; set; }

    /// <summary>
    /// Identity number (TC Kimlik No / Kimlik Numarası) - optional for individual companies
    /// </summary>
    public string? IdentityNo { get; set; }

    /// <summary>
    /// Trade registry number (Ticaret Sicil No) - optional
    /// </summary>
    public string? TradeRegistryNo { get; set; }

    // Step 2: Contact Information (İletişim Bilgileri)

    /// <summary>
    /// Contact person name (Yetkili Adı Soyadı)
    /// </summary>
    public string ContactPersonName { get; set; } = string.Empty;

    /// <summary>
    /// Contact person title (Yetkili Ünvanı)
    /// </summary>
    public string ContactPersonTitle { get; set; } = string.Empty;

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
    /// Email address (E-posta) - must be unique
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Website URL (Web Sitesi)
    /// </summary>
    public string? Website { get; set; }

    // Step 3: Address Information (Adres Bilgileri)

    // Shipping Address (Teslimat Adresi)

    /// <summary>
    /// Shipping address country (Ülke)
    /// </summary>
    public string ShippingCountry { get; set; } = string.Empty;

    /// <summary>
    /// Shipping address city/state (İl)
    /// </summary>
    public string ShippingCity { get; set; } = string.Empty;

    /// <summary>
    /// Shipping address district (İlçe)
    /// </summary>
    public string ShippingDistrict { get; set; } = string.Empty;

    /// <summary>
    /// Shipping address neighborhood (Mahalle)
    /// </summary>
    public string? ShippingNeighborhood { get; set; }

    /// <summary>
    /// Shipping address street (Adres)
    /// </summary>
    public string ShippingStreet { get; set; } = string.Empty;

    /// <summary>
    /// Shipping address postal code (Posta Kodu)
    /// </summary>
    public string ShippingPostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Shipping address state/province (Bölge) - kept for compatibility
    /// </summary>
    public string ShippingState { get; set; } = string.Empty;

    // Billing Address (Fatura Adresi)

    /// <summary>
    /// Use shipping address as billing address (Fatura adresi teslimat adresi ile aynı)
    /// </summary>
    public bool UseSameAddressForBilling { get; set; } = true;

    /// <summary>
    /// Billing address country (Ülke)
    /// </summary>
    public string BillingCountry { get; set; } = string.Empty;

    /// <summary>
    /// Billing address city/state (İl)
    /// </summary>
    public string BillingCity { get; set; } = string.Empty;

    /// <summary>
    /// Billing address district (İlçe)
    /// </summary>
    public string BillingDistrict { get; set; } = string.Empty;

    /// <summary>
    /// Billing address neighborhood (Mahalle)
    /// </summary>
    public string? BillingNeighborhood { get; set; }

    /// <summary>
    /// Billing address street (Adres)
    /// </summary>
    public string BillingStreet { get; set; } = string.Empty;

    /// <summary>
    /// Billing address postal code (Posta Kodu)
    /// </summary>
    public string BillingPostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Billing address state/province (Bölge) - kept for compatibility
    /// </summary>
    public string BillingState { get; set; } = string.Empty;

    // Step 4: Membership Information (Üyelik Bilgileri)

    /// <summary>
    /// Username for authentication (Kullanıcı Adı)
    /// </summary>
    public string Username { get; set; } = string.Empty;

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

    // Optional fields

    /// <summary>
    /// Requested credit limit amount
    /// </summary>
    public decimal CreditLimit { get; set; }

    /// <summary>
    /// Currency code (e.g., USD, EUR, TRY)
    /// </summary>
    public string Currency { get; set; } = "TRY";

    /// <summary>
    /// Customer type (optional, defaults to Standard)
    /// </summary>
    public string? Type { get; set; }
}
