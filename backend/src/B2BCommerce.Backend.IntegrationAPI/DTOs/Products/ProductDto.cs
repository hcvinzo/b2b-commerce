using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Products;

/// <summary>
/// Ürün veri transfer nesnesi - API yanıtları için.
/// Id = ExternalId (string), Code = ExternalCode (string).
/// Tüm ilişkili entity ID'leri (CategoryId, BrandId, ProductTypeId) de ExternalId'lerdir.
/// Dahili Guid'ler asla dışarıya açılmaz.
/// </summary>
public class ProductDto
{
    /// <summary>
    /// Harici ID (kaynak sistemden gelen ID, örn: LOGO ERP)
    /// </summary>
    /// <example>PRD001</example>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Harici kod (isteğe bağlı ikincil referans)
    /// </summary>
    /// <example>LAPTOP-001</example>
    public string? Code { get; set; }

    /// <summary>
    /// Stok Takip Birimi (benzersiz)
    /// </summary>
    /// <example>SKU-LAPTOP-001</example>
    public string SKU { get; set; } = string.Empty;

    /// <summary>
    /// Ürün adı
    /// </summary>
    /// <example>Dell Latitude 5520 Laptop</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Ürün açıklaması
    /// </summary>
    /// <example>Intel Core i7, 16GB RAM, 512GB SSD</example>
    public string? Description { get; set; }

    /// <summary>
    /// Kategorinin harici ID'si
    /// </summary>
    /// <example>CAT-BILGISAYAR</example>
    public string? CategoryId { get; set; }

    /// <summary>
    /// Kategori adı
    /// </summary>
    /// <example>Bilgisayarlar</example>
    public string? CategoryName { get; set; }

    /// <summary>
    /// Markanın harici ID'si
    /// </summary>
    /// <example>BRAND-DELL</example>
    public string? BrandId { get; set; }

    /// <summary>
    /// Marka adı
    /// </summary>
    /// <example>Dell</example>
    public string? BrandName { get; set; }

    /// <summary>
    /// Ürün tipinin harici ID'si
    /// </summary>
    /// <example>TYPE-LAPTOP</example>
    public string? ProductTypeId { get; set; }

    /// <summary>
    /// Ürün tipi adı
    /// </summary>
    /// <example>Dizüstü Bilgisayar</example>
    public string? ProductTypeName { get; set; }

    /// <summary>
    /// Liste fiyatı (KDV hariç)
    /// </summary>
    /// <example>25000.00</example>
    public decimal ListPrice { get; set; }

    /// <summary>
    /// Para birimi kodu
    /// </summary>
    /// <example>TRY</example>
    public string Currency { get; set; } = "TRY";

    /// <summary>
    /// Kademe 1 fiyatı (en yüksek seviye bayi)
    /// </summary>
    /// <example>24000.00</example>
    public decimal? Tier1Price { get; set; }

    /// <summary>
    /// Kademe 2 fiyatı
    /// </summary>
    /// <example>23500.00</example>
    public decimal? Tier2Price { get; set; }

    /// <summary>
    /// Kademe 3 fiyatı
    /// </summary>
    /// <example>23000.00</example>
    public decimal? Tier3Price { get; set; }

    /// <summary>
    /// Kademe 4 fiyatı
    /// </summary>
    /// <example>22500.00</example>
    public decimal? Tier4Price { get; set; }

    /// <summary>
    /// Kademe 5 fiyatı (en düşük seviye)
    /// </summary>
    /// <example>22000.00</example>
    public decimal? Tier5Price { get; set; }

    /// <summary>
    /// Mevcut stok miktarı
    /// </summary>
    /// <example>150</example>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Minimum sipariş miktarı
    /// </summary>
    /// <example>1</example>
    public int MinimumOrderQuantity { get; set; }

    /// <summary>
    /// KDV oranı (0.00 - 1.00 arası, örn: 0.20 = %20)
    /// </summary>
    /// <example>0.20</example>
    public decimal TaxRate { get; set; }

    /// <summary>
    /// Ürün durumu (Draft = 0, Active = 1, Inactive = 2)
    /// </summary>
    /// <example>1</example>
    public ProductStatus Status { get; set; }

    /// <summary>
    /// Ürün aktif mi (Status == Active hesaplanır).
    /// Geriye dönük uyumluluk için tutulur.
    /// </summary>
    /// <example>true</example>
    public bool IsActive { get; set; }

    /// <summary>
    /// Seri numarası takibi yapılıyor mu?
    /// </summary>
    /// <example>false</example>
    public bool IsSerialTracked { get; set; }

    /// <summary>
    /// Ana ürün görseli URL'i
    /// </summary>
    /// <example>https://cdn.example.com/images/products/laptop-001.jpg</example>
    public string? MainImageUrl { get; set; }

    /// <summary>
    /// Ek ürün görselleri URL listesi
    /// </summary>
    public List<string> ImageUrls { get; set; } = new();

    /// <summary>
    /// Ağırlık (kilogram)
    /// </summary>
    /// <example>2.5</example>
    public decimal? Weight { get; set; }

    /// <summary>
    /// Uzunluk (santimetre)
    /// </summary>
    /// <example>35.0</example>
    public decimal? Length { get; set; }

    /// <summary>
    /// Genişlik (santimetre)
    /// </summary>
    /// <example>24.0</example>
    public decimal? Width { get; set; }

    /// <summary>
    /// Yükseklik (santimetre)
    /// </summary>
    /// <example>2.5</example>
    public decimal? Height { get; set; }

    /// <summary>
    /// Son senkronizasyon tarihi
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }

    /// <summary>
    /// Ana ürünün harici ID'si (bu bir varyant ise)
    /// </summary>
    /// <example>PRD-MAIN-001</example>
    public string? MainProductId { get; set; }

    /// <summary>
    /// Bu ürün bir varyant mı (MainProductId varsa true)
    /// </summary>
    /// <example>false</example>
    public bool IsVariant { get; set; }

    /// <summary>
    /// Bu ürün bir ana ürün mü (varyantları olabilir)
    /// </summary>
    /// <example>true</example>
    public bool IsMainProduct { get; set; }

    /// <summary>
    /// Varyant sayısı (ana ürün ise)
    /// </summary>
    /// <example>3</example>
    public int VariantCount { get; set; }

    /// <summary>
    /// Oluşturulma tarihi
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Son güncellenme tarihi
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Ürün listesi öğesi - sayfalanmış yanıtlar için.
/// Id = ExternalId (string), Code = ExternalCode (string).
/// Dahili Guid'ler asla dışarıya açılmaz.
/// </summary>
public class ProductListDto
{
    /// <summary>
    /// Harici ID (kaynak sistemden gelen ID, örn: LOGO ERP)
    /// </summary>
    /// <example>PRD001</example>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Harici kod (isteğe bağlı ikincil referans)
    /// </summary>
    /// <example>LAPTOP-001</example>
    public string? Code { get; set; }

    /// <summary>
    /// Stok Takip Birimi (benzersiz)
    /// </summary>
    /// <example>SKU-LAPTOP-001</example>
    public string SKU { get; set; } = string.Empty;

    /// <summary>
    /// Ürün adı
    /// </summary>
    /// <example>Dell Latitude 5520 Laptop</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Kategorinin harici ID'si
    /// </summary>
    /// <example>CAT-BILGISAYAR</example>
    public string? CategoryId { get; set; }

    /// <summary>
    /// Kategori adı
    /// </summary>
    /// <example>Bilgisayarlar</example>
    public string? CategoryName { get; set; }

    /// <summary>
    /// Markanın harici ID'si
    /// </summary>
    /// <example>BRAND-DELL</example>
    public string? BrandId { get; set; }

    /// <summary>
    /// Marka adı
    /// </summary>
    /// <example>Dell</example>
    public string? BrandName { get; set; }

    /// <summary>
    /// Liste fiyatı (KDV hariç)
    /// </summary>
    /// <example>25000.00</example>
    public decimal ListPrice { get; set; }

    /// <summary>
    /// Para birimi kodu
    /// </summary>
    /// <example>TRY</example>
    public string Currency { get; set; } = "TRY";

    /// <summary>
    /// Mevcut stok miktarı
    /// </summary>
    /// <example>150</example>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Ürün durumu (Draft = 0, Active = 1, Inactive = 2)
    /// </summary>
    /// <example>1</example>
    public ProductStatus Status { get; set; }

    /// <summary>
    /// Ürün aktif mi (Status == Active hesaplanır).
    /// Geriye dönük uyumluluk için tutulur.
    /// </summary>
    /// <example>true</example>
    public bool IsActive { get; set; }

    /// <summary>
    /// Ana ürün görseli URL'i
    /// </summary>
    /// <example>https://cdn.example.com/images/products/laptop-001.jpg</example>
    public string? MainImageUrl { get; set; }

    /// <summary>
    /// Son senkronizasyon tarihi
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }

    /// <summary>
    /// Ana ürünün harici ID'si (bu bir varyant ise)
    /// </summary>
    /// <example>PRD-MAIN-001</example>
    public string? MainProductId { get; set; }

    /// <summary>
    /// Bu ürün bir varyant mı (MainProductId varsa true)
    /// </summary>
    /// <example>false</example>
    public bool IsVariant { get; set; }

    /// <summary>
    /// Varyant sayısı (ana ürün ise)
    /// </summary>
    /// <example>3</example>
    public int VariantCount { get; set; }
}
