namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Categories;

/// <summary>
/// Kategori listesi için filtreleme parametreleri.
/// Filtrelemede harici ID'ler kullanılır.
/// </summary>
public class CategoryFilterDto
{
    /// <summary>
    /// Kategori adında arama yapılacak metin
    /// </summary>
    /// <example>Elektronik</example>
    public string? Search { get; set; }

    /// <summary>
    /// Üst kategorinin harici ID'sine göre filtrele (kök kategoriler için null)
    /// </summary>
    /// <example>ROOT001</example>
    public string? ParentId { get; set; }

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
    /// Sıralama alanı (name, displayOrder, createdAt)
    /// </summary>
    /// <example>displayOrder</example>
    public string SortBy { get; set; } = "displayOrder";

    /// <summary>
    /// Sıralama yönü (asc = artan, desc = azalan)
    /// </summary>
    /// <example>asc</example>
    public string SortDirection { get; set; } = "asc";
}
