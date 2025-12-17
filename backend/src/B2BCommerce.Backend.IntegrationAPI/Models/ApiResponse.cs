namespace B2BCommerce.Backend.IntegrationAPI.Models;

/// <summary>
/// Standart API yanıt sarmalayıcısı
/// </summary>
/// <remarks>
/// Tüm API endpoint'leri bu formatta yanıt döner.
/// Başarılı işlemlerde Success=true, hatalı işlemlerde Success=false olur.
/// </remarks>
public class ApiResponse
{
    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// İşlem sonucu hakkında bilgi mesajı
    /// </summary>
    /// <example>Kayıt başarıyla oluşturuldu</example>
    public string? Message { get; set; }

    /// <summary>
    /// Hata kodu (başarısız işlemlerde). Programatik hata işleme için kullanılır.
    /// </summary>
    /// <example>NOT_FOUND</example>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Başarılı yanıt oluşturur
    /// </summary>
    /// <param name="message">İsteğe bağlı bilgi mesajı</param>
    /// <returns>Başarılı API yanıtı</returns>
    public static ApiResponse Ok(string? message = null) => new()
    {
        Success = true,
        Message = message
    };

    /// <summary>
    /// Hatalı yanıt oluşturur
    /// </summary>
    /// <param name="message">Hata mesajı</param>
    /// <param name="errorCode">İsteğe bağlı hata kodu</param>
    /// <returns>Hatalı API yanıtı</returns>
    public static ApiResponse Error(string message, string? errorCode = null) => new()
    {
        Success = false,
        Message = message,
        ErrorCode = errorCode
    };
}

/// <summary>
/// Veri içeren standart API yanıt sarmalayıcısı
/// </summary>
/// <typeparam name="T">Döndürülecek veri tipi</typeparam>
/// <remarks>
/// Tek kayıt döndüren endpoint'ler (GET by ID, POST, PUT) bu formatta yanıt döner.
/// </remarks>
public class ApiResponse<T> : ApiResponse
{
    /// <summary>
    /// İstek sonucu dönen veri
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Veri içeren başarılı yanıt oluşturur
    /// </summary>
    /// <param name="data">Döndürülecek veri</param>
    /// <param name="message">İsteğe bağlı bilgi mesajı</param>
    /// <returns>Veri içeren başarılı API yanıtı</returns>
    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message
    };

    /// <summary>
    /// Hatalı yanıt oluşturur
    /// </summary>
    /// <param name="message">Hata mesajı</param>
    /// <param name="errorCode">İsteğe bağlı hata kodu</param>
    /// <returns>Hatalı API yanıtı</returns>
    public new static ApiResponse<T> Error(string message, string? errorCode = null) => new()
    {
        Success = false,
        Message = message,
        ErrorCode = errorCode
    };
}

/// <summary>
/// Sayfalanmış liste endpoint'leri için API yanıt sarmalayıcısı
/// </summary>
/// <typeparam name="T">Liste öğelerinin tipi</typeparam>
/// <remarks>
/// Liste döndüren endpoint'ler (GET all) bu formatta yanıt döner.
/// Sayfalama meta bilgileri (toplam kayıt, toplam sayfa vb.) Pagination alanında bulunur.
/// </remarks>
public class PagedApiResponse<T> : ApiResponse<IEnumerable<T>>
{
    /// <summary>
    /// Sayfalama meta bilgileri
    /// </summary>
    public PaginationMeta? Pagination { get; set; }

    /// <summary>
    /// Sayfalanmış başarılı yanıt oluşturur
    /// </summary>
    /// <param name="data">Döndürülecek veri listesi</param>
    /// <param name="pageNumber">Mevcut sayfa numarası</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
    /// <param name="totalCount">Toplam kayıt sayısı</param>
    /// <param name="message">İsteğe bağlı bilgi mesajı</param>
    /// <returns>Sayfalanmış başarılı API yanıtı</returns>
    public static PagedApiResponse<T> Ok(
        IEnumerable<T> data,
        int pageNumber,
        int pageSize,
        int totalCount,
        string? message = null)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PagedApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message,
            Pagination = new PaginationMeta
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages
            }
        };
    }
}

/// <summary>
/// Sayfalama meta bilgileri
/// </summary>
/// <remarks>
/// Sayfalanmış yanıtlarda dönen ek bilgiler.
/// İstemcinin sayfalama kontrollerini oluşturması için kullanılır.
/// </remarks>
public class PaginationMeta
{
    /// <summary>
    /// Mevcut sayfa numarası (1'den başlar)
    /// </summary>
    /// <example>1</example>
    public int PageNumber { get; set; }

    /// <summary>
    /// Sayfa başına kayıt sayısı
    /// </summary>
    /// <example>20</example>
    public int PageSize { get; set; }

    /// <summary>
    /// Toplam kayıt sayısı
    /// </summary>
    /// <example>150</example>
    public int TotalCount { get; set; }

    /// <summary>
    /// Toplam sayfa sayısı
    /// </summary>
    /// <example>8</example>
    public int TotalPages { get; set; }

    /// <summary>
    /// Önceki sayfa var mı?
    /// </summary>
    /// <example>false</example>
    public bool HasPreviousPage { get; set; }

    /// <summary>
    /// Sonraki sayfa var mı?
    /// </summary>
    /// <example>true</example>
    public bool HasNextPage { get; set; }
}
