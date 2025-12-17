namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Brands;

/// <summary>
/// Marka listesi için filtreleme parametreleri
/// </summary>
public class BrandFilterDto
{
    /// <summary>
    /// Marka adı veya açıklamasında arama yapılacak metin
    /// </summary>
    /// <example>Dell</example>
    public string? Search { get; set; }

    /// <summary>
    /// Aktiflik durumuna göre filtrele (null = tümü)
    /// </summary>
    /// <example>true</example>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Sayfa numarası (1'den başlar)
    /// </summary>
    /// <example>1</example>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Sayfa başına kayıt sayısı (maksimum 100)
    /// </summary>
    /// <example>20</example>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Sıralama alanı (name, createdAt, updatedAt)
    /// </summary>
    /// <example>name</example>
    public string SortBy { get; set; } = "name";

    /// <summary>
    /// Sıralama yönü (asc = artan, desc = azalan)
    /// </summary>
    /// <example>asc</example>
    public string SortDirection { get; set; } = "asc";
}
