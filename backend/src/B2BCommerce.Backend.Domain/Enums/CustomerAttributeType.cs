namespace B2BCommerce.Backend.Domain.Enums;

/// <summary>
/// Types of customer attributes for storing additional B2B information
/// </summary>
public enum CustomerAttributeType
{
    /// <summary>
    /// Yetkililer & Ortaklar - Company shareholders and directors
    /// JSON: { "fullName": string, "identityNumber": string, "sharePercentage": decimal }
    /// </summary>
    ShareholderOrDirector = 1,

    /// <summary>
    /// Çalışmakta Olduğunuz Şirketler & Çalışma Koşulları - Business partners
    /// JSON: { "companyName": string, "paymentTerm": string, "creditLimitUsd": decimal }
    /// </summary>
    BusinessPartner = 2,

    /// <summary>
    /// Satışını Gerçekleştirdiğiniz Ürün Kategorileri - Product categories (checkbox selections)
    /// JSON: { "categories": string[] }
    /// </summary>
    ProductCategory = 3,

    /// <summary>
    /// Banka Hesap Bilgileri - Bank account information
    /// JSON: { "bankName": string, "iban": string }
    /// </summary>
    BankAccount = 4,

    /// <summary>
    /// Teminatlar - Collateral information
    /// JSON: { "type": string, "amount": decimal, "currency": string }
    /// </summary>
    Collateral = 5,

    /// <summary>
    /// Talep Ettiğiniz Çalışma Koşulları - Payment preferences (checkbox selections)
    /// JSON: { "preferences": string[] }
    /// </summary>
    PaymentPreference = 6
}
