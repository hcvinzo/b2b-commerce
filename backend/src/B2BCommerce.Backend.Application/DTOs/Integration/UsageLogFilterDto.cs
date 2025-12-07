namespace B2BCommerce.Backend.Application.DTOs.Integration;

/// <summary>
/// DTO for filtering usage log queries
/// </summary>
public class UsageLogFilterDto
{
    public Guid? ApiKeyId { get; set; }
    public Guid? ApiClientId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Endpoint { get; set; }
    public int? StatusCode { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
