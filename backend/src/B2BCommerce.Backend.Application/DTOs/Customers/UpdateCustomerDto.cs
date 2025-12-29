namespace B2BCommerce.Backend.Application.DTOs.Customers;

/// <summary>
/// Data transfer object for updating customer information
/// </summary>
public class UpdateCustomerDto
{
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
}
