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
    /// External ID for ERP integration
    /// </summary>
    public string? ExternalId { get; set; }

    /// <summary>
    /// External code for ERP integration
    /// </summary>
    public string? ExternalCode { get; set; }

    /// <summary>
    /// Company title/name (Ünvan)
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Tax office name (Vergi Dairesi)
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
    /// Website URL
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// Current status in approval workflow
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Document URLs (JSON array)
    /// </summary>
    public string? DocumentUrls { get; set; }

    /// <summary>
    /// Customer contacts
    /// </summary>
    public List<CustomerContactDto> Contacts { get; set; } = new();

    /// <summary>
    /// Customer addresses
    /// </summary>
    public List<CustomerAddressDto> Addresses { get; set; } = new();

    /// <summary>
    /// Customer attributes
    /// </summary>
    public List<CustomerAttributeDto> Attributes { get; set; } = new();

    /// <summary>
    /// Date created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Last sync date from external system
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }
}
