namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Brands;

/// <summary>
/// Marka veri transfer nesnesi - API yanıtları için.
/// Id = ExternalId (string), Code = ExternalCode (string).
/// Dahili Guid asla dışarıya açılmaz.
/// </summary>
public class BrandDto
{
    /// <summary>
    /// Harici ID (kaynak sistemden gelen ID, örn: LOGO ERP)
    /// </summary>
    /// <example>BRAND001</example>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Harici kod (isteğe bağlı ikincil referans)
    /// </summary>
    /// <example>DELL</example>
    public string? Code { get; set; }

    /// <summary>
    /// Marka adı
    /// </summary>
    /// <example>Dell Technologies</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Marka açıklaması
    /// </summary>
    /// <example>Bilgisayar ve teknoloji ürünleri üreticisi</example>
    public string? Description { get; set; }

    /// <summary>
    /// Marka logosu URL'i
    /// </summary>
    /// <example>https://cdn.example.com/logos/dell.png</example>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Marka web sitesi URL'i
    /// </summary>
    /// <example>https://www.dell.com</example>
    public string? WebsiteUrl { get; set; }

    /// <summary>
    /// Marka aktif mi?
    /// </summary>
    /// <example>true</example>
    public bool IsActive { get; set; }

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
/// Marka listesi öğesi - sayfalanmış yanıtlar için.
/// Id = ExternalId (string), Code = ExternalCode (string).
/// Dahili Guid asla dışarıya açılmaz.
/// </summary>
public class BrandListDto
{
    /// <summary>
    /// Harici ID (kaynak sistemden gelen ID, örn: LOGO ERP)
    /// </summary>
    /// <example>BRAND001</example>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Harici kod (isteğe bağlı ikincil referans)
    /// </summary>
    /// <example>DELL</example>
    public string? Code { get; set; }

    /// <summary>
    /// Marka adı
    /// </summary>
    /// <example>Dell Technologies</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Marka logosu URL'i
    /// </summary>
    /// <example>https://cdn.example.com/logos/dell.png</example>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Marka aktif mi?
    /// </summary>
    /// <example>true</example>
    public bool IsActive { get; set; }

    /// <summary>
    /// Bu markaya ait ürün sayısı
    /// </summary>
    /// <example>250</example>
    public int ProductCount { get; set; }

    /// <summary>
    /// Son senkronizasyon tarihi
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }
}
