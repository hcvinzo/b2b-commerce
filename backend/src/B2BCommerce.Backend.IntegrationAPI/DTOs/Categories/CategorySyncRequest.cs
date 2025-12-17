using System.ComponentModel.DataAnnotations;

namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Categories;

/// <summary>
/// Harici sistemden kategori senkronizasyonu için istek nesnesi.
/// Id = ExternalId (string) - birincil upsert anahtarı.
/// Code = ExternalCode (string) - isteğe bağlı ikincil referans.
/// </summary>
public class CategorySyncRequest
{
    /// <summary>
    /// Harici ID (BİRİNCİL upsert anahtarı).
    /// Kaynak sistemden gelen ID (LOGO ERP).
    /// Yeni kategori oluşturmak için zorunludur.
    /// </summary>
    /// <example>CAT001</example>
    [StringLength(100)]
    public string? Id { get; set; }

    /// <summary>
    /// Harici kod (İSTEĞE BAĞLI ikincil referans)
    /// </summary>
    /// <example>ELEKTRONIK</example>
    [StringLength(100)]
    public string? Code { get; set; }

    /// <summary>
    /// Kategori adı (zorunlu)
    /// </summary>
    /// <example>Elektronik</example>
    [Required(ErrorMessage = "Kategori adı zorunludur")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Kategori adı 1-200 karakter arasında olmalıdır")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Kategori açıklaması
    /// </summary>
    /// <example>Elektronik ürünler ve aksesuarlar</example>
    [StringLength(1000, ErrorMessage = "Açıklama 1000 karakteri geçemez")]
    public string? Description { get; set; }

    /// <summary>
    /// Üst kategorinin harici ID'si (hiyerarşi için).
    /// Kök kategori için boş bırakılır.
    /// </summary>
    /// <example>ROOT001</example>
    [StringLength(100)]
    public string? ParentId { get; set; }

    /// <summary>
    /// Kategori görseli URL'i
    /// </summary>
    /// <example>https://cdn.example.com/images/categories/elektronik.jpg</example>
    [StringLength(500, ErrorMessage = "Görsel URL'i 500 karakteri geçemez")]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Görüntüleme sırası (küçük değer önce gösterilir)
    /// </summary>
    /// <example>1</example>
    [Range(0, int.MaxValue, ErrorMessage = "Görüntüleme sırası 0 veya daha büyük olmalıdır")]
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Kategori aktif mi? (varsayılan: true)
    /// </summary>
    /// <example>true</example>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Harici sistemden toplu kategori senkronizasyonu için istek nesnesi.
/// </summary>
public class BulkCategorySyncRequest
{
    /// <summary>
    /// Senkronize edilecek kategoriler listesi (1-1000 adet)
    /// </summary>
    [Required(ErrorMessage = "Kategori listesi zorunludur")]
    [MinLength(1, ErrorMessage = "En az 1 kategori gereklidir")]
    [MaxLength(1000, ErrorMessage = "Tek seferde en fazla 1000 kategori gönderilebilir")]
    public List<CategorySyncRequest> Categories { get; set; } = new();
}
