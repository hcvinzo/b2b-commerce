using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Represents a document uploaded by or for a customer
/// </summary>
public class CustomerDocument : BaseEntity
{
    /// <summary>
    /// Foreign key to the customer who owns this document
    /// </summary>
    public Guid CustomerId { get; private set; }

    /// <summary>
    /// Type of the document (e.g., TaxCertificate, SignatureCircular)
    /// </summary>
    public CustomerDocumentType DocumentType { get; private set; }

    /// <summary>
    /// Original filename of the uploaded document
    /// </summary>
    public string FileName { get; private set; } = string.Empty;

    /// <summary>
    /// MIME type of the document (e.g., application/pdf, image/jpeg)
    /// </summary>
    public string FileType { get; private set; } = string.Empty;

    /// <summary>
    /// URL to the document stored in S3/CloudFront
    /// </summary>
    public string ContentUrl { get; private set; } = string.Empty;

    /// <summary>
    /// Size of the file in bytes
    /// </summary>
    public long FileSize { get; private set; }

    /// <summary>
    /// Navigation property to the customer
    /// </summary>
    public Customer? Customer { get; private set; }

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    [Obsolete("Use CustomerDocument.Create() factory method instead")]
    public CustomerDocument()
    {
    }

    /// <summary>
    /// Factory method to create a new customer document
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <param name="documentType">Type of the document</param>
    /// <param name="fileName">Original filename</param>
    /// <param name="fileType">MIME type</param>
    /// <param name="contentUrl">S3/CloudFront URL</param>
    /// <param name="fileSize">File size in bytes</param>
    /// <returns>A new CustomerDocument instance</returns>
    public static CustomerDocument Create(
        Guid customerId,
        CustomerDocumentType documentType,
        string fileName,
        string fileType,
        string contentUrl,
        long fileSize)
    {
        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("Customer ID cannot be empty", nameof(customerId));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name cannot be empty", nameof(fileName));
        }

        if (string.IsNullOrWhiteSpace(fileType))
        {
            throw new ArgumentException("File type cannot be empty", nameof(fileType));
        }

        if (string.IsNullOrWhiteSpace(contentUrl))
        {
            throw new ArgumentException("Content URL cannot be empty", nameof(contentUrl));
        }

        if (fileSize <= 0)
        {
            throw new ArgumentException("File size must be greater than zero", nameof(fileSize));
        }

#pragma warning disable CS0618 // Type or member is obsolete
        var document = new CustomerDocument
        {
            CustomerId = customerId,
            DocumentType = documentType,
            FileName = fileName,
            FileType = fileType,
            ContentUrl = contentUrl,
            FileSize = fileSize
        };
#pragma warning restore CS0618

        return document;
    }

    /// <summary>
    /// Updates the document with new file information
    /// </summary>
    /// <param name="fileName">New filename</param>
    /// <param name="fileType">New MIME type</param>
    /// <param name="contentUrl">New S3/CloudFront URL</param>
    /// <param name="fileSize">New file size in bytes</param>
    public void Update(string fileName, string fileType, string contentUrl, long fileSize)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name cannot be empty", nameof(fileName));
        }

        if (string.IsNullOrWhiteSpace(fileType))
        {
            throw new ArgumentException("File type cannot be empty", nameof(fileType));
        }

        if (string.IsNullOrWhiteSpace(contentUrl))
        {
            throw new ArgumentException("Content URL cannot be empty", nameof(contentUrl));
        }

        if (fileSize <= 0)
        {
            throw new ArgumentException("File size must be greater than zero", nameof(fileSize));
        }

        FileName = fileName;
        FileType = fileType;
        ContentUrl = contentUrl;
        FileSize = fileSize;
    }

    /// <summary>
    /// Gets the Turkish display name for the document type
    /// </summary>
    public string GetDocumentTypeName()
    {
        return DocumentType switch
        {
            CustomerDocumentType.TaxCertificate => "Vergi Levhası",
            CustomerDocumentType.SignatureCircular => "İmza Sirküleri",
            CustomerDocumentType.TradeRegistry => "Sicil Gazetesi",
            CustomerDocumentType.PartnershipAgreement => "İş Ortağı Sözleşmesi",
            CustomerDocumentType.AuthorizedIdCopy => "Yetkili Kimlik Fotokopisi",
            CustomerDocumentType.AuthorizedResidenceDocument => "Yetkili İkamet Belgesi",
            _ => DocumentType.ToString()
        };
    }
}
