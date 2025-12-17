using System.ComponentModel.DataAnnotations;

namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Brands;

/// <summary>
/// Harici sistemden (LOGO ERP) marka senkronizasyonu için istek nesnesi.
/// Id = ExternalId (string) - birincil upsert anahtarı.
/// Code = ExternalCode (string) - isteğe bağlı ikincil referans.
/// </summary>
public class BrandSyncRequest
{
    /// <summary>
    /// Harici ID (BİRİNCİL upsert anahtarı - yeni markalar için zorunlu).
    /// Kaynak sistemden gelen ID (LOGO ERP).
    /// </summary>
    /// <example>BRAND001</example>
    [StringLength(100)]
    public string? Id { get; set; }

    /// <summary>
    /// Harici kod (İSTEĞE BAĞLI ikincil referans)
    /// </summary>
    /// <example>DELL</example>
    [StringLength(100)]
    public string? Code { get; set; }

    /// <summary>
    /// Marka adı (zorunlu, benzersiz)
    /// </summary>
    /// <example>Dell Technologies</example>
    [Required(ErrorMessage = "Marka adı zorunludur")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Marka adı 1-200 karakter arasında olmalıdır")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Marka açıklaması
    /// </summary>
    /// <example>Bilgisayar ve teknoloji ürünleri üreticisi</example>
    [StringLength(1000, ErrorMessage = "Açıklama 1000 karakteri geçemez")]
    public string? Description { get; set; }

    /// <summary>
    /// Marka logosu URL'i
    /// </summary>
    /// <example>https://cdn.example.com/logos/dell.png</example>
    [StringLength(500, ErrorMessage = "Logo URL'i 500 karakteri geçemez")]
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Marka web sitesi URL'i
    /// </summary>
    /// <example>https://www.dell.com</example>
    [StringLength(500, ErrorMessage = "Web sitesi URL'i 500 karakteri geçemez")]
    public string? WebsiteUrl { get; set; }

    /// <summary>
    /// Marka aktif mi? (varsayılan: true)
    /// </summary>
    /// <example>true</example>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Harici sistemden toplu marka senkronizasyonu için istek nesnesi.
/// </summary>
public class BulkBrandSyncRequest
{
    /// <summary>
    /// Senkronize edilecek markalar listesi (1-1000 adet)
    /// </summary>
    [Required(ErrorMessage = "Marka listesi zorunludur")]
    [MinLength(1, ErrorMessage = "En az 1 marka gereklidir")]
    [MaxLength(1000, ErrorMessage = "Tek seferde en fazla 1000 marka gönderilebilir")]
    public List<BrandSyncRequest> Brands { get; set; } = new();
}
