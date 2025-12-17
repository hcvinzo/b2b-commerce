using System.ComponentModel.DataAnnotations;

namespace B2BCommerce.Backend.IntegrationAPI.DTOs.ProductTypes;

/// <summary>
/// Harici sistemden ürün tipi senkronizasyonu için istek nesnesi.
/// Id = ExternalId (string) - birincil upsert anahtarı.
/// Code iş mantığı kodudur (ExternalCode değil).
/// </summary>
/// <remarks>
/// Bu istek nesnesi, ERP sistemlerinden ürün tiplerini senkronize etmek için kullanılır.
/// Aynı Id ile mevcut kayıt varsa güncellenir, yoksa yeni kayıt oluşturulur (upsert).
///
/// **Özellik Ataması:**
/// - Attributes dizisi ile özellikleri atayabilirsiniz
/// - Özellikler AttributeId (harici ID) veya AttributeCode (iş mantığı kodu) ile eşleştirilir
/// - Güncelleme sırasında tam değiştirme (full replacement) uygulanır
/// </remarks>
public class ProductTypeSyncRequest
{
    /// <summary>
    /// Harici ID (BİRİNCİL upsert anahtarı).
    /// Kaynak sistemdeki (LOGO ERP) benzersiz ID.
    /// Yeni ürün tipi oluşturmak için zorunludur.
    /// </summary>
    /// <example>TYPE-LAPTOP</example>
    [StringLength(100, ErrorMessage = "Harici ID en fazla 100 karakter olabilir")]
    public string? Id { get; set; }

    /// <summary>
    /// Benzersiz iş mantığı kodu (zorunlu, örn: "laptop", "monitor", "ssd").
    /// Id sağlanmadığında eşleştirme için yedek anahtar olarak kullanılır.
    /// </summary>
    /// <example>laptop</example>
    [Required(ErrorMessage = "Ürün tipi kodu zorunludur")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Ürün tipi kodu 1-100 karakter arasında olmalıdır")]
    public string Code { get; set; } = null!;

    /// <summary>
    /// Görünen ad (zorunlu, Türkçe)
    /// </summary>
    /// <example>Dizüstü Bilgisayar</example>
    [Required(ErrorMessage = "Ürün tipi adı zorunludur")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Ürün tipi adı 1-200 karakter arasında olmalıdır")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Yönetici referansı için açıklama
    /// </summary>
    /// <example>Taşınabilir bilgisayarlar kategorisi - RAM, işlemci, ekran boyutu özellikleri içerir</example>
    [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
    public string? Description { get; set; }

    /// <summary>
    /// Yeni ürünler bu tipi kullanabilir mi?
    /// </summary>
    /// <example>true</example>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Bu ürün tipine atanacak özellikler (tam değiştirme uygulanır).
    /// Güncelleme sırasında mevcut özellikler silinir ve yeni atamalar yapılır.
    /// </summary>
    /// <remarks>
    /// Her özellik için AttributeId (harici ID) veya AttributeCode (iş mantığı kodu) sağlanmalıdır.
    /// İkisi de sağlanırsa AttributeId önceliklidir.
    /// </remarks>
    [MaxLength(100, ErrorMessage = "Bir ürün tipine en fazla 100 özellik atanabilir")]
    public List<ProductTypeAttributeSyncRequest>? Attributes { get; set; }
}

/// <summary>
/// Ürün tipine özellik ataması senkronizasyonu için istek nesnesi.
/// AttributeId = Özelliğin ExternalId'si (string).
/// </summary>
/// <remarks>
/// Özellik eşleştirmesi için önce AttributeId, sonra AttributeCode kontrol edilir.
/// En az biri sağlanmalıdır.
/// </remarks>
public class ProductTypeAttributeSyncRequest
{
    /// <summary>
    /// Özelliğin harici ID'si (birincil arama).
    /// Kaynak sistemdeki (LOGO ERP) özellik ID'si.
    /// </summary>
    /// <example>ATTR-RAM</example>
    [StringLength(100, ErrorMessage = "Özellik harici ID'si en fazla 100 karakter olabilir")]
    public string? AttributeId { get; set; }

    /// <summary>
    /// Özelliğin iş mantığı kodu (AttributeId sağlanmadığında yedek arama)
    /// </summary>
    /// <example>ram_kapasitesi</example>
    [StringLength(100, ErrorMessage = "Özellik kodu en fazla 100 karakter olabilir")]
    public string? AttributeCode { get; set; }

    /// <summary>
    /// Bu ürün tipi için bu özellik zorunlu mu?
    /// </summary>
    /// <example>true</example>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// Bu ürün tipi içindeki sıralama önceliği (küçük değer önce gelir)
    /// </summary>
    /// <example>1</example>
    [Range(0, int.MaxValue, ErrorMessage = "Sıralama değeri 0 veya daha büyük olmalıdır")]
    public int DisplayOrder { get; set; } = 0;
}

/// <summary>
/// Toplu ürün tipi senkronizasyonu için istek nesnesi
/// </summary>
/// <remarks>
/// Birden fazla ürün tipini tek istekte senkronize etmek için kullanılır.
/// Her ürün tipi için aynı upsert mantığı uygulanır.
/// </remarks>
public class BulkProductTypeSyncRequest
{
    /// <summary>
    /// Senkronize edilecek ürün tipi listesi
    /// </summary>
    [Required(ErrorMessage = "En az bir ürün tipi gereklidir")]
    [MinLength(1, ErrorMessage = "En az bir ürün tipi gereklidir")]
    [MaxLength(100, ErrorMessage = "Tek istekte en fazla 100 ürün tipi senkronize edilebilir")]
    public List<ProductTypeSyncRequest> ProductTypes { get; set; } = new();
}
