namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Attributes;

/// <summary>
/// Özellik (Attribute) tanımı veri transfer nesnesi - API yanıtları için.
/// Id = ExternalId (string), Code iş mantığı kodudur (ExternalCode değil).
/// Dahili Guid asla dışarıya açılmaz.
/// </summary>
/// <remarks>
/// Özellikler, ürünlerin filtrelenebilir ve karşılaştırılabilir niteliklerini tanımlar.
/// Örnek: RAM kapasitesi, ekran boyutu, renk seçenekleri vb.
/// Select/MultiSelect tipleri için önceden tanımlı değerler (PredefinedValues) desteklenir.
/// </remarks>
public class AttributeDefinitionDto
{
    /// <summary>
    /// Harici ID (kaynak sistemden gelen ID, örn: LOGO ERP)
    /// </summary>
    /// <example>ATTR-RAM</example>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// İş mantığı kodu (benzersiz tanımlayıcı, örn: "ekran_boyutu", "ram_kapasitesi")
    /// </summary>
    /// <example>ram_kapasitesi</example>
    public string Code { get; set; } = null!;

    /// <summary>
    /// Özellik adı (Türkçe görünen ad)
    /// </summary>
    /// <example>RAM Kapasitesi</example>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Veri tipi (Text, Number, Select, MultiSelect, Boolean, Date)
    /// </summary>
    /// <example>Select</example>
    public string Type { get; set; } = null!;

    /// <summary>
    /// Ölçü birimi (isteğe bağlı, örn: "GB", "MB/s", "mm", "inç")
    /// </summary>
    /// <example>GB</example>
    public string? Unit { get; set; }

    /// <summary>
    /// Ürün filtrelerinde gösterilsin mi?
    /// </summary>
    /// <example>true</example>
    public bool IsFilterable { get; set; }

    /// <summary>
    /// Varsayılan zorunluluk durumu (ProductType bazında geçersiz kılınabilir)
    /// </summary>
    /// <example>false</example>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Ürün detay sayfasında gösterilsin mi?
    /// </summary>
    /// <example>true</example>
    public bool IsVisibleOnProductPage { get; set; }

    /// <summary>
    /// Arayüzde sıralama önceliği (küçük değer önce gelir)
    /// </summary>
    /// <example>10</example>
    public int DisplayOrder { get; set; }

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

    /// <summary>
    /// Önceden tanımlı değerler listesi (Select/MultiSelect tipleri için)
    /// </summary>
    public List<AttributeValueDto>? PredefinedValues { get; set; }
}

/// <summary>
/// Özellik tanımı liste öğesi - sayfalanmış yanıtlar için (değerler dahil değil).
/// Id = ExternalId (string).
/// Dahili Guid asla dışarıya açılmaz.
/// </summary>
public class AttributeDefinitionListDto
{
    /// <summary>
    /// Harici ID (kaynak sistemden gelen ID, örn: LOGO ERP)
    /// </summary>
    /// <example>ATTR-RAM</example>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// İş mantığı kodu (benzersiz tanımlayıcı)
    /// </summary>
    /// <example>ram_kapasitesi</example>
    public string Code { get; set; } = null!;

    /// <summary>
    /// Özellik adı (Türkçe görünen ad)
    /// </summary>
    /// <example>RAM Kapasitesi</example>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Veri tipi (Text, Number, Select, MultiSelect, Boolean, Date)
    /// </summary>
    /// <example>Select</example>
    public string Type { get; set; } = null!;

    /// <summary>
    /// Ölçü birimi (isteğe bağlı)
    /// </summary>
    /// <example>GB</example>
    public string? Unit { get; set; }

    /// <summary>
    /// Ürün filtrelerinde gösterilsin mi?
    /// </summary>
    /// <example>true</example>
    public bool IsFilterable { get; set; }

    /// <summary>
    /// Varsayılan zorunluluk durumu
    /// </summary>
    /// <example>false</example>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Ürün detay sayfasında gösterilsin mi?
    /// </summary>
    /// <example>true</example>
    public bool IsVisibleOnProductPage { get; set; }

    /// <summary>
    /// Arayüzde sıralama önceliği
    /// </summary>
    /// <example>10</example>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Son senkronizasyon tarihi
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }

    /// <summary>
    /// Önceden tanımlı değer sayısı
    /// </summary>
    /// <example>5</example>
    public int ValueCount { get; set; }
}

/// <summary>
/// Önceden tanımlı değer veri transfer nesnesi.
/// Value alanı tanımlayıcı olarak kullanılır (özellik içinde benzersiz).
/// </summary>
/// <remarks>
/// Select ve MultiSelect tipindeki özellikler için kullanıcının seçebileceği değerleri tanımlar.
/// Örnek: RAM kapasitesi için "8", "16", "32" değerleri.
/// </remarks>
public class AttributeValueDto
{
    /// <summary>
    /// Değer anahtarı (özellik içinde benzersiz, tanımlayıcı olarak kullanılır)
    /// </summary>
    /// <example>16</example>
    public string Value { get; set; } = null!;

    /// <summary>
    /// Kullanıcıya gösterilecek metin (isteğe bağlı, boşsa Value kullanılır)
    /// </summary>
    /// <example>16 GB</example>
    public string? DisplayText { get; set; }

    /// <summary>
    /// Dropdown/liste içinde sıralama önceliği
    /// </summary>
    /// <example>2</example>
    public int DisplayOrder { get; set; }
}
