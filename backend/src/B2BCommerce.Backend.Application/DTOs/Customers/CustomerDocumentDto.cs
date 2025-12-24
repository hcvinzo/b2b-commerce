namespace B2BCommerce.Backend.Application.DTOs.Customers;

/// <summary>
/// DTO for customer document output
/// </summary>
public class CustomerDocumentDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentTypeName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string ContentUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating/uploading a customer document
/// </summary>
public class CreateCustomerDocumentDto
{
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string ContentUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
}

/// <summary>
/// DTO for updating/replacing a customer document
/// </summary>
public class UpdateCustomerDocumentDto
{
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string ContentUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
}
