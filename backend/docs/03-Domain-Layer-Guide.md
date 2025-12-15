# Domain Layer Guide

## Overview

The Domain layer is the **heart of the application**. It contains all business logic, entities, value objects, and domain events. This layer has **zero external dependencies**.

## Namespace

```csharp
namespace B2BCommerce.Backend.Domain.Entities
namespace B2BCommerce.Backend.Domain.ValueObjects
namespace B2BCommerce.Backend.Domain.Events
namespace B2BCommerce.Backend.Domain.Services
namespace B2BCommerce.Backend.Domain.Exceptions
namespace B2BCommerce.Backend.Domain.Enums
namespace B2BCommerce.Backend.Domain.Common
```

## Base Classes

### BaseEntity

All entities inherit from `BaseEntity` for common audit fields.

```csharp
// File: Domain/Common/BaseEntity.cs
namespace B2BCommerce.Backend.Domain.Common
{
    public abstract class BaseEntity
    {
        public int Id { get; protected set; }
        
        // Audit fields
        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
        public string? CreatedBy { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }
        public string? UpdatedBy { get; protected set; }
        
        // Soft delete
        public bool IsDeleted { get; protected set; }
        public DateTime? DeletedAt { get; protected set; }
        public string? DeletedBy { get; protected set; }
        
        // Domain events
        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
        
        public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
        public void ClearDomainEvents() => _domainEvents.Clear();
        
        // Soft delete method
        public virtual void Delete(string? deletedBy = null)
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            DeletedBy = deletedBy;
        }
    }
}
```

### ExternalEntity

For entities that originate from external systems (LOGO ERP).

```csharp
// File: Domain/Common/ExternalEntity.cs
namespace B2BCommerce.Backend.Domain.Common
{
    public abstract class ExternalEntity : BaseEntity, IExternalEntity
    {
        public string ExternalCode { get; protected set; } = null!;
        public string? ExternalId { get; protected set; }
        public DateTime? LastSyncedAt { get; protected set; }
        
        public void MarkAsSynced(DateTime? syncTime = null)
        {
            LastSyncedAt = syncTime ?? DateTime.UtcNow;
        }
    }
}
```

### IAggregateRoot

Marker interface for aggregate roots.

```csharp
// File: Domain/Common/IAggregateRoot.cs
namespace B2BCommerce.Backend.Domain.Common
{
    /// <summary>
    /// Marker interface for aggregate roots
    /// </summary>
    public interface IAggregateRoot { }
}
```

## Entity Guidelines

### Use Private Setters

Protect entity state from external modification.

```csharp
public class Product : ExternalEntity, IAggregateRoot
{
    // ✅ CORRECT: Private setter
    public string Name { get; private set; }
    
    // ❌ WRONG: Public setter
    public string Name { get; set; }
}
```

### Use Factory Methods

Create entities through factory methods, not constructors.

```csharp
public class Product : ExternalEntity, IAggregateRoot
{
    // Private constructor prevents direct instantiation
    private Product() { }
    
    /// <summary>
    /// Create product from external system (LOGO ERP)
    /// </summary>
    public static Product CreateFromExternal(
        string externalCode,
        string name,
        string sku,
        int categoryId,
        int brandId,
        decimal listPrice,
        decimal dealerPrice,
        string currency = "TRY",
        string? externalId = null)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(externalCode))
            throw new DomainException("External code is required");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name is required");
        if (string.IsNullOrWhiteSpace(sku))
            throw new DomainException("SKU is required");
        if (listPrice < 0)
            throw new DomainException("List price cannot be negative");
        if (dealerPrice > listPrice)
            throw new DomainException("Dealer price cannot exceed list price");
        
        var product = new Product
        {
            ExternalCode = externalCode,
            ExternalId = externalId,
            Name = name,
            Sku = sku,
            CategoryId = categoryId,
            BrandId = brandId,
            ListPrice = listPrice,
            DealerPrice = dealerPrice,
            Currency = currency,
            IsActive = true,
            LastSyncedAt = DateTime.UtcNow
        };
        
        product.AddDomainEvent(new ProductCreatedEvent(product.Id, product.Sku));
        return product;
    }
    
    /// <summary>
    /// Create product internally (Admin panel)
    /// </summary>
    public static Product Create(
        string name,
        string sku,
        int categoryId,
        int brandId,
        decimal listPrice,
        decimal dealerPrice,
        string currency = "TRY")
    {
        // Use SKU as external code for internal creates
        return CreateFromExternal(sku, name, sku, categoryId, brandId, 
            listPrice, dealerPrice, currency);
    }
}
```

### Use Behavior Methods

Encapsulate state changes with business rules.

```csharp
public class Product : ExternalEntity
{
    // Properties
    public int StockQuantity { get; private set; }
    public bool TrackStock { get; private set; }
    
    // Behavior methods
    public void ReserveStock(int quantity)
    {
        if (!TrackStock) return;
        
        if (StockQuantity < quantity)
            throw new InsufficientStockException(Id, quantity, StockQuantity);
        
        StockQuantity -= quantity;
        AddDomainEvent(new StockReservedEvent(Id, quantity));
    }
    
    public void ReleaseStock(int quantity)
    {
        if (!TrackStock) return;
        
        StockQuantity += quantity;
        AddDomainEvent(new StockReleasedEvent(Id, quantity));
    }
    
    public void UpdatePricing(decimal listPrice, decimal dealerPrice)
    {
        if (dealerPrice > listPrice)
            throw new DomainException("Dealer price cannot exceed list price");
        
        ListPrice = listPrice;
        DealerPrice = dealerPrice;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new ProductPriceChangedEvent(Id, listPrice, dealerPrice));
    }
    
    public bool CanBeOrdered(int quantity) =>
        IsActive && !IsDeleted && (!TrackStock || StockQuantity >= quantity);
}
```

### Update Methods

For external updates, provide specific update methods.

```csharp
public class Product : ExternalEntity
{
    /// <summary>
    /// Update from external sync
    /// </summary>
    public void UpdateFromExternal(
        string name,
        int categoryId,
        int brandId,
        decimal listPrice,
        decimal dealerPrice,
        string currency,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name is required");
        if (dealerPrice > listPrice)
            throw new DomainException("Dealer price cannot exceed list price");
        
        Name = name;
        CategoryId = categoryId;
        BrandId = brandId;
        ListPrice = listPrice;
        DealerPrice = dealerPrice;
        Currency = currency;
        Description = description;
        
        MarkAsSynced();
        AddDomainEvent(new ProductUpdatedEvent(Id));
    }
}
```

## Value Objects

Value objects are immutable and compared by value, not identity.

```csharp
// File: Domain/ValueObjects/Money.cs
namespace B2BCommerce.Backend.Domain.ValueObjects
{
    public sealed class Money : IEquatable<Money>
    {
        public decimal Amount { get; }
        public string Currency { get; }
        
        private Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }
        
        public static Money Create(decimal amount, string currency)
        {
            if (amount < 0)
                throw new DomainException("Amount cannot be negative");
            if (string.IsNullOrWhiteSpace(currency))
                throw new DomainException("Currency is required");
            
            return new Money(amount, currency.ToUpperInvariant());
        }
        
        public static Money Zero(string currency) => Create(0, currency);
        
        public Money Add(Money other)
        {
            if (Currency != other.Currency)
                throw new DomainException("Cannot add different currencies");
            
            return Create(Amount + other.Amount, Currency);
        }
        
        public Money Subtract(Money other)
        {
            if (Currency != other.Currency)
                throw new DomainException("Cannot subtract different currencies");
            
            return Create(Amount - other.Amount, Currency);
        }
        
        // Equality
        public bool Equals(Money? other)
        {
            if (other is null) return false;
            return Amount == other.Amount && Currency == other.Currency;
        }
        
        public override bool Equals(object? obj) => Equals(obj as Money);
        
        public override int GetHashCode() => HashCode.Combine(Amount, Currency);
        
        public static bool operator ==(Money? left, Money? right) =>
            left?.Equals(right) ?? right is null;
        
        public static bool operator !=(Money? left, Money? right) =>
            !(left == right);
        
        public override string ToString() => $"{Amount:N2} {Currency}";
    }
}
```

```csharp
// File: Domain/ValueObjects/Email.cs
namespace B2BCommerce.Backend.Domain.ValueObjects
{
    public sealed class Email : IEquatable<Email>
    {
        public string Value { get; }
        
        private Email(string value)
        {
            Value = value;
        }
        
        public static Email Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new DomainException("Email is required");
            
            email = email.Trim().ToLowerInvariant();
            
            if (!IsValidEmail(email))
                throw new DomainException("Invalid email format");
            
            return new Email(email);
        }
        
        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        
        public bool Equals(Email? other) => Value == other?.Value;
        public override bool Equals(object? obj) => Equals(obj as Email);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value;
        
        public static implicit operator string(Email email) => email.Value;
    }
}
```

## Domain Events

Domain events signal that something important happened in the domain.

```csharp
// File: Domain/Events/IDomainEvent.cs
namespace B2BCommerce.Backend.Domain.Events
{
    public interface IDomainEvent
    {
        DateTime OccurredAt { get; }
    }
    
    public abstract class DomainEvent : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
}

// File: Domain/Events/ProductCreatedEvent.cs
namespace B2BCommerce.Backend.Domain.Events
{
    public class ProductCreatedEvent : DomainEvent
    {
        public int ProductId { get; }
        public string Sku { get; }
        
        public ProductCreatedEvent(int productId, string sku)
        {
            ProductId = productId;
            Sku = sku;
        }
    }
}

// File: Domain/Events/OrderSubmittedEvent.cs
namespace B2BCommerce.Backend.Domain.Events
{
    public class OrderSubmittedEvent : DomainEvent
    {
        public int OrderId { get; }
        public int CustomerId { get; }
        public decimal TotalAmount { get; }
        
        public OrderSubmittedEvent(int orderId, int customerId, decimal totalAmount)
        {
            OrderId = orderId;
            CustomerId = customerId;
            TotalAmount = totalAmount;
        }
    }
}
```

## Domain Exceptions

Use specific exceptions for domain rule violations.

```csharp
// File: Domain/Exceptions/DomainException.cs
namespace B2BCommerce.Backend.Domain.Exceptions
{
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
    }
}

// File: Domain/Exceptions/InsufficientStockException.cs
namespace B2BCommerce.Backend.Domain.Exceptions
{
    public class InsufficientStockException : DomainException
    {
        public int ProductId { get; }
        public int RequestedQuantity { get; }
        public int AvailableQuantity { get; }
        
        public InsufficientStockException(int productId, int requested, int available)
            : base($"Insufficient stock for product {productId}. " +
                   $"Requested: {requested}, Available: {available}")
        {
            ProductId = productId;
            RequestedQuantity = requested;
            AvailableQuantity = available;
        }
    }
}

// File: Domain/Exceptions/InsufficientCreditException.cs
namespace B2BCommerce.Backend.Domain.Exceptions
{
    public class InsufficientCreditException : DomainException
    {
        public int CustomerId { get; }
        public decimal RequiredAmount { get; }
        public decimal AvailableCredit { get; }
        
        public InsufficientCreditException(int customerId, decimal required, decimal available)
            : base($"Insufficient credit for customer {customerId}. " +
                   $"Required: {required:C}, Available: {available:C}")
        {
            CustomerId = customerId;
            RequiredAmount = required;
            AvailableCredit = available;
        }
    }
}
```

## Enumerations

```csharp
// File: Domain/Enums/OrderStatus.cs
namespace B2BCommerce.Backend.Domain.Enums
{
    public enum OrderStatus
    {
        Draft = 0,
        Submitted = 1,
        PendingApproval = 2,
        Approved = 3,
        PartiallyApproved = 4,
        Rejected = 5,
        Processing = 6,
        Shipped = 7,
        Delivered = 8,
        Cancelled = 9
    }
}

// File: Domain/Enums/PaymentStatus.cs
namespace B2BCommerce.Backend.Domain.Enums
{
    public enum PaymentStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3,
        Refunded = 4,
        PartiallyRefunded = 5
    }
}

// File: Domain/Enums/UserRole.cs
namespace B2BCommerce.Backend.Domain.Enums
{
    public enum UserRole
    {
        Owner = 1,
        Purchasing = 2,
        Finance = 3,
        Viewer = 4
    }
}
```

## Domain Services

For operations that don't naturally belong to a single entity.

```csharp
// File: Domain/Services/IPricingService.cs
namespace B2BCommerce.Backend.Domain.Services
{
    /// <summary>
    /// Domain service for complex pricing calculations
    /// </summary>
    public interface IPricingService
    {
        Money CalculateEffectivePrice(
            Product product, 
            Customer customer, 
            int quantity);
    }
}
```

## Complete Entity Example

```csharp
// File: Domain/Entities/Order.cs
namespace B2BCommerce.Backend.Domain.Entities
{
    public class Order : BaseEntity, IAggregateRoot
    {
        private Order() { }
        
        #region Properties
        
        public string OrderNumber { get; private set; } = null!;
        public int CustomerId { get; private set; }
        public OrderStatus Status { get; private set; }
        
        public decimal SubTotal { get; private set; }
        public decimal TaxAmount { get; private set; }
        public decimal DiscountAmount { get; private set; }
        public decimal TotalAmount { get; private set; }
        public string Currency { get; private set; } = "TRY";
        
        public decimal? LockedExchangeRate { get; private set; }
        
        public string? Notes { get; private set; }
        public DateTime? ApprovedAt { get; private set; }
        public string? ApprovedBy { get; private set; }
        
        // Navigation
        public Customer Customer { get; private set; } = null!;
        private readonly List<OrderItem> _items = new();
        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
        
        #endregion
        
        #region Factory Methods
        
        public static Order Create(
            int customerId,
            string currency,
            string? notes = null)
        {
            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                CustomerId = customerId,
                Currency = currency,
                Notes = notes,
                Status = OrderStatus.Draft
            };
            
            return order;
        }
        
        private static string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        }
        
        #endregion
        
        #region Behavior Methods
        
        public void AddItem(int productId, string sku, string productName,
            int quantity, decimal unitPrice, int? taxRate = null)
        {
            if (Status != OrderStatus.Draft)
                throw new DomainException("Cannot add items to non-draft order");
            
            var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
            
            if (existingItem != null)
            {
                existingItem.UpdateQuantity(existingItem.Quantity + quantity);
            }
            else
            {
                var item = OrderItem.Create(Id, productId, sku, productName, 
                    quantity, unitPrice, taxRate);
                _items.Add(item);
            }
            
            RecalculateTotals();
        }
        
        public void RemoveItem(int productId)
        {
            if (Status != OrderStatus.Draft)
                throw new DomainException("Cannot remove items from non-draft order");
            
            var item = _items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                _items.Remove(item);
                RecalculateTotals();
            }
        }
        
        public void Submit()
        {
            if (Status != OrderStatus.Draft)
                throw new DomainException("Only draft orders can be submitted");
            
            if (!_items.Any())
                throw new DomainException("Cannot submit empty order");
            
            Status = OrderStatus.Submitted;
            AddDomainEvent(new OrderSubmittedEvent(Id, CustomerId, TotalAmount));
        }
        
        public void Approve(string approvedBy, decimal? exchangeRate = null)
        {
            if (Status != OrderStatus.Submitted && Status != OrderStatus.PendingApproval)
                throw new DomainException("Order cannot be approved in current status");
            
            Status = OrderStatus.Approved;
            ApprovedAt = DateTime.UtcNow;
            ApprovedBy = approvedBy;
            LockedExchangeRate = exchangeRate;
            
            foreach (var item in _items)
            {
                item.Approve();
            }
            
            AddDomainEvent(new OrderApprovedEvent(this));
        }
        
        public void Cancel(string reason, string cancelledBy)
        {
            if (Status == OrderStatus.Shipped || Status == OrderStatus.Delivered)
                throw new DomainException("Cannot cancel shipped or delivered order");
            
            Status = OrderStatus.Cancelled;
            AddDomainEvent(new OrderCancelledEvent(this, reason));
        }
        
        private void RecalculateTotals()
        {
            SubTotal = _items.Sum(i => i.LineTotal);
            TaxAmount = _items.Sum(i => i.TaxAmount);
            TotalAmount = SubTotal + TaxAmount - DiscountAmount;
        }
        
        #endregion
    }
}
```

## Rules Summary

| Rule | Description |
|------|-------------|
| No external dependencies | Only .NET BCL allowed |
| Private setters | Protect entity state |
| Factory methods | Control entity creation |
| Behavior methods | Encapsulate state changes |
| Domain events | Signal important changes |
| Value objects | Immutable, compared by value |
| Aggregate roots | Entry point for entity graphs |
| Domain exceptions | Specific business rule violations |

---

**Next**: [04-Application-Layer-Guide](04-Application-Layer-Guide.md)
