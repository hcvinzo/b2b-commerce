namespace B2BCommerce.Backend.IntegrationAPI.DTOs.ProductTypes;

/// <summary>
/// Ürün tipi veri transfer nesnesi - API yanıtları için.
/// Id = ExternalId (string).
/// Code iş mantığı kodudur (ExternalCode değil).
/// Dahili Guid asla dışarıya açılmaz.
/// </summary>
/// <remarks>
/// Ürün tipi, benzer özelliklere sahip ürünleri gruplamak için kullanılır.
/// Her ürün tipine atanan özellikler (Attributes), o tipe ait ürünlerde doldurulması gereken/beklenen alanları belirler.
/// </remarks>
public class ProductTypeDto
{
    /// <summary>
    /// Harici ID (kaynak sistemden gelen ID, örn: LOGO ERP)
    /// </summary>
    /// <example>TYPE-LAPTOP</example>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Benzersiz iş mantığı kodu
    /// </summary>
    /// <example>laptop</example>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Görünen ad (Türkçe)
    /// </summary>
    /// <example>Dizüstü Bilgisayar</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Yönetici referansı için açıklama
    /// </summary>
    /// <example>Taşınabilir bilgisayarlar kategorisi</example>
    public string? Description { get; set; }

    /// <summary>
    /// Bu ürün tipi aktif mi?
    /// </summary>
    /// <example>true</example>
    public bool IsActive { get; set; }

    /// <summary>
    /// Bu ürün tipine atanmış özellik sayısı
    /// </summary>
    /// <example>5</example>
    public int AttributeCount { get; set; }

    /// <summary>
    /// Bu ürün tipine atanmış özellikler listesi
    /// </summary>
    public List<ProductTypeAttributeDto> Attributes { get; set; } = new();

    /// <summary>
    /// Son senkronizasyon tarihi
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }

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
/// Ürün tipi liste öğesi - sayfalanmış yanıtlar için.
/// Id = ExternalId (string).
/// Dahili Guid asla dışarıya açılmaz.
/// </summary>
public class ProductTypeListDto
{
    /// <summary>
    /// Harici ID (kaynak sistemden gelen ID, örn: LOGO ERP)
    /// </summary>
    /// <example>TYPE-LAPTOP</example>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Benzersiz iş mantığı kodu
    /// </summary>
    /// <example>laptop</example>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Görünen ad (Türkçe)
    /// </summary>
    /// <example>Dizüstü Bilgisayar</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Bu ürün tipi aktif mi?
    /// </summary>
    /// <example>true</example>
    public bool IsActive { get; set; }

    /// <summary>
    /// Bu ürün tipine atanmış özellik sayısı
    /// </summary>
    /// <example>5</example>
    public int AttributeCount { get; set; }

    /// <summary>
    /// Son senkronizasyon tarihi
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }
}

/// <summary>
/// Ürün tipi özellik ataması veri transfer nesnesi.
/// AttributeId = Özelliğin ExternalId'si (string).
/// </summary>
/// <remarks>
/// Bu DTO, bir ürün tipine atanmış özelliğin detaylarını ve
/// o tipe özel ayarlarını (zorunluluk, sıralama) içerir.
/// </remarks>
public class ProductTypeAttributeDto
{
    /// <summary>
    /// Özelliğin harici ID'si
    /// </summary>
    /// <example>ATTR-RAM</example>
    public string AttributeId { get; set; } = string.Empty;

    /// <summary>
    /// Özelliğin iş mantığı kodu
    /// </summary>
    /// <example>ram_kapasitesi</example>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Özelliğin görünen adı (Türkçe)
    /// </summary>
    /// <example>RAM Kapasitesi</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Özelliğin İngilizce görünen adı
    /// </summary>
    /// <example>RAM Capacity</example>
    public string? NameEn { get; set; }

    /// <summary>
    /// Özelliğin veri tipi (Text, Number, Select, MultiSelect, Boolean, Date)
    /// </summary>
    /// <example>Select</example>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Ölçü birimi
    /// </summary>
    /// <example>GB</example>
    public string? Unit { get; set; }

    /// <summary>
    /// Bu ürün tipi için bu özellik zorunlu mu?
    /// </summary>
    /// <example>true</example>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Bu ürün tipi içindeki sıralama önceliği
    /// </summary>
    /// <example>1</example>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Önceden tanımlı değerler (Select/MultiSelect tipleri için)
    /// </summary>
    public List<AttributeValueOptionDto> PredefinedValues { get; set; } = new();
}

/// <summary>
/// Özellik değer seçeneği veri transfer nesnesi (Select/MultiSelect tipleri için).
/// Value alanı tanımlayıcı olarak kullanılır (özellik içinde benzersiz).
/// </summary>
public class AttributeValueOptionDto
{
    /// <summary>
    /// Dahili değer (tanımlayıcı olarak kullanılır, özellik içinde benzersiz)
    /// </summary>
    /// <example>16</example>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Kullanıcıya gösterilecek metin
    /// </summary>
    /// <example>16 GB</example>
    public string? DisplayText { get; set; }

    /// <summary>
    /// Dropdown içinde sıralama önceliği
    /// </summary>
    /// <example>2</example>
    public int DisplayOrder { get; set; }
}

/// <summary>
/// Ürün tipi listesi için filtreleme parametreleri
/// </summary>
public class ProductTypeFilterDto
{
    /// <summary>
    /// Ad veya kod'da arama yapılacak metin
    /// </summary>
    /// <example>laptop</example>
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
    /// Sayfa başına kayıt sayısı
    /// </summary>
    /// <example>20</example>
    public int PageSize { get; set; } = 20;
}
