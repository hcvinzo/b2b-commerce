namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Categories;

/// <summary>
/// Kategori veri transfer nesnesi - API yanıtları için.
/// Id = ExternalId (string), Code = ExternalCode (string).
/// Dahili Guid asla dışarıya açılmaz.
/// </summary>
public class CategoryDto
{
    /// <summary>
    /// Harici ID (kaynak sistemden gelen ID, örn: LOGO ERP)
    /// </summary>
    /// <example>CAT001</example>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Harici kod (isteğe bağlı ikincil referans)
    /// </summary>
    /// <example>ELEKTRONIK</example>
    public string? Code { get; set; }

    /// <summary>
    /// Kategori adı
    /// </summary>
    /// <example>Elektronik</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Kategori açıklaması
    /// </summary>
    /// <example>Elektronik ürünler kategorisi</example>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Üst kategorinin harici ID'si (kök kategori için null)
    /// </summary>
    /// <example>ROOT001</example>
    public string? ParentId { get; set; }

    /// <summary>
    /// Üst kategorinin adı
    /// </summary>
    /// <example>Ana Kategoriler</example>
    public string? ParentName { get; set; }

    /// <summary>
    /// Kategori görseli URL'i
    /// </summary>
    /// <example>https://cdn.example.com/images/categories/elektronik.jpg</example>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Görüntüleme sırası (küçük değer önce gösterilir)
    /// </summary>
    /// <example>1</example>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Kategori aktif mi? (false ise müşterilere gösterilmez)
    /// </summary>
    /// <example>true</example>
    public bool IsActive { get; set; }

    /// <summary>
    /// SEO dostu URL yolu
    /// </summary>
    /// <example>elektronik</example>
    public string? Slug { get; set; }

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
/// Kategori listesi öğesi - sayfalanmış yanıtlar için.
/// Id = ExternalId (string), Code = ExternalCode (string).
/// Dahili Guid asla dışarıya açılmaz.
/// </summary>
public class CategoryListDto
{
    /// <summary>
    /// Harici ID (kaynak sistemden gelen ID, örn: LOGO ERP)
    /// </summary>
    /// <example>CAT001</example>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Harici kod (isteğe bağlı ikincil referans)
    /// </summary>
    /// <example>ELEKTRONIK</example>
    public string? Code { get; set; }

    /// <summary>
    /// Kategori adı
    /// </summary>
    /// <example>Elektronik</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Üst kategorinin harici ID'si (kök kategori için null)
    /// </summary>
    /// <example>ROOT001</example>
    public string? ParentId { get; set; }

    /// <summary>
    /// Üst kategorinin adı
    /// </summary>
    /// <example>Ana Kategoriler</example>
    public string? ParentName { get; set; }

    /// <summary>
    /// Görüntüleme sırası
    /// </summary>
    /// <example>1</example>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Kategori aktif mi?
    /// </summary>
    /// <example>true</example>
    public bool IsActive { get; set; }

    /// <summary>
    /// Alt kategori sayısı
    /// </summary>
    /// <example>5</example>
    public int SubCategoryCount { get; set; }

    /// <summary>
    /// Bu kategorideki ürün sayısı
    /// </summary>
    /// <example>150</example>
    public int ProductCount { get; set; }

    /// <summary>
    /// Son senkronizasyon tarihi
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }
}

/// <summary>
/// Kategori ağacı düğümü - hiyerarşik yanıtlar için.
/// Id = ExternalId (string), Code = ExternalCode (string).
/// Dahili Guid asla dışarıya açılmaz.
/// </summary>
public class CategoryTreeDto
{
    /// <summary>
    /// Harici ID (kaynak sistemden gelen ID, örn: LOGO ERP)
    /// </summary>
    /// <example>CAT001</example>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Harici kod (isteğe bağlı ikincil referans)
    /// </summary>
    /// <example>ELEKTRONIK</example>
    public string? Code { get; set; }

    /// <summary>
    /// Kategori adı
    /// </summary>
    /// <example>Elektronik</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Kategori açıklaması
    /// </summary>
    /// <example>Elektronik ürünler kategorisi</example>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Kategori görseli URL'i
    /// </summary>
    /// <example>https://cdn.example.com/images/categories/elektronik.jpg</example>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Görüntüleme sırası
    /// </summary>
    /// <example>1</example>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Kategori aktif mi?
    /// </summary>
    /// <example>true</example>
    public bool IsActive { get; set; }

    /// <summary>
    /// Alt kategoriler listesi (hiyerarşik yapı)
    /// </summary>
    public List<CategoryTreeDto> SubCategories { get; set; } = new();

    /// <summary>
    /// Son senkronizasyon tarihi
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }
}
