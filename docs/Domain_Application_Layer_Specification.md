# B2B E-Commerce Platform - Domain & Application Layer Specification

## Document Purpose

Complete implementation specification for Domain and Application layers of the B2B E-Commerce Platform.

**Target**: .NET 8, EF Core 8, Clean Architecture, MediatR  
**Version**: 1.0  
**Date**: November 2025

---

## Quick Reference

### Key Technologies
- **.NET 8** - Framework
- **EF Core 8** - ORM
- **MediatR** - Domain events & CQRS
- **AutoMapper** - Object mapping
- **FluentValidation** - Input validation

### Project Structure
```
Domain/
├── Entities/          # Domain entities (aggregate roots)
├── ValueObjects/      # Immutable value objects
├── Events/           # Domain events
├── Services/         # Domain services
├── Exceptions/       # Domain exceptions
└── Enums/           # Enumerations

Application/
├── Services/         # Application services
├── DTOs/            # Data transfer objects
├── Interfaces/      # Service & repository interfaces
├── Mapping/         # AutoMapper profiles
├── Validators/      # FluentValidation rules
├── EventHandlers/   # MediatR event handlers
└── Exceptions/      # Application exceptions
```

---

# DOMAIN LAYER SPECIFICATION

## Core Entities

### 1. Product Aggregate

**Product.cs** (Aggregate Root)
```csharp
public class Product : BaseEntity, IAggregateRoot
{
    // Identity
    public string Sku { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    
    // Categorization
    public int CategoryId { get; private set; }
    public int BrandId { get; private set; }
    
    // Pricing
    public Money ListPrice { get; private set; }
    public Money DealerPrice { get; private set; }
    public string Currency { get; private set; }
    
    // Stock
    public int StockQuantity { get; private set; }
    public bool TrackStock { get; private set; }
    
    // Serial Tracking
    public bool RequiresSerialNumber { get; private set; }
    public int? WarrantyPeriodMonths { get; private set; }
    
    // Status
    public bool IsActive { get; private set; }
    public bool IsFeatured { get; private set; }
    
    // Navigation
    public Category Category { get; private set; }
    public Brand Brand { get; private set; }
    public ICollection<ProductImage> Images { get; private set; }
    public ICollection<ProductAttribute> Attributes { get; private set; }
    public ICollection<ProductPrice> CustomerPrices { get; private set; }
    
    // Factory Method
    public static Product Create(string sku, string name, int categoryId, int brandId,
        Money listPrice, Money dealerPrice, string currency, bool trackStock = true,
        bool requiresSerialNumber = false, int? warrantyPeriodMonths = null)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(sku))
            throw new DomainException("SKU required");
        if (dealerPrice.Amount > listPrice.Amount)
            throw new DomainException("Dealer price cannot exceed list price");
        if (requiresSerialNumber && !warrantyPeriodMonths.HasValue)
            throw new DomainException("Warranty period required for serial tracking");
        
        var product = new Product
        {
            Sku = sku,
            Name = name,
            CategoryId = categoryId,
            BrandId = brandId,
            ListPrice = listPrice,
            DealerPrice = dealerPrice,
            Currency = currency,
            TrackStock = trackStock,
            RequiresSerialNumber = requiresSerialNumber,
            WarrantyPeriodMonths = warrantyPeriodMonths,
            IsActive = true,
            StockQuantity = 0,
            CreatedAt = DateTime.UtcNow
        };
        
        product.AddDomainEvent(new ProductCreatedEvent(product));
        return product;
    }
    
    // Business Methods
    public void UpdatePricing(Money listPrice, Money dealerPrice)
    {
        if (dealerPrice.Amount > listPrice.Amount)
            throw new DomainException("Dealer price cannot exceed list price");
        
        ListPrice = listPrice;
        DealerPrice = dealerPrice;
        AddDomainEvent(new ProductPriceChangedEvent(this));
    }
    
    public void ReserveStock(int quantity)
    {
        if (!TrackStock) return;
        if (StockQuantity < quantity)
            throw new InsufficientStockException(Id, quantity, StockQuantity);
        
        StockQuantity -= quantity;
        AddDomainEvent(new StockReservedEvent(this, quantity));
    }
    
    public void ReleaseStock(int quantity)
    {
        if (!TrackStock) return;
        StockQuantity += quantity;
        AddDomainEvent(new StockReleasedEvent(this, quantity));
    }
    
    public bool CanBeOrdered(int quantity) =>
        IsActive && !IsDeleted && (!TrackStock || StockQuantity >= quantity);
}
```

### 2. Customer Aggregate

**Customer.cs** (Aggregate Root)
```csharp
public class Customer : BaseEntity, IAggregateRoot
{
    // Company Info
    public string CompanyName { get; private set; }
    public string TaxNumber { get; private set; }
    public Email PrimaryEmail { get; private set; }
    public PhoneNumber PrimaryPhone { get; private set; }
    
    // Financial
    public Money CreditLimit { get; private set; }
    public Money UsedCreditAmount { get; private set; }
    public Money OverdueAmount { get; private set; }
    public decimal? CustomerOverdueRatePercent { get; private set; }
    
    // Settings (Parametric)
    public string PreferredCurrency { get; private set; }
    public bool RequireOrderApproval { get; private set; }
    public bool AllowPartialOrderApproval { get; private set; }
    public bool RequireReturnApproval { get; private set; }
    
    // Status
    public bool IsActive { get; private set; }
    
    // Navigation
    public ICollection<User> Users { get; private set; }
    public ICollection<Address> Addresses { get; private set; }
    
    // Factory
    public static Customer Create(string companyName, string taxNumber, 
        Email email, PhoneNumber phone, Money creditLimit, string preferredCurrency)
    {
        var customer = new Customer
        {
            CompanyName = companyName,
            TaxNumber = taxNumber,
            PrimaryEmail = email,
            PrimaryPhone = phone,
            CreditLimit = creditLimit,
            UsedCreditAmount = Money.Zero(creditLimit.Currency),
            OverdueAmount = Money.Zero(creditLimit.Currency),
            PreferredCurrency = preferredCurrency,
            RequireOrderApproval = true,  // Default: manual approval
            AllowPartialOrderApproval = false,
            RequireReturnApproval = true,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };
        
        customer.AddDomainEvent(new CustomerCreatedEvent(customer));
        return customer;
    }
    
    // Business Methods
    public Money GetAvailableCredit()
    {
        var available = CreditLimit.Amount - UsedCreditAmount.Amount;
        return Money.Create(Math.Max(0, available), CreditLimit.Currency);
    }
    
    public void UseCreditForOrder(Money amount)
    {
        UsedCreditAmount = Money.Create(UsedCreditAmount.Amount + amount.Amount, 
            UsedCreditAmount.Currency);
        AddDomainEvent(new CreditUsedEvent(Id, amount));
    }
    
    public void ReleaseCreditForOrder(Money amount)
    {
        var newAmount = Math.Max(0, UsedCreditAmount.Amount - amount.Amount);
        UsedCreditAmount = Money.Create(newAmount, UsedCreditAmount.Currency);
        AddDomainEvent(new CreditReleasedEvent(Id, amount));
    }
    
    public void RecordPayment(Money amount)
    {
        var newUsed = Math.Max(0, UsedCreditAmount.Amount - amount.Amount);
        UsedCreditAmount = Money.Create(newUsed, UsedCreditAmount.Currency);
        
        var newOverdue = Math.Max(0, OverdueAmount.Amount - amount.Amount);
        OverdueAmount = Money.Create(newOverdue, OverdueAmount.Currency);
        
        AddDomainEvent(new PaymentReceivedEvent(Id, amount));
    }
    
    public bool CanPlaceOrder(Money orderTotal)
    {
        if (!IsActive) return false;
        if (CreditLimit.Amount == 0) return true; // Prepayment required
        return GetAvailableCredit().Amount >= orderTotal.Amount;
    }
    
    public bool RequiresPrepayment() => CreditLimit.Amount == 0;
}
```

### 3. Order Aggregate

**Order.cs** (Aggregate Root)
```csharp
public class Order : BaseEntity, IAggregateRoot
{
    public string OrderNumber { get; private set; }
    public int CustomerId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public OrderStatus Status { get; private set; }
    
    // Currency & Exchange
    public string Currency { get; private set; }
    public decimal ExchangeRate { get; private set; }  // Locked at approval
    public string BaseCurrency { get; private set; }
    
    // Totals
    public Money Subtotal { get; private set; }
    public Money TaxAmount { get; private set; }
    public Money ShippingAmount { get; private set; }
    public Money DiscountAmount { get; private set; }
    public Money TotalAmount { get; private set; }
    
    // Delivery
    public int ShippingAddressId { get; private set; }
    public int BillingAddressId { get; private set; }
    public string? TrackingNumber { get; private set; }
    
    // Payment
    public PaymentMethod PaymentMethod { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    
    // Approval (Parametric)
    public bool RequiresApproval { get; private set; }
    public int? ApprovedByUserId { get; private set; }
    public DateTime? ApprovedDate { get; private set; }
    public string? RejectionReason { get; private set; }
    
    // Navigation
    public Customer Customer { get; private set; }
    public ICollection<OrderItem> OrderItems { get; private set; }
    
    // Factory
    public static Order Create(int customerId, int shippingAddressId, int billingAddressId,
        string currency, string baseCurrency, PaymentMethod paymentMethod, bool requiresApproval)
    {
        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            CustomerId = customerId,
            OrderDate = DateTime.UtcNow,
            Status = requiresApproval ? OrderStatus.PendingApproval : OrderStatus.Approved,
            Currency = currency,
            BaseCurrency = baseCurrency,
            ExchangeRate = 1,
            ShippingAddressId = shippingAddressId,
            BillingAddressId = billingAddressId,
            PaymentMethod = paymentMethod,
            PaymentStatus = PaymentStatus.Pending,
            RequiresApproval = requiresApproval,
            Subtotal = Money.Zero(currency),
            TaxAmount = Money.Zero(currency),
            ShippingAmount = Money.Zero(currency),
            DiscountAmount = Money.Zero(currency),
            TotalAmount = Money.Zero(currency),
            CreatedAt = DateTime.UtcNow
        };
        
        order.AddDomainEvent(new OrderCreatedEvent(order));
        return order;
    }
    
    // Business Methods
    public void AddItem(int productId, string productName, int quantity, 
        Money unitPrice, decimal taxRate)
    {
        if (Status != OrderStatus.Draft && Status != OrderStatus.PendingApproval)
            throw new DomainException("Cannot modify approved order");
        
        var item = OrderItem.Create(Id, productId, productName, quantity, unitPrice, taxRate);
        OrderItems.Add(item);
        RecalculateTotals();
    }
    
    public void Approve(int approvedByUserId, decimal exchangeRate)
    {
        if (Status != OrderStatus.PendingApproval)
            throw new DomainException("Order not pending approval");
        
        Status = OrderStatus.Approved;
        ApprovedByUserId = approvedByUserId;
        ApprovedDate = DateTime.UtcNow;
        ExchangeRate = exchangeRate;
        
        AddDomainEvent(new OrderApprovedEvent(this));
    }
    
    public void ApprovePartially(int approvedByUserId, decimal exchangeRate,
        List<int> approvedItemIds, Dictionary<int, string> rejectedItemReasons)
    {
        if (Status != OrderStatus.PendingApproval)
            throw new DomainException("Order not pending approval");
        
        foreach (var itemId in approvedItemIds)
        {
            var item = OrderItems.FirstOrDefault(i => i.Id == itemId);
            item?.Approve();
        }
        
        foreach (var kvp in rejectedItemReasons)
        {
            var item = OrderItems.FirstOrDefault(i => i.Id == kvp.Key);
            item?.Reject(kvp.Value);
        }
        
        Status = OrderStatus.PartiallyApproved;
        ApprovedByUserId = approvedByUserId;
        ApprovedDate = DateTime.UtcNow;
        ExchangeRate = exchangeRate;
        
        RecalculateTotals();
        AddDomainEvent(new OrderPartiallyApprovedEvent(this));
    }
    
    public Money GetTotalInBaseCurrency()
    {
        return Money.Create(TotalAmount.Amount * ExchangeRate, BaseCurrency);
    }
    
    private void RecalculateTotals()
    {
        var subtotal = OrderItems.Sum(i => i.TotalPrice.Amount);
        Subtotal = Money.Create(subtotal, Currency);
        
        var tax = OrderItems.Sum(i => i.TaxAmount.Amount);
        TaxAmount = Money.Create(tax, Currency);
        
        var total = subtotal + tax + ShippingAmount.Amount - DiscountAmount.Amount;
        TotalAmount = Money.Create(total, Currency);
    }
    
    private static string GenerateOrderNumber() =>
        $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
}
```

**OrderItem.cs**
```csharp
public class OrderItem : BaseEntity
{
    public int OrderId { get; private set; }
    public int ProductId { get; private set; }
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    public decimal TaxRate { get; private set; }
    public Money TaxAmount { get; private set; }
    public Money TotalPrice { get; private set; }
    
    // Partial Approval Support
    public OrderItemStatus ItemStatus { get; private set; }
    public string? RejectionReason { get; private set; }
    
    public static OrderItem Create(int orderId, int productId, string productName,
        int quantity, Money unitPrice, decimal taxRate)
    {
        var subtotal = unitPrice.Amount * quantity;
        var tax = subtotal * taxRate;
        var total = subtotal + tax;
        
        return new OrderItem
        {
            OrderId = orderId,
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice,
            TaxRate = taxRate,
            TaxAmount = Money.Create(tax, unitPrice.Currency),
            TotalPrice = Money.Create(total, unitPrice.Currency),
            ItemStatus = OrderItemStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    public void Approve()
    {
        ItemStatus = OrderItemStatus.Approved;
        RejectionReason = null;
    }
    
    public void Reject(string reason)
    {
        ItemStatus = OrderItemStatus.Rejected;
        RejectionReason = reason;
    }
}
```

### 4. Return Aggregate

**Return.cs** (Aggregate Root)
```csharp
public class Return : BaseEntity, IAggregateRoot
{
    public string ReturnNumber { get; private set; }
    public int OrderId { get; private set; }
    public int CustomerId { get; private set; }
    public int RequestedByUserId { get; private set; }
    public DateTime RequestDate { get; private set; }
    public ReturnStatus Status { get; private set; }
    public string Reason { get; private set; }
    
    // Approval (Parametric)
    public bool RequiresApproval { get; private set; }
    public int? ApprovedByUserId { get; private set; }
    public DateTime? ApprovedDate { get; private set; }
    public string? RejectionReason { get; private set; }
    
    // Refund
    public Money RefundAmount { get; private set; }
    public DateTime? RefundDate { get; private set; }
    
    // Navigation
    public Order Order { get; private set; }
    public ICollection<ReturnItem> ReturnItems { get; private set; }
    
    public static Return Create(int orderId, int customerId, int requestedByUserId,
        string reason, bool requiresApproval, string currency)
    {
        var returnRequest = new Return
        {
            ReturnNumber = $"RET-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
            OrderId = orderId,
            CustomerId = customerId,
            RequestedByUserId = requestedByUserId,
            RequestDate = DateTime.UtcNow,
            Status = requiresApproval ? ReturnStatus.PendingApproval : ReturnStatus.Approved,
            Reason = reason,
            RequiresApproval = requiresApproval,
            RefundAmount = Money.Zero(currency),
            CreatedAt = DateTime.UtcNow
        };
        
        returnRequest.AddDomainEvent(new ReturnRequestedEvent(returnRequest));
        return returnRequest;
    }
    
    public void Approve(int approvedByUserId)
    {
        if (Status != ReturnStatus.PendingApproval)
            throw new DomainException("Return not pending approval");
        
        Status = ReturnStatus.Approved;
        ApprovedByUserId = approvedByUserId;
        ApprovedDate = DateTime.UtcNow;
        
        AddDomainEvent(new ReturnApprovedEvent(this));
    }
}
```

### 5. Payment Entity

**Payment.cs**
```csharp
public class Payment : BaseEntity, IAggregateRoot
{
    public int OrderId { get; private set; }
    public int CustomerId { get; private set; }
    public string PaymentNumber { get; private set; }
    public PaymentMethod Method { get; private set; }
    public Money Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTime PaymentDate { get; private set; }
    
    // Credit Card
    public string? TransactionId { get; private set; }
    public string? CardLastFourDigits { get; private set; }
    
    // Commission
    public decimal CommissionRate { get; private set; }
    public Money CommissionAmount { get; private set; }
    public Money NetAmount { get; private set; }
    
    public static Payment Create(int orderId, int customerId, PaymentMethod method,
        Money amount, decimal commissionRate)
    {
        var commission = amount.Amount * commissionRate;
        var net = amount.Amount - commission;
        
        return new Payment
        {
            OrderId = orderId,
            CustomerId = customerId,
            PaymentNumber = $"PAY-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
            Method = method,
            Amount = amount,
            Status = PaymentStatus.Pending,
            PaymentDate = DateTime.UtcNow,
            CommissionRate = commissionRate,
            CommissionAmount = Money.Create(commission, amount.Currency),
            NetAmount = Money.Create(net, amount.Currency),
            CreatedAt = DateTime.UtcNow
        };
    }
    
    public void MarkAsCompleted(string? transactionId = null, string? cardLastFour = null)
    {
        Status = PaymentStatus.Completed;
        TransactionId = transactionId;
        CardLastFourDigits = cardLastFour;
        AddDomainEvent(new PaymentCompletedEvent(this));
    }
}
```

---

## Value Objects

### Money

```csharp
public class Money : ValueObject
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }
    
    public static Money Create(decimal amount, string currency) => 
        new Money(amount, currency);
    
    public static Money Zero(string currency) => new Money(0, currency);
    
    public static Money TRY(decimal amount) => new Money(amount, "TRY");
    public static Money USD(decimal amount) => new Money(amount, "USD");
    public static Money EUR(decimal amount) => new Money(amount, "EUR");
    
    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new DomainException($"Cannot add different currencies: {a.Currency} and {b.Currency}");
        return new Money(a.Amount + b.Amount, a.Currency);
    }
    
    public static Money operator -(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new DomainException($"Cannot subtract different currencies");
        return new Money(a.Amount - b.Amount, a.Currency);
    }
    
    public Money ConvertTo(string targetCurrency, decimal exchangeRate) =>
        new Money(Amount * exchangeRate, targetCurrency);
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
    
    public override string ToString() => $"{Amount:N2} {Currency}";
}
```

### Email

```csharp
public class Email : ValueObject
{
    public string Value { get; private set; }
    
    private Email(string value)
    {
        if (!IsValid(value))
            throw new DomainException($"Invalid email: {value}");
        Value = value.ToLowerInvariant();
    }
    
    public static Email Create(string value) => new Email(value);
    
    private static bool IsValid(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch { return false; }
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public static implicit operator string(Email email) => email.Value;
}
```

### PhoneNumber

```csharp
public class PhoneNumber : ValueObject
{
    public string CountryCode { get; private set; }
    public string Number { get; private set; }
    
    private PhoneNumber(string countryCode, string number)
    {
        CountryCode = new string(countryCode.Where(char.IsDigit).ToArray());
        Number = new string(number.Where(char.IsDigit).ToArray());
        
        if (string.IsNullOrWhiteSpace(Number))
            throw new DomainException("Phone number invalid");
    }
    
    public static PhoneNumber Create(string countryCode, string number) =>
        new PhoneNumber(countryCode, number);
    
    public static PhoneNumber CreateTurkish(string number) =>
        new PhoneNumber("90", number);
    
    public string GetFullNumber() => $"+{CountryCode}{Number}";
    
    public string GetFormattedNumber()
    {
        // Turkish format: +90 (5XX) XXX XX XX
        if (CountryCode == "90" && Number.Length == 10)
            return $"+90 ({Number[..3]}) {Number.Substring(3, 3)} {Number.Substring(6, 2)} {Number.Substring(8, 2)}";
        return GetFullNumber();
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return CountryCode;
        yield return Number;
    }
}
```

### TaxNumber

```csharp
public class TaxNumber : ValueObject
{
    public string Value { get; private set; }
    public TaxNumberType Type { get; private set; }
    
    private TaxNumber(string value, TaxNumberType type)
    {
        value = new string(value.Where(char.IsDigit).ToArray());
        
        if (type == TaxNumberType.VKN && value.Length != 10)
            throw new DomainException("VKN must be 10 digits");
        if (type == TaxNumberType.TCKN && value.Length != 11)
            throw new DomainException("TCKN must be 11 digits");
        
        Value = value;
        Type = type;
    }
    
    public static TaxNumber CreateVKN(string value) => new TaxNumber(value, TaxNumberType.VKN);
    public static TaxNumber CreateTCKN(string value) => new TaxNumber(value, TaxNumberType.TCKN);
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Type;
    }
}

public enum TaxNumberType { VKN = 1, TCKN = 2 }
```

---

## Domain Events

```csharp
// Base
public abstract class DomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredOn { get; }
    
    protected DomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }
}

// Product Events
public class ProductCreatedEvent : DomainEvent
{
    public Product Product { get; }
    public ProductCreatedEvent(Product product) { Product = product; }
}

public class ProductPriceChangedEvent : DomainEvent
{
    public Product Product { get; }
    public ProductPriceChangedEvent(Product product) { Product = product; }
}

public class StockReservedEvent : DomainEvent
{
    public Product Product { get; }
    public int Quantity { get; }
    public StockReservedEvent(Product product, int quantity)
    {
        Product = product;
        Quantity = quantity;
    }
}

public class StockReleasedEvent : DomainEvent
{
    public Product Product { get; }
    public int Quantity { get; }
    public StockReleasedEvent(Product product, int quantity)
    {
        Product = product;
        Quantity = quantity;
    }
}

// Customer Events
public class CustomerCreatedEvent : DomainEvent
{
    public Customer Customer { get; }
    public CustomerCreatedEvent(Customer customer) { Customer = customer; }
}

public class CreditUsedEvent : DomainEvent
{
    public int CustomerId { get; }
    public Money Amount { get; }
    public CreditUsedEvent(int customerId, Money amount)
    {
        CustomerId = customerId;
        Amount = amount;
    }
}

public class CreditReleasedEvent : DomainEvent
{
    public int CustomerId { get; }
    public Money Amount { get; }
    public CreditReleasedEvent(int customerId, Money amount)
    {
        CustomerId = customerId;
        Amount = amount;
    }
}

public class PaymentReceivedEvent : DomainEvent
{
    public int CustomerId { get; }
    public Money Amount { get; }
    public PaymentReceivedEvent(int customerId, Money amount)
    {
        CustomerId = customerId;
        Amount = amount;
    }
}

// Order Events
public class OrderCreatedEvent : DomainEvent
{
    public Order Order { get; }
    public OrderCreatedEvent(Order order) { Order = order; }
}

public class OrderApprovedEvent : DomainEvent
{
    public Order Order { get; }
    public OrderApprovedEvent(Order order) { Order = order; }
}

public class OrderPartiallyApprovedEvent : DomainEvent
{
    public Order Order { get; }
    public OrderPartiallyApprovedEvent(Order order) { Order = order; }
}

public class OrderCancelledEvent : DomainEvent
{
    public Order Order { get; }
    public string Reason { get; }
    public OrderCancelledEvent(Order order, string reason)
    {
        Order = order;
        Reason = reason;
    }
}

// Payment Events
public class PaymentCompletedEvent : DomainEvent
{
    public Payment Payment { get; }
    public PaymentCompletedEvent(Payment payment) { Payment = payment; }
}

// Return Events
public class ReturnRequestedEvent : DomainEvent
{
    public Return Return { get; }
    public ReturnRequestedEvent(Return returnRequest) { Return = returnRequest; }
}

public class ReturnApprovedEvent : DomainEvent
{
    public Return Return { get; }
    public ReturnApprovedEvent(Return returnRequest) { Return = returnRequest; }
}
```

---

## Domain Services

### PricingService

```csharp
public interface IPricingService
{
    Task<Money> GetPriceForCustomerAsync(Product product, Customer customer, DateTime? priceDate = null);
}

public class PricingService : IPricingService
{
    public async Task<Money> GetPriceForCustomerAsync(Product product, Customer customer, DateTime? priceDate = null)
    {
        var effectiveDate = priceDate ?? DateTime.UtcNow;
        
        // Priority 1: Customer-specific price
        var customerPrice = product.CustomerPrices
            .FirstOrDefault(p => p.CustomerId == customer.Id && p.IsValid(effectiveDate));
        
        if (customerPrice != null)
            return customerPrice.Price;
        
        // Priority 2: Dealer price
        return product.DealerPrice;
    }
}
```

### CreditManagementService

```csharp
public interface ICreditManagementService
{
    Task<bool> CanCustomerPlaceOrderAsync(Customer customer, Money orderTotal);
    Task<Money> GetAvailableCreditAsync(Customer customer);
    Task<decimal> GetEffectiveOverdueRateAsync(Customer customer);
}

public class CreditManagementService : ICreditManagementService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ISystemConfigurationRepository _configRepository;
    
    public CreditManagementService(IOrderRepository orderRepository, 
        ISystemConfigurationRepository configRepository)
    {
        _orderRepository = orderRepository;
        _configRepository = configRepository;
    }
    
    public async Task<bool> CanCustomerPlaceOrderAsync(Customer customer, Money orderTotal)
    {
        if (!customer.IsActive) return false;
        if (customer.CreditLimit.Amount == 0) return true; // Prepayment
        
        var availableCredit = await GetAvailableCreditAsync(customer);
        return orderTotal.Amount <= availableCredit.Amount;
    }
    
    public async Task<Money> GetAvailableCreditAsync(Customer customer)
    {
        var pendingOrders = await _orderRepository.GetPendingOrdersByCustomerAsync(customer.Id);
        var pendingAmount = pendingOrders.Sum(o => o.GetTotalInBaseCurrency().Amount);
        
        var available = customer.CreditLimit.Amount 
            - customer.UsedCreditAmount.Amount 
            - pendingAmount;
        
        return Money.Create(Math.Max(0, available), customer.CreditLimit.Currency);
    }
    
    public async Task<decimal> GetEffectiveOverdueRateAsync(Customer customer)
    {
        // Customer-specific rate takes precedence
        if (customer.CustomerOverdueRatePercent.HasValue)
            return customer.CustomerOverdueRatePercent.Value;
        
        // System-wide rate
        return await _configRepository.GetValueAsync<decimal>("SystemOverdueRatePercent");
    }
}
```

### CurrencyConversionService

```csharp
public interface ICurrencyConversionService
{
    Task<Money> ConvertAsync(Money amount, string targetCurrency, DateTime? rateDate = null);
    Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency, DateTime? rateDate = null);
}

public class CurrencyConversionService : ICurrencyConversionService
{
    private readonly ICurrencyRateRepository _currencyRateRepository;
    
    public CurrencyConversionService(ICurrencyRateRepository currencyRateRepository)
    {
        _currencyRateRepository = currencyRateRepository;
    }
    
    public async Task<Money> ConvertAsync(Money amount, string targetCurrency, DateTime? rateDate = null)
    {
        if (amount.Currency == targetCurrency) return amount;
        
        var rate = await GetExchangeRateAsync(amount.Currency, targetCurrency, rateDate);
        return amount.ConvertTo(targetCurrency, rate);
    }
    
    public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency, DateTime? rateDate = null)
    {
        if (fromCurrency == toCurrency) return 1m;
        
        var effectiveDate = rateDate ?? DateTime.UtcNow;
        var rate = await _currencyRateRepository.GetRateAsync(fromCurrency, toCurrency, effectiveDate);
        
        if (rate == null)
            throw new DomainException($"Exchange rate not found: {fromCurrency} to {toCurrency}");
        
        return rate.Rate;
    }
}
```

---

## Domain Exceptions

```csharp
// Base
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

// Specific
public class InsufficientStockException : DomainException
{
    public int ProductId { get; }
    public int RequestedQuantity { get; }
    public int AvailableQuantity { get; }
    
    public InsufficientStockException(int productId, int requested, int available)
        : base($"Insufficient stock for product {productId}. Requested: {requested}, Available: {available}")
    {
        ProductId = productId;
        RequestedQuantity = requested;
        AvailableQuantity = available;
    }
}

public class InsufficientCreditException : DomainException
{
    public int CustomerId { get; }
    public Money RequiredAmount { get; }
    public Money AvailableCredit { get; }
    
    public InsufficientCreditException(int customerId, Money required, Money available)
        : base($"Insufficient credit for customer {customerId}")
    {
        CustomerId = customerId;
        RequiredAmount = required;
        AvailableCredit = available;
    }
}
```

---

## Enumerations

```csharp
public enum OrderStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    PartiallyApproved = 3,
    Rejected = 4,
    Processing = 5,
    Shipped = 6,
    Delivered = 7,
    Cancelled = 8
}

public enum OrderItemStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public enum PaymentMethod
{
    CreditCard = 1,
    BankTransfer = 2,
    OnCredit = 3,
    Cash = 4
}

public enum PaymentStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    PartiallyPaid = 4,
    Refunded = 5
}

public enum ReturnStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Rejected = 3,
    Shipped = 4,
    Received = 5,
    Refunded = 6
}

public enum UserRole
{
    Owner = 1,
    Purchasing = 2,
    Finance = 3,
    Viewer = 4
}

public enum TransactionType
{
    Debit = 1,
    Credit = 2
}
```

---

# APPLICATION LAYER SPECIFICATION

## Application Services

### ProductService

```csharp
public interface IProductService
{
    // Queries
    Task<ProductDto> GetByIdAsync(int id);
    Task<PaginatedList<ProductDto>> GetAllAsync(ProductQueryDto query);
    Task<Money> GetPriceForCustomerAsync(int productId, int customerId);
    
    // Commands
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto);
    Task UpdateStockAsync(int id, int quantity);
    Task UpdatePricingAsync(int id, UpdateProductPricingDto dto);
}

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPricingService _pricingService;
    
    public ProductService(IUnitOfWork unitOfWork, IMapper mapper, IPricingService pricingService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _pricingService = pricingService;
    }
    
    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        // Validate SKU uniqueness
        var existing = await _unitOfWork.Products.GetBySkuAsync(dto.Sku);
        if (existing != null)
            throw new ValidationException($"SKU {dto.Sku} already exists");
        
        // Create entity
        var product = Product.Create(
            sku: dto.Sku,
            name: dto.Name,
            categoryId: dto.CategoryId,
            brandId: dto.BrandId,
            listPrice: Money.Create(dto.ListPrice, dto.Currency),
            dealerPrice: Money.Create(dto.DealerPrice, dto.Currency),
            currency: dto.Currency,
            trackStock: dto.TrackStock,
            requiresSerialNumber: dto.RequiresSerialNumber,
            warrantyPeriodMonths: dto.WarrantyPeriodMonths
        );
        
        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        
        return _mapper.Map<ProductDto>(product);
    }
    
    public async Task<Money> GetPriceForCustomerAsync(int productId, int customerId)
    {
        var product = await _unitOfWork.Products.GetByIdWithPricesAsync(productId);
        if (product == null) throw new NotFoundException($"Product {productId} not found");
        
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
        if (customer == null) throw new NotFoundException($"Customer {customerId} not found");
        
        return await _pricingService.GetPriceForCustomerAsync(product, customer);
    }
}
```

### OrderService

```csharp
public interface IOrderService
{
    Task<OrderDto> CreateAsync(CreateOrderDto dto);
    Task ApproveOrderAsync(int orderId, ApproveOrderDto dto);
    Task ApproveOrderPartiallyAsync(int orderId, PartialApprovalDto dto);
    Task CancelOrderAsync(int orderId, string reason);
}

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICreditManagementService _creditManagement;
    private readonly ICurrencyConversionService _currencyConversion;
    
    public OrderService(IUnitOfWork unitOfWork, IMapper mapper,
        ICreditManagementService creditManagement, ICurrencyConversionService currencyConversion)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _creditManagement = creditManagement;
        _currencyConversion = currencyConversion;
    }
    
    public async Task<OrderDto> CreateAsync(CreateOrderDto dto)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(dto.CustomerId);
        if (customer == null) throw new NotFoundException("Customer not found");
        
        // Create order
        var order = Order.Create(
            customerId: dto.CustomerId,
            shippingAddressId: dto.ShippingAddressId,
            billingAddressId: dto.BillingAddressId,
            currency: dto.Currency,
            baseCurrency: "TRY", // Get from system config
            paymentMethod: dto.PaymentMethod,
            requiresApproval: customer.RequireOrderApproval
        );
        
        // Add items
        foreach (var itemDto in dto.Items)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId);
            if (product == null) throw new NotFoundException($"Product {itemDto.ProductId} not found");
            
            if (!product.CanBeOrdered(itemDto.Quantity))
                throw new BusinessRuleViolationException("Product not available");
            
            var price = await _pricingService.GetPriceForCustomerAsync(product, customer);
            
            order.AddItem(
                productId: product.Id,
                productName: product.Name,
                quantity: itemDto.Quantity,
                unitPrice: price,
                taxRate: 0.20m // Get from tax config
            );
        }
        
        // Validate credit
        if (!customer.RequiresPrepayment())
        {
            var canPlace = await _creditManagement.CanCustomerPlaceOrderAsync(customer, order.TotalAmount);
            if (!canPlace)
                throw new InsufficientCreditException(customer.Id, order.TotalAmount, 
                    await _creditManagement.GetAvailableCreditAsync(customer));
        }
        
        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();
        
        return _mapper.Map<OrderDto>(order);
    }
    
    public async Task ApproveOrderAsync(int orderId, ApproveOrderDto dto)
    {
        var order = await _unitOfWork.Orders.GetByIdWithDetailsAsync(orderId);
        if (order == null) throw new NotFoundException("Order not found");
        
        // Get exchange rate
        var rate = await _currencyConversion.GetExchangeRateAsync(
            order.Currency, order.BaseCurrency);
        
        order.Approve(dto.ApprovedByUserId, rate);
        
        await _unitOfWork.Orders.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();
    }
}
```

---

## DTOs

```csharp
// Product DTOs
public class ProductDto
{
    public int Id { get; set; }
    public string Sku { get; set; }
    public string Name { get; set; }
    public decimal ListPrice { get; set; }
    public decimal DealerPrice { get; set; }
    public string Currency { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public string CategoryName { get; set; }
    public string BrandName { get; set; }
}

public class CreateProductDto
{
    public string Sku { get; set; }
    public string Name { get; set; }
    public int CategoryId { get; set; }
    public int BrandId { get; set; }
    public decimal ListPrice { get; set; }
    public decimal DealerPrice { get; set; }
    public string Currency { get; set; }
    public bool TrackStock { get; set; } = true;
    public bool RequiresSerialNumber { get; set; }
    public int? WarrantyPeriodMonths { get; set; }
}

public class ProductQueryDto : PaginationQuery
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public bool? IsActive { get; set; }
}

// Order DTOs
public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; }
    public string CustomerName { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; }
    public List<OrderItemDto> Items { get; set; }
}

public class CreateOrderDto
{
    public int CustomerId { get; set; }
    public int ShippingAddressId { get; set; }
    public int BillingAddressId { get; set; }
    public string Currency { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public List<CreateOrderItemDto> Items { get; set; }
}

public class CreateOrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class ApproveOrderDto
{
    public int ApprovedByUserId { get; set; }
}

public class PartialApprovalDto
{
    public int ApprovedByUserId { get; set; }
    public List<int> ApprovedItemIds { get; set; }
    public Dictionary<int, string> RejectedItemReasons { get; set; }
}

// Common
public class PaginationQuery
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class PaginatedList<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
```

---

## Validators

```csharp
public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(50)
            .Matches("^[A-Z0-9-]+$");
        
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
        
        RuleFor(x => x.ListPrice)
            .GreaterThan(0);
        
        RuleFor(x => x.DealerPrice)
            .GreaterThan(0)
            .LessThanOrEqualTo(x => x.ListPrice);
        
        When(x => x.RequiresSerialNumber, () =>
        {
            RuleFor(x => x.WarrantyPeriodMonths)
                .NotNull()
                .GreaterThan(0);
        });
    }
}

public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new CreateOrderItemValidator());
    }
}

public class CreateOrderItemValidator : AbstractValidator<CreateOrderItemDto>
{
    public CreateOrderItemValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(10000);
    }
}
```

---

## AutoMapper Profiles

```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Product
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name))
            .ForMember(d => d.BrandName, o => o.MapFrom(s => s.Brand.Name))
            .ForMember(d => d.ListPrice, o => o.MapFrom(s => s.ListPrice.Amount))
            .ForMember(d => d.DealerPrice, o => o.MapFrom(s => s.DealerPrice.Amount));
        
        // Order
        CreateMap<Order, OrderDto>()
            .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer.CompanyName))
            .ForMember(d => d.TotalAmount, o => o.MapFrom(s => s.TotalAmount.Amount));
        
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(d => d.UnitPrice, o => o.MapFrom(s => s.UnitPrice.Amount))
            .ForMember(d => d.TotalPrice, o => o.MapFrom(s => s.TotalPrice.Amount));
    }
}
```

---

## Event Handlers

```csharp
// Order Approved Handler
public class OrderApprovedEventHandler : INotificationHandler<OrderApprovedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    
    public OrderApprovedEventHandler(IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }
    
    public async Task Handle(OrderApprovedEvent notification, CancellationToken cancellationToken)
    {
        var order = notification.Order;
        
        // 1. Reserve stock
        foreach (var item in order.OrderItems.Where(i => i.ItemStatus == OrderItemStatus.Approved))
        {
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
            if (product?.TrackStock == true)
            {
                product.ReserveStock(item.Quantity);
            }
        }
        
        // 2. Use customer credit
        var customer = await _unitOfWork.Customers.GetByIdAsync(order.CustomerId);
        if (customer?.CreditLimit.Amount > 0)
        {
            customer.UseCreditForOrder(order.GetTotalInBaseCurrency());
        }
        
        // 3. Create transaction
        var transaction = CurrentAccountTransaction.CreateDebit(
            order.CustomerId, order.TotalAmount, $"Order {order.OrderNumber}", order.Id);
        await _unitOfWork.CurrentAccountTransactions.AddAsync(transaction);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // 4. Send email
        await _emailService.SendOrderApprovedEmailAsync(order);
    }
}

// Order Cancelled Handler
public class OrderCancelledEventHandler : INotificationHandler<OrderCancelledEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    
    public OrderCancelledEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task Handle(OrderCancelledEvent notification, CancellationToken cancellationToken)
    {
        var order = notification.Order;
        
        // Release stock
        foreach (var item in order.OrderItems)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
            if (product?.TrackStock == true)
            {
                product.ReleaseStock(item.Quantity);
            }
        }
        
        // Release credit
        if (order.Status == OrderStatus.Approved || order.Status == OrderStatus.PartiallyApproved)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(order.CustomerId);
            customer?.ReleaseCreditForOrder(order.GetTotalInBaseCurrency());
        }
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

// Payment Completed Handler
public class PaymentCompletedEventHandler : INotificationHandler<PaymentCompletedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    
    public PaymentCompletedEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task Handle(PaymentCompletedEvent notification, CancellationToken cancellationToken)
    {
        var payment = notification.Payment;
        
        // Create credit transaction
        var transaction = CurrentAccountTransaction.CreateCredit(
            payment.CustomerId, payment.Amount, 
            $"Payment for Order {payment.Order.OrderNumber}",
            payment.Id, payment.TransactionId);
        await _unitOfWork.CurrentAccountTransactions.AddAsync(transaction);
        
        // Update customer credit
        var customer = await _unitOfWork.Customers.GetByIdAsync(payment.CustomerId);
        customer?.RecordPayment(payment.Amount);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
```

---

## Business Rules Summary

### 1. Product Pricing
- Customer-specific prices override dealer prices
- Dealer price ≤ List price
- Prices valid for effective date

### 2. Order Approval (Parametric)
- **System Setting**: RequireOrderApproval (true/false)
- **Customer Setting**: RequireOrderApproval (true/false)
- **Customer Setting**: AllowPartialOrderApproval (true/false)
- Exchange rate locked at approval time
- Stock reserved when approved

### 3. Return Approval (Parametric)
- **System Setting**: RequireReturnApproval (true/false)
- **Customer Setting**: RequireReturnApproval (true/false)
- Returns are always partial (per item)

### 4. Credit Management
- Available Credit = Limit - Used - Pending Orders
- Orders without credit require prepayment
- Overdue rate: customer-specific > system-wide
- Account suspension is manual

### 5. Stock Management
- Reserved when order approved
- Released when order cancelled
- Serial tracking per product
- Warranty from product settings

### 6. Currency Handling
- Product has base currency
- Order uses customer's preferred currency
- Exchange rate locked at approval
- Order total converted to base currency for credit

### 7. Payment
- Commission rates configurable
- Multiple payment methods
- Auto-credit update on completion

---

## Repository Interfaces

```csharp
public interface IProductRepository : IGenericRepository<Product>
{
    Task<Product?> GetBySkuAsync(string sku);
    Task<Product?> GetByIdWithPricesAsync(int id);
    Task<PaginatedList<Product>> GetAllAsync(ProductQueryDto query);
}

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<Order?> GetByIdWithDetailsAsync(int id);
    Task<List<Order>> GetPendingOrdersByCustomerAsync(int customerId);
}

public interface ICustomerRepository : IGenericRepository<Customer>
{
    Task<Customer?> GetByTaxNumberAsync(string taxNumber);
}

public interface ICurrencyRateRepository : IGenericRepository<CurrencyRate>
{
    Task<CurrencyRate?> GetRateAsync(string from, string to, DateTime date);
}

public interface ISystemConfigurationRepository : IGenericRepository<SystemConfiguration>
{
    Task<T?> GetValueAsync<T>(string key);
}
```

---

## External Service Interfaces

```csharp
public interface IEmailService
{
    Task SendOrderApprovedEmailAsync(Order order);
    Task SendOrderCancelledEmailAsync(Order order, string reason);
    Task SendPaymentConfirmationEmailAsync(Payment payment);
}

public interface IErpIntegrationService
{
    Task SendOrderToErpAsync(Order order);
    Task NotifyStockChangeAsync(int productId, string sku, int oldQty, int newQty);
}

public interface IPaymentGatewayService
{
    Task<PaymentInitiationResult> InitiatePaymentAsync(Payment payment);
    Task<PaymentVerificationResult> VerifyPaymentAsync(string transactionId);
}
```

---

## Implementation Checklist

### Domain Layer
- [ ] BaseEntity with audit fields
- [ ] IAggregateRoot marker interface
- [ ] Product aggregate with business methods
- [ ] Customer aggregate with credit management
- [ ] Order aggregate with approval workflow
- [ ] Value objects (Money, Email, PhoneNumber, TaxNumber)
- [ ] Domain events for all major operations
- [ ] Domain services (Pricing, Credit, Currency)
- [ ] Domain exceptions
- [ ] All enumerations

### Application Layer
- [ ] Service interfaces
- [ ] Service implementations with business logic
- [ ] DTOs for all operations
- [ ] AutoMapper profiles
- [ ] FluentValidation validators
- [ ] MediatR event handlers
- [ ] Application exceptions

### Testing
- [ ] Unit tests for domain entities
- [ ] Unit tests for domain services
- [ ] Unit tests for application services
- [ ] Integration tests for repositories

---

**Document Version**: 1.0  
**Created**: November 2025  
**Target**: Claude Code Implementation  
**Framework**: .NET 8, EF Core 8, Clean Architecture

