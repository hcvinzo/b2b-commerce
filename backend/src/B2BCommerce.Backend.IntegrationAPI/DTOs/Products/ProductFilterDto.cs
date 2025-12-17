namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Products;

/// <summary>
/// Ürün listesi için filtreleme parametreleri.
/// Tüm entity ID'leri (CategoryId, BrandId, ProductTypeId) ExternalId'lerdir.
/// </summary>
public class ProductFilterDto
{
    /// <summary>
    /// Ürün adı, SKU veya açıklamasında arama yapılacak metin
    /// </summary>
    /// <example>Laptop</example>
    public string? Search { get; set; }

    /// <summary>
    /// Kategorinin harici ID'sine göre filtrele
    /// </summary>
    /// <example>CAT-BILGISAYAR</example>
    public string? CategoryId { get; set; }

    /// <summary>
    /// Markanın harici ID'sine göre filtrele
    /// </summary>
    /// <example>BRAND-DELL</example>
    public string? BrandId { get; set; }

    /// <summary>
    /// Ürün tipinin harici ID'sine göre filtrele
    /// </summary>
    /// <example>TYPE-LAPTOP</example>
    public string? ProductTypeId { get; set; }

    /// <summary>
    /// Aktiflik durumuna göre filtrele (null = tümü)
    /// </summary>
    /// <example>true</example>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Minimum stok miktarına göre filtrele
    /// </summary>
    /// <example>10</example>
    public int? MinStock { get; set; }

    /// <summary>
    /// Maksimum stok miktarına göre filtrele
    /// </summary>
    /// <example>100</example>
    public int? MaxStock { get; set; }

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
    /// Sıralama alanı (name, sku, createdAt, updatedAt, stockQuantity, listPrice)
    /// </summary>
    /// <example>name</example>
    public string SortBy { get; set; } = "name";

    /// <summary>
    /// Sıralama yönü (asc = artan, desc = azalan)
    /// </summary>
    /// <example>asc</example>
    public string SortDirection { get; set; } = "asc";
}
