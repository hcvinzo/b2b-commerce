using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.Exceptions;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Customer entity representing B2B customers.
/// Inherits from ExternalEntity to support external system synchronization.
/// </summary>
public class Customer : ExternalEntity, IAggregateRoot
{
    /// <summary>
    /// Company title/name (Ünvan)
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Tax office name (Vergi Dairesi)
    /// </summary>
    public string? TaxOffice { get; private set; }

    /// <summary>
    /// Tax number (Vergi Numarası)
    /// </summary>
    public string? TaxNo { get; private set; }

    /// <summary>
    /// Year of establishment (Kuruluş Yılı)
    /// </summary>
    public int? EstablishmentYear { get; private set; }

    /// <summary>
    /// Company website URL
    /// </summary>
    public string? Website { get; private set; }

    /// <summary>
    /// Current status in approval workflow
    /// </summary>
    public CustomerStatus Status { get; private set; }

    /// <summary>
    /// FK to the ASP.NET Identity User associated with this customer
    /// </summary>
    public Guid? UserId { get; private set; }

    /// <summary>
    /// Document URLs stored as JSON array (e.g., tax certificate, signature circular)
    /// Format: [{"type": "TaxCertificate", "url": "https://..."}, ...]
    /// </summary>
    public string? DocumentUrls { get; private set; }

    // Navigation properties
    private readonly List<CustomerContact> _contacts = new();
    public IReadOnlyCollection<CustomerContact> Contacts => _contacts.AsReadOnly();

    private readonly List<CustomerAddress> _addresses = new();
    public IReadOnlyCollection<CustomerAddress> Addresses => _addresses.AsReadOnly();

    private readonly List<CustomerAttribute> _attributes = new();
    public IReadOnlyCollection<CustomerAttribute> Attributes => _attributes.AsReadOnly();

    private readonly List<Order> _orders = new();
    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

    private Customer() // For EF Core
    {
        Title = string.Empty;
    }

    /// <summary>
    /// Creates a new Customer instance
    /// </summary>
    public static Customer Create(
        string title,
        string? taxOffice = null,
        string? taxNo = null,
        int? establishmentYear = null,
        string? website = null,
        Guid? userId = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Customer title is required");
        }

        var customer = new Customer
        {
            Title = title.Trim(),
            TaxOffice = taxOffice?.Trim(),
            TaxNo = taxNo?.Trim(),
            EstablishmentYear = establishmentYear,
            Website = website?.Trim(),
            Status = CustomerStatus.Pending,
            UserId = userId
        };

        // Auto-populate ExternalId for Integration API compatibility
        customer.SetExternalIdentifiers(externalCode: null, externalId: customer.Id.ToString());

        return customer;
    }

    /// <summary>
    /// Creates a customer from an external system (LOGO ERP).
    /// Uses ExternalId as the primary upsert key.
    /// </summary>
    public static Customer CreateFromExternal(
        string externalId,
        string title,
        string? taxOffice = null,
        string? taxNo = null,
        int? establishmentYear = null,
        string? website = null,
        CustomerStatus status = CustomerStatus.Active,
        Guid? userId = null,
        string? externalCode = null)
    {
        if (string.IsNullOrWhiteSpace(externalId))
        {
            throw new ArgumentException("External ID is required", nameof(externalId));
        }

        var customer = Create(title, taxOffice, taxNo, establishmentYear, website, userId);
        customer.Status = status;

        // Use base class helper for consistent initialization
        InitializeFromExternal(customer, externalId, externalCode);

        return customer;
    }

    /// <summary>
    /// Updates the customer details
    /// </summary>
    public void Update(
        string title,
        string? taxOffice,
        string? taxNo,
        int? establishmentYear,
        string? website)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Customer title is required");
        }

        Title = title.Trim();
        TaxOffice = taxOffice?.Trim();
        TaxNo = taxNo?.Trim();
        EstablishmentYear = establishmentYear;
        Website = website?.Trim();
    }

    /// <summary>
    /// Updates customer from external system sync (LOGO ERP).
    /// </summary>
    public void UpdateFromExternal(
        string title,
        string? taxOffice,
        string? taxNo,
        int? establishmentYear,
        string? website,
        CustomerStatus? status = null,
        string? externalCode = null)
    {
        Update(title, taxOffice, taxNo, establishmentYear, website);

        if (status.HasValue)
        {
            SetStatus(status.Value);
        }

        if (externalCode is not null)
        {
            SetExternalIdentifiers(externalCode, ExternalId);
        }

        MarkAsSynced();
    }

    /// <summary>
    /// Sets the customer status
    /// </summary>
    public void SetStatus(CustomerStatus status)
    {
        Status = status;
    }

    /// <summary>
    /// Approves the customer (sets status to Active)
    /// </summary>
    public void Approve()
    {
        if (Status == CustomerStatus.Active)
        {
            throw new InvalidOperationDomainException("Customer is already approved");
        }

        SetStatus(CustomerStatus.Active);
    }

    /// <summary>
    /// Rejects the customer application
    /// </summary>
    public void Reject()
    {
        if (Status == CustomerStatus.Active)
        {
            throw new InvalidOperationDomainException("Cannot reject an active customer");
        }

        SetStatus(CustomerStatus.Rejected);
    }

    /// <summary>
    /// Suspends the customer account
    /// </summary>
    public void Suspend()
    {
        SetStatus(CustomerStatus.Suspended);
    }

    /// <summary>
    /// Sets the user ID associated with this customer
    /// </summary>
    public void SetUserId(Guid? userId)
    {
        UserId = userId;
    }

    /// <summary>
    /// Updates the document URLs (stored as JSON)
    /// </summary>
    public void UpdateDocumentUrls(string? documentUrlsJson)
    {
        DocumentUrls = documentUrlsJson;
    }
}
