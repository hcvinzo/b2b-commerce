using System.ComponentModel.DataAnnotations;

namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Attributes;

/// <summary>
/// Harici sistemden özellik tanımı senkronizasyonu için istek nesnesi.
/// Id = ExternalId (string) - birincil upsert anahtarı.
/// Code iş mantığı kodudur ("ekran_boyutu" gibi), ExternalCode değil.
/// </summary>
/// <remarks>
/// Bu istek nesnesi, ERP sistemlerinden özellik tanımlarını senkronize etmek için kullanılır.
/// Aynı Id ile mevcut kayıt varsa güncellenir, yoksa yeni kayıt oluşturulur (upsert).
///
/// **Tip Açıklamaları:**
/// - **Text**: Serbest metin girişi (açıklama, not vb.)
/// - **Number**: Sayısal değer (ağırlık, boyut vb.)
/// - **Select**: Tek seçimli liste (renk, marka vb.)
/// - **MultiSelect**: Çoklu seçimli liste (özellikler, bağlantı portları vb.)
/// - **Boolean**: Evet/Hayır değeri (garantili mi?, aktif mi?)
/// - **Date**: Tarih değeri (üretim tarihi, garanti bitiş tarihi vb.)
/// </remarks>
public class AttributeSyncRequest
{
    /// <summary>
    /// Harici ID (BİRİNCİL upsert anahtarı).
    /// Kaynak sistemdeki (LOGO ERP) benzersiz ID.
    /// Yeni özellik oluşturmak için zorunludur.
    /// </summary>
    /// <example>ATTR-RAM</example>
    [StringLength(100, ErrorMessage = "Harici ID en fazla 100 karakter olabilir")]
    public string? Id { get; set; }

    /// <summary>
    /// Benzersiz iş mantığı kodu (zorunlu, örn: "ekran_boyutu", "ram_kapasitesi").
    /// Id sağlanmadığında eşleştirme için yedek anahtar olarak kullanılır.
    /// </summary>
    /// <example>ram_kapasitesi</example>
    [Required(ErrorMessage = "Özellik kodu zorunludur")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Özellik kodu 1-100 karakter arasında olmalıdır")]
    public string Code { get; set; } = null!;

    /// <summary>
    /// Türkçe görünen ad (zorunlu)
    /// </summary>
    /// <example>RAM Kapasitesi</example>
    [Required(ErrorMessage = "Özellik adı zorunludur")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Özellik adı 1-200 karakter arasında olmalıdır")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Bu özellik için veri tipi (zorunlu).
    /// Geçerli değerler: Text, Number, Select, MultiSelect, Boolean, Date
    /// </summary>
    /// <example>Select</example>
    [Required(ErrorMessage = "Özellik tipi zorunludur")]
    public string Type { get; set; } = null!;

    /// <summary>
    /// Ölçü birimi (isteğe bağlı, örn: "GB", "MB/s", "mm", "inç")
    /// </summary>
    /// <example>GB</example>
    [StringLength(50, ErrorMessage = "Ölçü birimi en fazla 50 karakter olabilir")]
    public string? Unit { get; set; }

    /// <summary>
    /// Bu özellik ürün filtrelerinde görünsün mü?
    /// </summary>
    /// <example>true</example>
    public bool IsFilterable { get; set; } = true;

    /// <summary>
    /// Varsayılan zorunluluk durumu (ProductType bazında geçersiz kılınabilir)
    /// </summary>
    /// <example>false</example>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// Ürün detay sayfasında gösterilsin mi?
    /// </summary>
    /// <example>true</example>
    public bool IsVisibleOnProductPage { get; set; } = true;

    /// <summary>
    /// Arayüzde sıralama önceliği (küçük değer önce gelir)
    /// </summary>
    /// <example>10</example>
    [Range(0, int.MaxValue, ErrorMessage = "Sıralama değeri 0 veya daha büyük olmalıdır")]
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Önceden tanımlı değerler (Select/MultiSelect tipleri için).
    /// Güncelleme sırasında tam değiştirme (full replacement) uygulanır.
    /// </summary>
    /// <remarks>
    /// Values gönderildiğinde, mevcut tüm değerler silinir ve yeni değerler eklenir.
    /// Mevcut değerleri korumak için tüm değerleri tekrar göndermeniz gerekir.
    /// </remarks>
    [MaxLength(500, ErrorMessage = "En fazla 500 önceden tanımlı değer eklenebilir")]
    public List<AttributeValueSyncRequest>? Values { get; set; }
}

/// <summary>
/// Önceden tanımlı değer senkronizasyonu için istek nesnesi
/// </summary>
/// <remarks>
/// Select ve MultiSelect tipindeki özellikler için kullanıcının seçebileceği değerleri tanımlar.
/// Value alanı özellik içinde benzersiz olmalıdır ve eşleştirme anahtarı olarak kullanılır.
/// </remarks>
public class AttributeValueSyncRequest
{
    /// <summary>
    /// Değer (zorunlu, mevcut değerlerle eşleştirme için anahtar olarak kullanılır)
    /// </summary>
    /// <example>16</example>
    [Required(ErrorMessage = "Değer zorunludur")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Değer 1-500 karakter arasında olmalıdır")]
    public string Value { get; set; } = null!;

    /// <summary>
    /// Kullanıcıya gösterilecek metin (isteğe bağlı, boşsa Value kullanılır)
    /// </summary>
    /// <example>16 GB</example>
    [StringLength(500, ErrorMessage = "Görüntüleme metni en fazla 500 karakter olabilir")]
    public string? DisplayText { get; set; }

    /// <summary>
    /// Dropdown/liste içinde sıralama önceliği
    /// </summary>
    /// <example>2</example>
    [Range(0, int.MaxValue, ErrorMessage = "Sıralama değeri 0 veya daha büyük olmalıdır")]
    public int DisplayOrder { get; set; } = 0;
}

/// <summary>
/// Toplu özellik tanımı senkronizasyonu için istek nesnesi
/// </summary>
/// <remarks>
/// Birden fazla özellik tanımını tek istekte senkronize etmek için kullanılır.
/// Her özellik için aynı upsert mantığı uygulanır.
/// </remarks>
public class BulkAttributeSyncRequest
{
    /// <summary>
    /// Senkronize edilecek özellik listesi
    /// </summary>
    [Required(ErrorMessage = "En az bir özellik gereklidir")]
    [MinLength(1, ErrorMessage = "En az bir özellik gereklidir")]
    [MaxLength(500, ErrorMessage = "Tek istekte en fazla 500 özellik senkronize edilebilir")]
    public List<AttributeSyncRequest> Attributes { get; set; } = new();
}
