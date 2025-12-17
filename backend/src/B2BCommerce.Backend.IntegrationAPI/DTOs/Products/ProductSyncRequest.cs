using System.ComponentModel.DataAnnotations;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Products;

/// <summary>
/// Harici sistemden (LOGO ERP) ürün senkronizasyonu için istek nesnesi.
/// Id = ExternalId (string) - birincil upsert anahtarı.
/// Code = ExternalCode (string) - isteğe bağlı ikincil referans.
/// Tüm ilişkili entity ID'leri (CategoryId, BrandId, ProductTypeId) de ExternalId'lerdir.
/// </summary>
public class ProductSyncRequest
{
    /// <summary>
    /// Harici ID (BİRİNCİL upsert anahtarı - yeni ürünler için zorunlu).
    /// Kaynak sistemden gelen ID (LOGO ERP).
    /// </summary>
    /// <example>PRD001</example>
    [StringLength(100)]
    public string? Id { get; set; }

    /// <summary>
    /// Harici kod (İSTEĞE BAĞLI ikincil referans)
    /// </summary>
    /// <example>LAPTOP-001</example>
    [StringLength(100)]
    public string? Code { get; set; }

    /// <summary>
    /// Stok Takip Birimi (zorunlu, benzersiz, eşleştirme için yedek olarak kullanılır)
    /// </summary>
    /// <example>SKU-LAPTOP-001</example>
    [Required(ErrorMessage = "SKU zorunludur")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "SKU 1-100 karakter arasında olmalıdır")]
    public string SKU { get; set; } = null!;

    /// <summary>
    /// Ürün adı (zorunlu)
    /// </summary>
    /// <example>Dell Latitude 5520 Laptop</example>
    [Required(ErrorMessage = "Ürün adı zorunludur")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Ürün adı 1-200 karakter arasında olmalıdır")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Ürün açıklaması
    /// </summary>
    /// <example>Intel Core i7, 16GB RAM, 512GB SSD dizüstü bilgisayar</example>
    [StringLength(5000, ErrorMessage = "Açıklama 5000 karakteri geçemez")]
    public string? Description { get; set; }

    /// <summary>
    /// Kategorinin harici ID'si (kategori atamak için zorunlu)
    /// </summary>
    /// <example>CAT-BILGISAYAR</example>
    [StringLength(100)]
    public string? CategoryId { get; set; }

    /// <summary>
    /// Markanın harici ID'si (isteğe bağlı)
    /// </summary>
    /// <example>BRAND-DELL</example>
    [StringLength(100)]
    public string? BrandId { get; set; }

    /// <summary>
    /// Ürün tipinin harici ID'si (isteğe bağlı)
    /// </summary>
    /// <example>TYPE-LAPTOP</example>
    [StringLength(100)]
    public string? ProductTypeId { get; set; }

    /// <summary>
    /// Liste fiyatı (KDV hariç, zorunlu)
    /// </summary>
    /// <example>25000.00</example>
    [Required(ErrorMessage = "Liste fiyatı zorunludur")]
    [Range(0, double.MaxValue, ErrorMessage = "Liste fiyatı 0 veya daha büyük olmalıdır")]
    public decimal ListPrice { get; set; }

    /// <summary>
    /// Para birimi kodu (varsayılan: TRY)
    /// </summary>
    /// <example>TRY</example>
    [StringLength(3, ErrorMessage = "Para birimi kodu 3 karakter olmalıdır")]
    public string Currency { get; set; } = "TRY";

    /// <summary>
    /// Kademe 1 fiyatı (en yüksek seviye bayi fiyatı)
    /// </summary>
    /// <example>24000.00</example>
    [Range(0, double.MaxValue, ErrorMessage = "Kademe 1 fiyatı 0 veya daha büyük olmalıdır")]
    public decimal? Tier1Price { get; set; }

    /// <summary>
    /// Kademe 2 fiyatı
    /// </summary>
    /// <example>23500.00</example>
    [Range(0, double.MaxValue, ErrorMessage = "Kademe 2 fiyatı 0 veya daha büyük olmalıdır")]
    public decimal? Tier2Price { get; set; }

    /// <summary>
    /// Kademe 3 fiyatı
    /// </summary>
    /// <example>23000.00</example>
    [Range(0, double.MaxValue, ErrorMessage = "Kademe 3 fiyatı 0 veya daha büyük olmalıdır")]
    public decimal? Tier3Price { get; set; }

    /// <summary>
    /// Kademe 4 fiyatı
    /// </summary>
    /// <example>22500.00</example>
    [Range(0, double.MaxValue, ErrorMessage = "Kademe 4 fiyatı 0 veya daha büyük olmalıdır")]
    public decimal? Tier4Price { get; set; }

    /// <summary>
    /// Kademe 5 fiyatı (en düşük seviye)
    /// </summary>
    /// <example>22000.00</example>
    [Range(0, double.MaxValue, ErrorMessage = "Kademe 5 fiyatı 0 veya daha büyük olmalıdır")]
    public decimal? Tier5Price { get; set; }

    /// <summary>
    /// Mevcut stok miktarı
    /// </summary>
    /// <example>150</example>
    [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı 0 veya daha büyük olmalıdır")]
    public int StockQuantity { get; set; } = 0;

    /// <summary>
    /// Minimum sipariş miktarı
    /// </summary>
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Minimum sipariş miktarı en az 1 olmalıdır")]
    public int MinimumOrderQuantity { get; set; } = 1;

    /// <summary>
    /// KDV oranı (0.00 - 1.00 arası, örn: 0.20 = %20)
    /// </summary>
    /// <example>0.20</example>
    [Range(0, 1, ErrorMessage = "KDV oranı 0 ile 1 arasında olmalıdır")]
    public decimal TaxRate { get; set; } = 0.20m;

    /// <summary>
    /// Ürün durumu (Draft = 0, Active = 1, Inactive = 2).
    /// Belirtilmezse, zorunlu alanlar dolu ise Active, değilse Draft olur.
    /// </summary>
    /// <example>1</example>
    public ProductStatus? Status { get; set; }

    /// <summary>
    /// Ana ürün görseli URL'i
    /// </summary>
    /// <example>https://cdn.example.com/images/products/laptop-001.jpg</example>
    [StringLength(500, ErrorMessage = "Görsel URL'i 500 karakteri geçemez")]
    public string? MainImageUrl { get; set; }

    /// <summary>
    /// Ek ürün görselleri URL listesi (maksimum 20 adet)
    /// </summary>
    [MaxLength(20, ErrorMessage = "En fazla 20 ek görsel eklenebilir")]
    public List<string>? ImageUrls { get; set; }

    /// <summary>
    /// Ağırlık (kilogram)
    /// </summary>
    /// <example>2.5</example>
    [Range(0, double.MaxValue, ErrorMessage = "Ağırlık 0 veya daha büyük olmalıdır")]
    public decimal? Weight { get; set; }

    /// <summary>
    /// Uzunluk (santimetre)
    /// </summary>
    /// <example>35.0</example>
    [Range(0, double.MaxValue, ErrorMessage = "Uzunluk 0 veya daha büyük olmalıdır")]
    public decimal? Length { get; set; }

    /// <summary>
    /// Genişlik (santimetre)
    /// </summary>
    /// <example>24.0</example>
    [Range(0, double.MaxValue, ErrorMessage = "Genişlik 0 veya daha büyük olmalıdır")]
    public decimal? Width { get; set; }

    /// <summary>
    /// Yükseklik (santimetre)
    /// </summary>
    /// <example>2.5</example>
    [Range(0, double.MaxValue, ErrorMessage = "Yükseklik 0 veya daha büyük olmalıdır")]
    public decimal? Height { get; set; }

    /// <summary>
    /// Ana ürünün harici ID'si (bu bir varyant ise).
    /// Belirtildiğinde, bu ürün belirtilen ana ürünün varyantı olur.
    /// </summary>
    /// <example>PRD-MAIN-001</example>
    [StringLength(100)]
    public string? MainProductId { get; set; }
}

/// <summary>
/// Harici sistemden toplu ürün senkronizasyonu için istek nesnesi.
/// </summary>
public class BulkProductSyncRequest
{
    /// <summary>
    /// Senkronize edilecek ürünler listesi (1-1000 adet)
    /// </summary>
    [Required(ErrorMessage = "Ürün listesi zorunludur")]
    [MinLength(1, ErrorMessage = "En az 1 ürün gereklidir")]
    [MaxLength(1000, ErrorMessage = "Tek seferde en fazla 1000 ürün gönderilebilir")]
    public List<ProductSyncRequest> Products { get; set; } = new();
}
