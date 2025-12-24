namespace B2BCommerce.Backend.Domain.Enums;

/// <summary>
/// Types of documents that customers can upload
/// </summary>
public enum CustomerDocumentType
{
    /// <summary>
    /// Vergi Levhası - Tax Certificate
    /// </summary>
    TaxCertificate = 1,

    /// <summary>
    /// İmza Sirküleri - Signature Circular
    /// </summary>
    SignatureCircular = 2,

    /// <summary>
    /// Sicil Gazetesi - Trade Registry Gazette
    /// </summary>
    TradeRegistry = 3,

    /// <summary>
    /// İş Ortağı Sözleşmesi - Partnership Agreement
    /// </summary>
    PartnershipAgreement = 4,

    /// <summary>
    /// Yetkili Kimlik Fotokopisi - Authorized Person ID Copy
    /// </summary>
    AuthorizedIdCopy = 5,

    /// <summary>
    /// Yetkili İkamet Belgesi - Authorized Person Residence Document
    /// </summary>
    AuthorizedResidenceDocument = 6
}
