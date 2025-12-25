using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.Exceptions;
using B2BCommerce.Backend.Domain.ValueObjects;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Customer entity representing B2B customers
/// </summary>
public class Customer : BaseEntity, IAggregateRoot
{
    public string CompanyName { get; private set; }
    public string TradeName { get; private set; }
    public TaxNumber TaxNumber { get; private set; }
    public string TaxOffice { get; private set; }
    public string? MersisNo { get; private set; }
    public string? IdentityNo { get; private set; }
    public string? TradeRegistryNo { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber Phone { get; private set; }
    public PhoneNumber? MobilePhone { get; private set; }
    public string? Fax { get; private set; }
    public string? Website { get; private set; }
    public CustomerType Type { get; private set; }
    public PriceTier PriceTier { get; private set; }

    // Credit management
    public Money CreditLimit { get; private set; }
    public Money UsedCredit { get; private set; }
    public bool IsApproved { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? ApprovedBy { get; private set; }

    // Contact information
    public string ContactPersonName { get; private set; }
    public string ContactPersonTitle { get; private set; }

    // Settings
    public string PreferredCurrency { get; private set; }
    public string PreferredLanguage { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation properties
    public ICollection<Order> Orders { get; set; }
    public ICollection<CustomerAddress> Addresses { get; set; }
    public ICollection<CustomerAttribute> Attributes { get; set; }

    private Customer() // For EF Core
    {
        CompanyName = string.Empty;
        TradeName = string.Empty;
        TaxNumber = new TaxNumber("0000000000");
        TaxOffice = string.Empty;
        Email = new Email("default@example.com");
        Phone = new PhoneNumber("0000000000");
        CreditLimit = Money.Zero("USD");
        UsedCredit = Money.Zero("USD");
        ContactPersonName = string.Empty;
        ContactPersonTitle = string.Empty;
        PreferredCurrency = "USD";
        PreferredLanguage = "en";
        Orders = new List<Order>();
        Addresses = new List<CustomerAddress>();
        Attributes = new List<CustomerAttribute>();
    }

    /// <summary>
    /// Creates a new Customer instance
    /// </summary>
    public static Customer Create(
        string companyName,
        string tradeName,
        TaxNumber taxNumber,
        string taxOffice,
        Email email,
        PhoneNumber phone,
        string contactPersonName,
        string contactPersonTitle,
        Money creditLimit,
        CustomerType type = CustomerType.Standard,
        PriceTier priceTier = PriceTier.List,
        string? mersisNo = null,
        string? identityNo = null,
        string? tradeRegistryNo = null,
        PhoneNumber? mobilePhone = null,
        string? fax = null,
        string? website = null)
    {
        if (string.IsNullOrWhiteSpace(companyName))
        {
            throw new ArgumentException("Company name cannot be null or empty", nameof(companyName));
        }

        if (string.IsNullOrWhiteSpace(contactPersonName))
        {
            throw new ArgumentException("Contact person name cannot be null or empty", nameof(contactPersonName));
        }

        if (string.IsNullOrWhiteSpace(taxOffice))
        {
            throw new ArgumentException("Tax office cannot be null or empty", nameof(taxOffice));
        }

        var customer = new Customer
        {
            CompanyName = companyName,
            TradeName = tradeName ?? string.Empty,
            TaxNumber = taxNumber ?? throw new ArgumentNullException(nameof(taxNumber)),
            TaxOffice = taxOffice,
            MersisNo = mersisNo,
            IdentityNo = identityNo,
            TradeRegistryNo = tradeRegistryNo,
            Email = email ?? throw new ArgumentNullException(nameof(email)),
            Phone = phone ?? throw new ArgumentNullException(nameof(phone)),
            MobilePhone = mobilePhone,
            Fax = fax,
            Website = website,
            ContactPersonName = contactPersonName,
            ContactPersonTitle = contactPersonTitle ?? string.Empty,
            CreditLimit = creditLimit ?? throw new ArgumentNullException(nameof(creditLimit)),
            UsedCredit = Money.Zero(creditLimit.Currency),
            Type = type,
            PriceTier = priceTier,
            PreferredCurrency = creditLimit.Currency,
            PreferredLanguage = "tr",
            IsApproved = false,
            IsActive = false, // New customers are passive until approved
            Orders = new List<Order>(),
            Addresses = new List<CustomerAddress>(),
            Attributes = new List<CustomerAttribute>()
        };

        return customer;
    }

    [Obsolete("Use Customer.Create() factory method instead")]
    public Customer(
        string companyName,
        TaxNumber taxNumber,
        Email email,
        PhoneNumber phone,
        string contactPersonName,
        string contactPersonTitle,
        Money creditLimit,
        CustomerType type = CustomerType.Standard,
        PriceTier priceTier = PriceTier.List)
    {
        if (string.IsNullOrWhiteSpace(companyName))
        {
            throw new ArgumentException("Company name cannot be null or empty", nameof(companyName));
        }

        if (string.IsNullOrWhiteSpace(contactPersonName))
        {
            throw new ArgumentException("Contact person name cannot be null or empty", nameof(contactPersonName));
        }

        CompanyName = companyName;
        TradeName = string.Empty;
        TaxNumber = taxNumber ?? throw new ArgumentNullException(nameof(taxNumber));
        TaxOffice = string.Empty;
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Phone = phone ?? throw new ArgumentNullException(nameof(phone));
        ContactPersonName = contactPersonName;
        ContactPersonTitle = contactPersonTitle ?? string.Empty;
        CreditLimit = creditLimit ?? throw new ArgumentNullException(nameof(creditLimit));
        UsedCredit = Money.Zero(creditLimit.Currency);
        Type = type;
        PriceTier = priceTier;
        PreferredCurrency = creditLimit.Currency;
        PreferredLanguage = "tr";
        IsApproved = false;
        IsActive = false; // New customers are passive until approved
        Orders = new List<Order>();
        Addresses = new List<CustomerAddress>();
        Attributes = new List<CustomerAttribute>();
    }

    public void Approve(string approvedBy)
    {
        if (string.IsNullOrWhiteSpace(approvedBy))
            throw new ArgumentException("Approver information is required", nameof(approvedBy));

        IsApproved = true;
        IsActive = true; // Customer becomes active when approved
        ApprovedAt = DateTime.UtcNow;
        ApprovedBy = approvedBy;
    }

    public void UpdateCreditLimit(Money newCreditLimit)
    {
        if (newCreditLimit.Currency != CreditLimit.Currency)
            throw new InvalidOperationDomainException($"Credit limit currency must be {CreditLimit.Currency}");

        CreditLimit = newCreditLimit;
    }

    public Money GetAvailableCredit()
    {
        return CreditLimit - UsedCredit;
    }

    public bool HasSufficientCredit(Money amount)
    {
        if (amount.Currency != CreditLimit.Currency)
            throw new InvalidOperationDomainException($"Amount currency must be {CreditLimit.Currency}");

        return GetAvailableCredit() >= amount;
    }

    public void UseCredit(Money amount)
    {
        if (!HasSufficientCredit(amount))
            throw new InsufficientCreditException(Id, amount.Amount, GetAvailableCredit().Amount);

        UsedCredit = UsedCredit + amount;
    }

    public void ReleaseCredit(Money amount)
    {
        if (amount.Currency != CreditLimit.Currency)
            throw new InvalidOperationDomainException($"Amount currency must be {CreditLimit.Currency}");

        UsedCredit = UsedCredit - amount;

        // Ensure used credit doesn't go negative
        if (UsedCredit.Amount < 0)
            UsedCredit = Money.Zero(CreditLimit.Currency);
    }

    public bool IsCreditNearLimit(decimal thresholdPercentage = 0.9m)
    {
        if (CreditLimit.Amount == 0) return false;

        var usedPercentage = UsedCredit.Amount / CreditLimit.Amount;
        return usedPercentage >= thresholdPercentage;
    }

    public void UpdateContactInfo(
        string companyName,
        string contactPersonName,
        string contactPersonTitle,
        PhoneNumber phone,
        PhoneNumber? mobilePhone = null,
        string? fax = null,
        string? website = null)
    {
        if (string.IsNullOrWhiteSpace(companyName))
        {
            throw new ArgumentException("Company name cannot be null or empty", nameof(companyName));
        }

        if (string.IsNullOrWhiteSpace(contactPersonName))
        {
            throw new ArgumentException("Contact person name cannot be null or empty", nameof(contactPersonName));
        }

        CompanyName = companyName;
        ContactPersonName = contactPersonName;
        ContactPersonTitle = contactPersonTitle ?? string.Empty;
        Phone = phone ?? throw new ArgumentNullException(nameof(phone));
        MobilePhone = mobilePhone;
        Fax = fax;
        Website = website;
    }

    public void UpdateCompanyInfo(
        string tradeName,
        string taxOffice,
        string? mersisNo = null,
        string? identityNo = null,
        string? tradeRegistryNo = null)
    {
        if (string.IsNullOrWhiteSpace(taxOffice))
        {
            throw new ArgumentException("Tax office cannot be null or empty", nameof(taxOffice));
        }

        TradeName = tradeName ?? string.Empty;
        TaxOffice = taxOffice;
        MersisNo = mersisNo;
        IdentityNo = identityNo;
        TradeRegistryNo = tradeRegistryNo;
    }

    public void UpdatePriceTier(PriceTier priceTier)
    {
        PriceTier = priceTier;
    }

    public void UpdateType(CustomerType type)
    {
        Type = type;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
