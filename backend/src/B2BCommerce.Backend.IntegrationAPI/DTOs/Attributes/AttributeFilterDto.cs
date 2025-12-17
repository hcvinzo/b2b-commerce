namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Attributes;

/// <summary>
/// Özellik tanımları listesi için filtreleme parametreleri
/// </summary>
public class AttributeFilterDto
{
    /// <summary>
    /// Ad veya kod'da arama yapılacak metin
    /// </summary>
    /// <example>ram</example>
    public string? Search { get; set; }

    /// <summary>
    /// Filtrelenebilirlik durumuna göre filtrele (null = tümü)
    /// </summary>
    /// <example>true</example>
    public bool? IsFilterable { get; set; }

    /// <summary>
    /// Özellik tipine göre filtrele (Text, Number, Select, MultiSelect, Boolean, Date)
    /// </summary>
    /// <example>Select</example>
    public string? Type { get; set; }

    /// <summary>
    /// Yanıta önceden tanımlı değerleri dahil et
    /// </summary>
    /// <example>false</example>
    public bool IncludeValues { get; set; } = false;

    /// <summary>
    /// Sayfa numarası (1'den başlar)
    /// </summary>
    /// <example>1</example>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Sayfa başına kayıt sayısı (maksimum 100)
    /// </summary>
    /// <example>50</example>
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// Sıralama alanı (code, name, displayOrder, createdAt)
    /// </summary>
    /// <example>displayOrder</example>
    public string SortBy { get; set; } = "displayOrder";

    /// <summary>
    /// Sıralama yönü (asc = artan, desc = azalan)
    /// </summary>
    /// <example>asc</example>
    public string SortDirection { get; set; } = "asc";
}
