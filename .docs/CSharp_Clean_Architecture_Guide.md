# C# Clean Architecture Implementation Guide

## Document Purpose

This document provides **detailed implementation guidelines** for Clean
Architecture in C# .NET projects. It serves as a reference for all backend
projects in the B2B E-Commerce solution, ensuring consistency, maintainability,
and adherence to best practices.

**This is a project-wide reference document** that should be consulted when
implementing any .NET backend project.

---

## Table of Contents

1. [Clean Architecture Overview](#clean-architecture-overview)
2. [Layer Responsibilities](#layer-responsibilities)
3. [Dependency Rules](#dependency-rules)
4. [Project Structure](#project-structure)
5. [Domain Layer Implementation](#domain-layer-implementation)
6. [Application Layer Implementation](#application-layer-implementation)
7. [Infrastructure Layer Implementation](#infrastructure-layer-implementation)
8. [Presentation/API Layer Implementation](#presentationapi-layer-implementation)
9. [Cross-Cutting Concerns](#cross-cutting-concerns)
10. [Coding Standards & Conventions](#coding-standards--conventions)
11. [Design Patterns & Best Practices](#design-patterns--best-practices)
12. [Testing Strategy](#testing-strategy)
13. [Common Pitfalls & Solutions](#common-pitfalls--solutions)

---

## Clean Architecture Overview

### Core Principles

Clean Architecture, also known as Onion Architecture or Hexagonal Architecture,
is based on these principles:

1. **Independence of Frameworks**: Business logic doesn't depend on external
   libraries
2. **Testability**: Business logic can be tested without UI, database, or
   external services
3. **Independence of UI**: UI can change without affecting business logic
4. **Independence of Database**: Can swap databases without changing business
   rules
5. **Independence of External Services**: Business logic doesn't know about
   external APIs

### The Dependency Rule

**Dependencies must point inward toward the domain**

```
┌─────────────────────────────────────────────────┐
│         Presentation/API Layer                  │
│         (Controllers, Middleware)               │
│              Dependencies: ↓                    │
└─────────────────┬───────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────┐
│         Infrastructure Layer                    │
│    (EF Core, Repositories, External APIs)       │
│              Dependencies: ↓                    │
└─────────────────┬───────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────┐
│         Application Layer                       │
│    (Services, DTOs, Interfaces, Validation)     │
│              Dependencies: ↓                    │
└─────────────────┬───────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────┐
│         Domain Layer                            │
│    (Entities, Value Objects, Rules)             │
│         NO DEPENDENCIES                         │
└─────────────────────────────────────────────────┘
```

**Key Point**: Inner layers never depend on outer layers. Outer layers can
depend on inner layers.

---

## Layer Responsibilities

### Domain Layer (Core)

**What it contains**: Pure business logic and domain models

**Responsibilities**:

- Define domain entities
- Define value objects
- Define domain events
- Implement business rules
- Define domain exceptions
- Define enumerations

**What it CANNOT do**:

- ❌ Reference any other layer
- ❌ Reference external libraries (except .NET BCL)
- ❌ Know about databases
- ❌ Know about HTTP/UI
- ❌ Know about external services

---

### Application Layer

**What it contains**: Application business logic and use cases

**Responsibilities**:

- Define service interfaces
- Implement business workflows (use cases)
- Define DTOs (Data Transfer Objects)
- Define repository interfaces
- Implement validation rules
- Map between DTOs and entities
- Define application exceptions
- Orchestrate domain operations

**What it CANNOT do**:

- ❌ Know about HTTP/UI specifics
- ❌ Know about database implementation details
- ❌ Know about external service implementation details
- ❌ Contain UI logic

---

### Infrastructure Layer

**What it contains**: External concerns and technical implementations

**Responsibilities**:

- Implement repository interfaces
- Implement database access (EF Core)
- Implement external API clients
- Implement file storage
- Implement caching
- Implement email/SMS services
- Implement background jobs

**What it CAN do**:

- ✅ Reference Application and Domain layers
- ✅ Reference external libraries (EF Core, HTTP clients, AWS SDK, etc.)
- ✅ Know about database specifics
- ✅ Know about external service APIs

---

### Presentation/API Layer

**What it contains**: HTTP/API concerns and user interface

**Responsibilities**:

- Define API endpoints (Controllers)
- Handle HTTP requests/responses
- Implement authentication/authorization
- Implement middleware
- Define API models/ViewModels
- Handle model validation (basic)
- Configure dependency injection
- Configure Swagger/OpenAPI

**What it CAN do**:

- ✅ Reference Application and Infrastructure layers
- ✅ Know about HTTP specifics
- ✅ Handle authentication tokens
- ✅ Return appropriate HTTP status codes

**What it CANNOT do**:

- ❌ Contain business logic
- ❌ Directly access database
- ❌ Know about domain entities structure (use DTOs)

---

## Dependency Rules

### Allowed Dependencies

```
Domain Layer:
  ↓ NO dependencies (pure C#)

Application Layer:
  ↓ Domain Layer ONLY
  ↓ AutoMapper (for mapping)
  ↓ FluentValidation (for validation)

Infrastructure Layer:
  ↓ Domain Layer
  ↓ Application Layer
  ↓ EF Core
  ↓ External SDKs (AWS, payment gateways, etc.)

API/Presentation Layer:
  ↓ Application Layer
  ↓ Infrastructure Layer (for DI registration only)
  ↓ ASP.NET Core libraries
```

### Dependency Inversion Principle

When Application Layer needs infrastructure functionality, use interfaces:

```csharp
// ✅ CORRECT: Application defines interface
// File: Application/Interfaces/IProductRepository.cs
namespace Application.Interfaces
{
    public interface IProductRepository
    {
        Task<Product> GetByIdAsync(int id);
        Task<List<Product>> GetAllAsync();
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
    }
}

// ✅ CORRECT: Infrastructure implements interface
// File: Infrastructure/Repositories/ProductRepository.cs
namespace Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;
        
        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }
        
        // ... other methods
    }
}

// ✅ CORRECT: Service depends on interface, not implementation
// File: Application/Services/ProductService.cs
namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        
        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
        
        // ... service methods
    }
}
```

---

## Project Structure

### Recommended Folder Structure

```
B2BCommerce.Backend/
│
├── src/
│   │
│   ├── B2BCommerce.Backend.Domain/
│   │   ├── Entities/
│   │   │   ├── Product.cs
│   │   │   ├── Order.cs
│   │   │   ├── Customer.cs
│   │   │   └── ...
│   │   ├── ValueObjects/
│   │   │   ├── Money.cs
│   │   │   ├── Address.cs
│   │   │   ├── Email.cs
│   │   │   └── ...
│   │   ├── Enums/
│   │   │   ├── OrderStatus.cs
│   │   │   ├── PaymentMethod.cs
│   │   │   └── ...
│   │   ├── Events/
│   │   │   ├── OrderPlacedEvent.cs
│   │   │   ├── PaymentReceivedEvent.cs
│   │   │   └── ...
│   │   ├── Exceptions/
│   │   │   ├── DomainException.cs
│   │   │   ├── InsufficientStockException.cs
│   │   │   └── ...
│   │   └── Common/
│   │       ├── BaseEntity.cs
│   │       ├── IAggregateRoot.cs
│   │       └── ...
│   │
│   ├── B2BCommerce.Backend.Application/
│   │   ├── Interfaces/
│   │   │   ├── Repositories/
│   │   │   │   ├── IProductRepository.cs
│   │   │   │   ├── IOrderRepository.cs
│   │   │   │   └── ...
│   │   │   ├── Services/
│   │   │   │   ├── IProductService.cs
│   │   │   │   ├── IOrderService.cs
│   │   │   │   └── ...
│   │   │   └── IUnitOfWork.cs
│   │   ├── Services/
│   │   │   ├── ProductService.cs
│   │   │   ├── OrderService.cs
│   │   │   ├── PricingService.cs
│   │   │   └── ...
│   │   ├── DTOs/
│   │   │   ├── Products/
│   │   │   │   ├── ProductDto.cs
│   │   │   │   ├── CreateProductDto.cs
│   │   │   │   ├── UpdateProductDto.cs
│   │   │   │   └── ...
│   │   │   ├── Orders/
│   │   │   └── ...
│   │   ├── Mapping/
│   │   │   ├── MappingProfile.cs
│   │   │   └── ...
│   │   ├── Validators/
│   │   │   ├── CreateProductValidator.cs
│   │   │   ├── CreateOrderValidator.cs
│   │   │   └── ...
│   │   ├── Exceptions/
│   │   │   ├── ApplicationException.cs
│   │   │   ├── NotFoundException.cs
│   │   │   ├── ValidationException.cs
│   │   │   └── ...
│   │   └── Common/
│   │       ├── PaginatedList.cs
│   │       ├── Result.cs
│   │       └── ...
│   │
│   ├── B2BCommerce.Backend.Infrastructure/
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   ├── ProductConfiguration.cs
│   │   │   │   ├── OrderConfiguration.cs
│   │   │   │   └── ...
│   │   │   ├── Migrations/
│   │   │   └── UnitOfWork.cs
│   │   ├── Repositories/
│   │   │   ├── GenericRepository.cs
│   │   │   ├── ProductRepository.cs
│   │   │   ├── OrderRepository.cs
│   │   │   └── ...
│   │   ├── Services/
│   │   │   ├── EmailService.cs
│   │   │   ├── SmsService.cs
│   │   │   ├── FileStorageService.cs
│   │   │   └── ...
│   │   ├── ExternalApis/
│   │   │   ├── PaymentGatewayClient.cs
│   │   │   ├── ShippingApiClient.cs
│   │   │   └── ...
│   │   ├── Caching/
│   │   │   ├── RedisCacheService.cs
│   │   │   └── ...
│   │   └── BackgroundJobs/
│   │       ├── SyncProductsJob.cs
│   │       └── ...
│   │
│   ├── B2BCommerce.Backend.Api/ (Web API Project)
│   │   ├── Controllers/
│   │   │   ├── ProductsController.cs
│   │   │   ├── OrdersController.cs
│   │   │   └── ...
│   │   ├── Middleware/
│   │   │   ├── ErrorHandlingMiddleware.cs
│   │   │   ├── JwtMiddleware.cs
│   │   │   └── ...
│   │   ├── Filters/
│   │   │   ├── AuthorizationFilter.cs
│   │   │   ├── ValidationFilter.cs
│   │   │   └── ...
│   │   ├── Models/ (API-specific models)
│   │   │   ├── ApiResponse.cs
│   │   │   ├── ErrorResponse.cs
│   │   │   └── ...
│   │   ├── Extensions/
│   │   │   ├── ServiceCollectionExtensions.cs
│   │   │   └── ...
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   └── B2BCommerce.Backend.Shared/
│       ├── Constants/
│       ├── Extensions/
│       ├── Helpers/
│       └── ...
│
└── tests/
    ├── B2BCommerce.Backend.Domain.Tests/
    ├── B2BCommerce.Backend.Application.Tests/
    ├── B2BCommerce.Backend.Infrastructure.Tests/
    └── B2BCommerce.Backend.Api.Tests/
```

---

## Domain Layer Implementation

### Base Entity

All entities should inherit from a base entity class:

```csharp
// File: Domain/Common/BaseEntity.cs
namespace B2BCommerce.Backend.Domain.Common
{
    public abstract class BaseEntity
    {
        public int Id { get; protected set; }
        
        // Audit fields
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        
        // Soft delete
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        
        // Domain events
        private List<DomainEvent>? _domainEvents;
        public IReadOnlyCollection<DomainEvent>? DomainEvents => _domainEvents?.AsReadOnly();
        
        public void AddDomainEvent(DomainEvent domainEvent)
        {
            _domainEvents ??= new List<DomainEvent>();
            _domainEvents.Add(domainEvent);
        }
        
        public void RemoveDomainEvent(DomainEvent domainEvent)
        {
            _domainEvents?.Remove(domainEvent);
        }
        
        public void ClearDomainEvents()
        {
            _domainEvents?.Clear();
        }
    }
}
```

### Domain Entity Example

```csharp
// File: Domain/Entities/Product.cs
namespace B2BCommerce.Backend.Domain.Entities
{
    public class Product : BaseEntity, IAggregateRoot
    {
        // Private constructor for EF Core
        private Product() { }
        
        // Public factory method for creating new products
        public static Product Create(
            string name,
            string sku,
            int categoryId,
            int brandId,
            decimal listPrice)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Product name is required");
                
            if (string.IsNullOrWhiteSpace(sku))
                throw new DomainException("Product SKU is required");
                
            if (listPrice <= 0)
                throw new DomainException("List price must be greater than zero");
            
            var product = new Product
            {
                Name = name,
                Sku = sku,
                CategoryId = categoryId,
                BrandId = brandId,
                ListPrice = listPrice,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            // Raise domain event
            product.AddDomainEvent(new ProductCreatedEvent(product));
            
            return product;
        }
        
        // Properties
        public string Name { get; private set; } = null!;
        public string Sku { get; private set; } = null!;
        public string? Description { get; private set; }
        public decimal ListPrice { get; private set; }
        public decimal DealerPrice { get; private set; }
        public int StockQuantity { get; private set; }
        public bool IsActive { get; private set; }
        
        // Foreign keys
        public int CategoryId { get; private set; }
        public int BrandId { get; private set; }
        
        // Navigation properties
        public Category Category { get; private set; } = null!;
        public Brand Brand { get; private set; } = null!;
        public ICollection<ProductImage> Images { get; private set; } = new List<ProductImage>();
        public ICollection<ProductPrice> TierPrices { get; private set; } = new List<ProductPrice>();
        
        // Business methods
        public void UpdatePrice(decimal listPrice, decimal dealerPrice)
        {
            if (listPrice <= 0 || dealerPrice <= 0)
                throw new DomainException("Prices must be greater than zero");
                
            if (dealerPrice > listPrice)
                throw new DomainException("Dealer price cannot exceed list price");
            
            ListPrice = listPrice;
            DealerPrice = dealerPrice;
            UpdatedAt = DateTime.UtcNow;
            
            AddDomainEvent(new ProductPriceChangedEvent(this));
        }
        
        public void UpdateStock(int quantity)
        {
            if (quantity < 0)
                throw new DomainException("Stock quantity cannot be negative");
            
            var oldQuantity = StockQuantity;
            StockQuantity = quantity;
            UpdatedAt = DateTime.UtcNow;
            
            AddDomainEvent(new StockUpdatedEvent(this, oldQuantity, quantity));
        }
        
        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }
        
        // Business rule: Check if product can be ordered
        public bool CanBeOrdered(int requestedQuantity)
        {
            return IsActive && 
                   !IsDeleted && 
                   StockQuantity >= requestedQuantity;
        }
    }
}
```

### Value Object Example

```csharp
// File: Domain/ValueObjects/Money.cs
namespace B2BCommerce.Backend.Domain.ValueObjects
{
    public class Money : ValueObject
    {
        public decimal Amount { get; private set; }
        public string Currency { get; private set; }
        
        private Money() { } // For EF Core
        
        public Money(decimal amount, string currency)
        {
            if (amount < 0)
                throw new DomainException("Amount cannot be negative");
                
            if (string.IsNullOrWhiteSpace(currency))
                throw new DomainException("Currency is required");
                
            Amount = amount;
            Currency = currency.ToUpperInvariant();
        }
        
        // Factory methods
        public static Money TRY(decimal amount) => new Money(amount, "TRY");
        public static Money USD(decimal amount) => new Money(amount, "USD");
        public static Money EUR(decimal amount) => new Money(amount, "EUR");
        
        // Operators
        public static Money operator +(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new DomainException("Cannot add money with different currencies");
                
            return new Money(a.Amount + b.Amount, a.Currency);
        }
        
        public static Money operator -(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new DomainException("Cannot subtract money with different currencies");
                
            return new Money(a.Amount - b.Amount, a.Currency);
        }
        
        public static Money operator *(Money money, decimal multiplier)
        {
            return new Money(money.Amount * multiplier, money.Currency);
        }
        
        // Value object equality
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
        
        public override string ToString() => $"{Amount:N2} {Currency}";
    }
    
    // Base class for value objects
    public abstract class ValueObject
    {
        protected abstract IEnumerable<object> GetEqualityComponents();
        
        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != GetType())
                return false;
                
            var valueObject = (ValueObject)obj;
            
            return GetEqualityComponents()
                .SequenceEqual(valueObject.GetEqualityComponents());
        }
        
        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Select(x => x?.GetHashCode() ?? 0)
                .Aggregate((x, y) => x ^ y);
        }
    }
}
```

### Domain Event Example

```csharp
// File: Domain/Events/DomainEvent.cs
namespace B2BCommerce.Backend.Domain.Events
{
    public abstract class DomainEvent
    {
        public DateTime OccurredOn { get; protected set; }
        
        protected DomainEvent()
        {
            OccurredOn = DateTime.UtcNow;
        }
    }
}

// File: Domain/Events/OrderPlacedEvent.cs
namespace B2BCommerce.Backend.Domain.Events
{
    public class OrderPlacedEvent : DomainEvent
    {
        public Order Order { get; }
        
        public OrderPlacedEvent(Order order)
        {
            Order = order;
        }
    }
}
```

### Domain Exception Example

```csharp
// File: Domain/Exceptions/DomainException.cs
namespace B2BCommerce.Backend.Domain.Exceptions
{
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message)
        {
        }
        
        public DomainException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
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
        
        public InsufficientStockException(
            int productId, 
            int requestedQuantity, 
            int availableQuantity)
            : base($"Insufficient stock for product {productId}. " +
                   $"Requested: {requestedQuantity}, Available: {availableQuantity}")
        {
            ProductId = productId;
            RequestedQuantity = requestedQuantity;
            AvailableQuantity = availableQuantity;
        }
    }
}
```

### Enumeration Example

```csharp
// File: Domain/Enums/OrderStatus.cs
namespace B2BCommerce.Backend.Domain.Enums
{
    public enum OrderStatus
    {
        Draft = 0,
        PendingApproval = 1,
        Approved = 2,
        PaymentPending = 3,
        PaymentReceived = 4,
        Processing = 5,
        Shipped = 6,
        Delivered = 7,
        Cancelled = 8,
        Rejected = 9
    }
}
```

---

## Application Layer Implementation

### Service Interface Example

```csharp
// File: Application/Interfaces/Services/IProductService.cs
namespace B2BCommerce.Backend.Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<ProductDto> GetByIdAsync(int id);
        Task<PaginatedList<ProductDto>> GetAllAsync(
            int pageNumber, 
            int pageSize, 
            string? searchTerm = null);
        Task<ProductDto> CreateAsync(CreateProductDto dto);
        Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto);
        Task DeleteAsync(int id);
        Task<decimal> GetPriceForCustomerAsync(int productId, int customerId);
    }
}
```

### Service Implementation Example

```csharp
// File: Application/Services/ProductService.cs
namespace B2BCommerce.Backend.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        
        public ProductService(
            IProductRepository productRepository,
            IMapper mapper,
            ILogger<ProductService> logger,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        
        public async Task<ProductDto> GetByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            
            if (product == null)
                throw new NotFoundException($"Product with ID {id} not found");
            
            return _mapper.Map<ProductDto>(product);
        }
        
        public async Task<PaginatedList<ProductDto>> GetAllAsync(
            int pageNumber, 
            int pageSize, 
            string? searchTerm = null)
        {
            var products = await _productRepository.GetAllAsync(
                pageNumber, 
                pageSize, 
                searchTerm);
            
            return _mapper.Map<PaginatedList<ProductDto>>(products);
        }
        
        public async Task<ProductDto> CreateAsync(CreateProductDto dto)
        {
            // Business validation
            var existingProduct = await _productRepository.GetBySkuAsync(dto.Sku);
            if (existingProduct != null)
                throw new ValidationException($"Product with SKU {dto.Sku} already exists");
            
            // Create domain entity using factory method
            var product = Product.Create(
                dto.Name,
                dto.Sku,
                dto.CategoryId,
                dto.BrandId,
                dto.ListPrice);
            
            // Set additional properties
            product.UpdatePrice(dto.ListPrice, dto.DealerPrice);
            
            // Save
            await _productRepository.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Product created: {ProductId}", product.Id);
            
            return _mapper.Map<ProductDto>(product);
        }
        
        public async Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            
            if (product == null)
                throw new NotFoundException($"Product with ID {id} not found");
            
            // Update using domain methods
            product.UpdatePrice(dto.ListPrice, dto.DealerPrice);
            
            await _productRepository.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Product updated: {ProductId}", product.Id);
            
            return _mapper.Map<ProductDto>(product);
        }
        
        public async Task DeleteAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            
            if (product == null)
                throw new NotFoundException($"Product with ID {id} not found");
            
            await _productRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Product deleted: {ProductId}", id);
        }
        
        public async Task<decimal> GetPriceForCustomerAsync(int productId, int customerId)
        {
            // Complex pricing logic
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                throw new NotFoundException($"Product with ID {productId} not found");
            
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
                throw new NotFoundException($"Customer with ID {customerId} not found");
            
            // Priority: Special Price > Tier Price > Dealer Price > List Price
            var specialPrice = await _productRepository
                .GetSpecialPriceAsync(productId, customerId);
            if (specialPrice.HasValue)
                return specialPrice.Value;
            
            if (customer.PriceTier.HasValue)
            {
                var tierPrice = await _productRepository
                    .GetTierPriceAsync(productId, customer.PriceTier.Value);
                if (tierPrice.HasValue)
                    return tierPrice.Value;
            }
            
            return customer.IsDealer ? product.DealerPrice : product.ListPrice;
        }
    }
}
```

### DTO Examples

```csharp
// File: Application/DTOs/Products/ProductDto.cs
namespace B2BCommerce.Backend.Application.DTOs.Products
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Sku { get; set; } = null!;
        public string? Description { get; set; }
        public decimal ListPrice { get; set; }
        public decimal DealerPrice { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public string CategoryName { get; set; } = null!;
        public string BrandName { get; set; } = null!;
        public List<string> ImageUrls { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}

// File: Application/DTOs/Products/CreateProductDto.cs
namespace B2BCommerce.Backend.Application.DTOs.Products
{
    public class CreateProductDto
    {
        public string Name { get; set; } = null!;
        public string Sku { get; set; } = null!;
        public string? Description { get; set; }
        public decimal ListPrice { get; set; }
        public decimal DealerPrice { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
    }
}

// File: Application/DTOs/Products/UpdateProductDto.cs
namespace B2BCommerce.Backend.Application.DTOs.Products
{
    public class UpdateProductDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? ListPrice { get; set; }
        public decimal? DealerPrice { get; set; }
    }
}
```

### AutoMapper Profile Example

```csharp
// File: Application/Mapping/MappingProfile.cs
namespace B2BCommerce.Backend.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Product mappings
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, 
                    opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.BrandName, 
                    opt => opt.MapFrom(src => src.Brand.Name))
                .ForMember(dest => dest.ImageUrls, 
                    opt => opt.MapFrom(src => src.Images.Select(i => i.Url).ToList()));
            
            CreateMap<CreateProductDto, Product>();
            CreateMap<UpdateProductDto, Product>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            
            // Order mappings
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.CustomerName, 
                    opt => opt.MapFrom(src => src.Customer.CompanyName))
                .ForMember(dest => dest.Items, 
                    opt => opt.MapFrom(src => src.OrderItems));
            
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ProductName, 
                    opt => opt.MapFrom(src => src.Product.Name));
            
            // ... other mappings
        }
    }
}
```

### FluentValidation Example

```csharp
// File: Application/Validators/CreateProductValidator.cs
namespace B2BCommerce.Backend.Application.Validators
{
    public class CreateProductValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");
            
            RuleFor(x => x.Sku)
                .NotEmpty().WithMessage("SKU is required")
                .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters")
                .Matches("^[A-Z0-9-]+$").WithMessage("SKU can only contain uppercase letters, numbers, and hyphens");
            
            RuleFor(x => x.ListPrice)
                .GreaterThan(0).WithMessage("List price must be greater than zero");
            
            RuleFor(x => x.DealerPrice)
                .GreaterThan(0).WithMessage("Dealer price must be greater than zero")
                .LessThanOrEqualTo(x => x.ListPrice)
                    .WithMessage("Dealer price cannot exceed list price");
            
            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Category must be selected");
            
            RuleFor(x => x.BrandId)
                .GreaterThan(0).WithMessage("Brand must be selected");
        }
    }
}
```

### Application Exception Examples

```csharp
// File: Application/Exceptions/ApplicationException.cs
namespace B2BCommerce.Backend.Application.Exceptions
{
    public class ApplicationException : Exception
    {
        public ApplicationException(string message) : base(message)
        {
        }
    }
}

// File: Application/Exceptions/NotFoundException.cs
namespace B2BCommerce.Backend.Application.Exceptions
{
    public class NotFoundException : ApplicationException
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }
}

// File: Application/Exceptions/ValidationException.cs
namespace B2BCommerce.Backend.Application.Exceptions
{
    public class ValidationException : ApplicationException
    {
        public IDictionary<string, string[]> Errors { get; }
        
        public ValidationException(string message) : base(message)
        {
            Errors = new Dictionary<string, string[]>();
        }
        
        public ValidationException(IDictionary<string, string[]> errors) 
            : base("One or more validation errors occurred")
        {
            Errors = errors;
        }
    }
}
```

---

## Infrastructure Layer Implementation

### DbContext Example

```csharp
// File: Infrastructure/Data/ApplicationDbContext.cs
namespace B2BCommerce.Backend.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        // DbSets
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Brand> Brands => Set<Brand>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        // ... other entities
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Apply all configurations from assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            
            // Global query filters
            modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Customer>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Order>().HasQueryFilter(o => !o.IsDeleted);
            // ... other soft-delete filters
        }
        
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Set audit fields
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity && 
                           (e.State == EntityState.Added || e.State == EntityState.Modified));
            
            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;
                
                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                    // Set CreatedBy from current user (implement later)
                }
                else if (entry.State == EntityState.Modified)
                {
                    entity.UpdatedAt = DateTime.UtcNow;
                    // Set UpdatedBy from current user (implement later)
                }
            }
            
            // Dispatch domain events (if using MediatR)
            // await DispatchDomainEventsAsync();
            
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
```

### Entity Configuration Example

```csharp
// File: Infrastructure/Data/Configurations/ProductConfiguration.cs
namespace B2BCommerce.Backend.Infrastructure.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            // Table name
            builder.ToTable("Products");
            
            // Primary key
            builder.HasKey(p => p.Id);
            
            // Properties
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);
            
            builder.Property(p => p.Sku)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.HasIndex(p => p.Sku)
                .IsUnique();
            
            builder.Property(p => p.Description)
                .HasMaxLength(2000);
            
            builder.Property(p => p.ListPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            
            builder.Property(p => p.DealerPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            
            builder.Property(p => p.StockQuantity)
                .IsRequired();
            
            builder.Property(p => p.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            
            // Relationships
            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasMany(p => p.Images)
                .WithOne(i => i.Product)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasMany(p => p.TierPrices)
                .WithOne(tp => tp.Product)
                .HasForeignKey(tp => tp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            builder.HasIndex(p => p.CategoryId);
            builder.HasIndex(p => p.BrandId);
            builder.HasIndex(p => p.IsActive);
            builder.HasIndex(p => p.CreatedAt);
            
            // Soft delete
            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }
}
```

### Generic Repository Implementation

```csharp
// File: Infrastructure/Repositories/GenericRepository.cs
namespace B2BCommerce.Backend.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;
        
        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }
        
        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }
        
        public async Task<List<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }
        
        public async Task<PaginatedList<T>> GetAllAsync(
            int pageNumber, 
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string? includeProperties = null)
        {
            IQueryable<T> query = _dbSet;
            
            // Apply filter
            if (filter != null)
                query = query.Where(filter);
            
            // Include properties
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }
            
            // Apply ordering
            if (orderBy != null)
                query = orderBy(query);
            
            // Get total count
            var count = await query.CountAsync();
            
            // Apply pagination
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return new PaginatedList<T>(items, count, pageNumber, pageSize);
        }
        
        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }
        
        public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            return entities;
        }
        
        public Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }
        
        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                // Soft delete
                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.UtcNow;
                // Set DeletedBy from current user (implement later)
                _dbSet.Update(entity);
            }
        }
        
        public Task DeleteAsync(T entity)
        {
            // Soft delete
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }
        
        public async Task<bool> ExistsAsync(int id)
        {
            return await _dbSet.AnyAsync(e => e.Id == id);
        }
        
        public async Task<int> CountAsync(Expression<Func<T, bool>>? filter = null)
        {
            if (filter == null)
                return await _dbSet.CountAsync();
            
            return await _dbSet.CountAsync(filter);
        }
    }
}
```

### Specific Repository Implementation

```csharp
// File: Infrastructure/Repositories/ProductRepository.cs
namespace B2BCommerce.Backend.Infrastructure.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }
        
        public async Task<Product?> GetBySkuAsync(string sku)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.Sku == sku);
        }
        
        public async Task<List<Product>> GetByCategoryAsync(int categoryId)
        {
            return await _dbSet
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .ToListAsync();
        }
        
        public async Task<List<Product>> SearchAsync(string searchTerm)
        {
            return await _dbSet
                .Where(p => p.IsActive && 
                           (p.Name.Contains(searchTerm) || 
                            p.Sku.Contains(searchTerm) ||
                            p.Description!.Contains(searchTerm)))
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .ToListAsync();
        }
        
        public async Task<decimal?> GetSpecialPriceAsync(int productId, int customerId)
        {
            return await _context.ProductSpecialPrices
                .Where(sp => sp.ProductId == productId && 
                            sp.CustomerId == customerId &&
                            sp.IsActive)
                .Select(sp => sp.Price)
                .FirstOrDefaultAsync();
        }
        
        public async Task<decimal?> GetTierPriceAsync(int productId, PriceTier tier)
        {
            return await _context.ProductTierPrices
                .Where(tp => tp.ProductId == productId && 
                            tp.Tier == tier)
                .Select(tp => tp.Price)
                .FirstOrDefaultAsync();
        }
        
        public async Task UpdateStockAsync(int productId, int quantity)
        {
            var product = await GetByIdAsync(productId);
            if (product != null)
            {
                product.UpdateStock(quantity);
            }
        }
    }
}
```

### Unit of Work Implementation

```csharp
// File: Infrastructure/Data/UnitOfWork.cs
namespace B2BCommerce.Backend.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        
        // Repository instances (lazy initialization)
        private IProductRepository? _productRepository;
        private IOrderRepository? _orderRepository;
        private ICustomerRepository? _customerRepository;
        // ... other repositories
        
        public UnitOfWork(
            ApplicationDbContext context,
            ILogger<UnitOfWork> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        // Repository properties with lazy initialization
        public IProductRepository Products => 
            _productRepository ??= new ProductRepository(_context);
        
        public IOrderRepository Orders => 
            _orderRepository ??= new OrderRepository(_context);
        
        public ICustomerRepository Customers => 
            _customerRepository ??= new CustomerRepository(_context);
        
        // ... other repository properties
        
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error saving changes to database");
                throw;
            }
        }
        
        public async Task<bool> SaveChangesWithResultAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken) > 0;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error saving changes to database");
                return false;
            }
        }
        
        public async Task BeginTransactionAsync()
        {
            await _context.Database.BeginTransactionAsync();
        }
        
        public async Task CommitTransactionAsync()
        {
            await _context.Database.CommitTransactionAsync();
        }
        
        public async Task RollbackTransactionAsync()
        {
            await _context.Database.RollbackTransactionAsync();
        }
        
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
```

### External Service Client Example

```csharp
// File: Infrastructure/ExternalApis/PaymentGatewayClient.cs
namespace B2BCommerce.Backend.Infrastructure.ExternalApis
{
    public class PaymentGatewayClient : IPaymentGatewayClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PaymentGatewayClient> _logger;
        private readonly PaymentGatewaySettings _settings;
        
        public PaymentGatewayClient(
            HttpClient httpClient,
            IOptions<PaymentGatewaySettings> settings,
            ILogger<PaymentGatewayClient> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
            
            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _settings.ApiKey);
        }
        
        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/payments", request);
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<PaymentResult>();
                
                _logger.LogInformation("Payment processed successfully: {TransactionId}", 
                    result?.TransactionId);
                
                return result!;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error processing payment");
                throw new PaymentException("Failed to process payment", ex);
            }
        }
        
        public async Task<PaymentStatus> GetPaymentStatusAsync(string transactionId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/payments/{transactionId}");
                response.EnsureSuccessStatusCode();
                
                var status = await response.Content.ReadFromJsonAsync<PaymentStatus>();
                return status!;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error getting payment status for {TransactionId}", transactionId);
                throw new PaymentException("Failed to get payment status", ex);
            }
        }
    }
}
```

---

## Presentation/API Layer Implementation

### Controller Example

```csharp
// File: API/Controllers/ProductsController.cs
namespace B2BCommerce.Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;
        
        public ProductsController(
            IProductService productService,
            ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }
        
        /// <summary>
        /// Get all products with pagination
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<ProductDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedList<ProductDto>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null)
        {
            var products = await _productService.GetAllAsync(pageNumber, pageSize, search);
            return Ok(products);
        }
        
        /// <summary>
        /// Get product by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> GetById(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                return Ok(product);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        
        /// <summary>
        /// Create new product
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto dto)
        {
            try
            {
                var product = await _productService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message, errors = ex.Errors });
            }
        }
        
        /// <summary>
        /// Update existing product
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductDto>> Update(
            int id, 
            [FromBody] UpdateProductDto dto)
        {
            try
            {
                var product = await _productService.UpdateAsync(id, dto);
                return Ok(product);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message, errors = ex.Errors });
            }
        }
        
        /// <summary>
        /// Delete product
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _productService.DeleteAsync(id);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        
        /// <summary>
        /// Get product price for current customer
        /// </summary>
        [HttpGet("{id}/price")]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        public async Task<ActionResult<decimal>> GetPrice(int id)
        {
            // Get customer ID from JWT claims
            var customerIdClaim = User.FindFirst("customer_id");
            if (customerIdClaim == null)
                return Unauthorized();
            
            var customerId = int.Parse(customerIdClaim.Value);
            var price = await _productService.GetPriceForCustomerAsync(id, customerId);
            
            return Ok(price);
        }
    }
}
```

### Middleware Example (Error Handling)

```csharp
// File: API/Middleware/ErrorHandlingMiddleware.cs
namespace B2BCommerce.Backend.Api.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        
        public ErrorHandlingMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }
        
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred");
            
            context.Response.ContentType = "application/json";
            
            var response = exception switch
            {
                NotFoundException notFoundEx => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = notFoundEx.Message
                },
                ValidationException validationEx => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = validationEx.Message,
                    Errors = validationEx.Errors
                },
                DomainException domainEx => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = domainEx.Message
                },
                UnauthorizedAccessException => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized access"
                },
                _ => new ErrorResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An internal server error occurred"
                }
            };
            
            context.Response.StatusCode = response.StatusCode;
            await context.Response.WriteAsJsonAsync(response);
        }
    }
    
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = null!;
        public IDictionary<string, string[]>? Errors { get; set; }
    }
}
```

### Program.cs (Dependency Injection Configuration)

```csharp
// File: API/Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();

// Application Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
// ... other services

// Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
// ... other repositories

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Infrastructure Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISmsService, SmsService>();
// ... other infrastructure services

// External API Clients
builder.Services.AddHttpClient<IPaymentGatewayClient, PaymentGatewayClient>();
builder.Services.AddHttpClient<IShippingApiClient, ShippingApiClient>();

// Caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// Authorization
builder.Services.AddAuthorization();

// Logging
builder.Services.AddSerilog((services, lc) => lc
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()!)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

---

## Cross-Cutting Concerns

### Pagination Helper

```csharp
// File: Application/Common/PaginatedList.cs
namespace B2BCommerce.Backend.Application.Common
{
    public class PaginatedList<T>
    {
        public List<T> Items { get; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public int TotalPages { get; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
        
        public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = count;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        }
        
        public static async Task<PaginatedList<T>> CreateAsync(
            IQueryable<T> source, 
            int pageNumber, 
            int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return new PaginatedList<T>(items, count, pageNumber, pageSize);
        }
    }
}
```

### Result Pattern

```csharp
// File: Application/Common/Result.cs
namespace B2BCommerce.Backend.Application.Common
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string? Error { get; }
        
        protected Result(bool isSuccess, string? error)
        {
            if (isSuccess && error != null)
                throw new InvalidOperationException("Success result cannot have an error");
            if (!isSuccess && error == null)
                throw new InvalidOperationException("Failure result must have an error");
            
            IsSuccess = isSuccess;
            Error = error;
        }
        
        public static Result Success() => new Result(true, null);
        public static Result Failure(string error) => new Result(false, error);
        
        public static Result<T> Success<T>(T value) => new Result<T>(value, true, null);
        public static Result<T> Failure<T>(string error) => new Result<T>(default, false, error);
    }
    
    public class Result<T> : Result
    {
        public T? Value { get; }
        
        protected internal Result(T? value, bool isSuccess, string? error)
            : base(isSuccess, error)
        {
            Value = value;
        }
    }
}
```

---

## Coding Standards & Conventions

### Naming Conventions

```csharp
// ✅ CORRECT: PascalCase for class names
public class ProductService { }

// ✅ CORRECT: PascalCase for public members
public string Name { get; set; }

// ✅ CORRECT: camelCase for private fields
private readonly IProductRepository _productRepository;

// ✅ CORRECT: camelCase for method parameters
public async Task<Product> GetByIdAsync(int id) { }

// ✅ CORRECT: PascalCase for methods
public async Task UpdatePriceAsync(decimal newPrice) { }

// ✅ CORRECT: Async suffix for async methods
public async Task<List<Product>> GetAllAsync() { }

// ✅ CORRECT: Interface prefix 'I'
public interface IProductService { }

// ✅ CORRECT: Descriptive names
public class OrderPlacedEvent { }  // ✅ Good
public class OrderEvent { }        // ❌ Too generic

// ✅ CORRECT: Plural for collections
public List<Product> Products { get; set; }
```

### Code Organization

```csharp
// ✅ CORRECT: Order of members
public class ProductService
{
    // 1. Private fields
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;
    
    // 2. Constructor
    public ProductService(
        IProductRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }
    
    // 3. Public properties (if any)
    public bool IsEnabled { get; set; }
    
    // 4. Public methods
    public async Task<ProductDto> GetByIdAsync(int id)
    {
        // Implementation
    }
    
    // 5. Private methods
    private bool ValidateProduct(Product product)
    {
        // Implementation
    }
}
```

### SOLID Principles

#### Single Responsibility Principle

```csharp
// ❌ BAD: Class doing too much
public class OrderService
{
    public async Task CreateOrder() { }
    public async Task SendEmail() { }
    public async Task ProcessPayment() { }
    public async Task UpdateInventory() { }
}

// ✅ GOOD: Separate responsibilities
public class OrderService
{
    private readonly IEmailService _emailService;
    private readonly IPaymentService _paymentService;
    private readonly IInventoryService _inventoryService;
    
    public async Task CreateOrder()
    {
        // Create order
        await _emailService.SendOrderConfirmation();
        await _paymentService.ProcessPayment();
        await _inventoryService.ReserveStock();
    }
}
```

#### Dependency Inversion Principle

```csharp
// ❌ BAD: Depending on concrete implementation
public class OrderService
{
    private readonly ProductRepository _repository; // Concrete class
}

// ✅ GOOD: Depending on abstraction
public class OrderService
{
    private readonly IProductRepository _repository; // Interface
}
```

### Async/Await Best Practices

```csharp
// ✅ CORRECT: Use async/await for I/O operations
public async Task<Product> GetProductAsync(int id)
{
    return await _repository.GetByIdAsync(id);
}

// ✅ CORRECT: ConfigureAwait(false) in libraries (not in ASP.NET Core)
// Note: In ASP.NET Core, ConfigureAwait(false) is not needed

// ❌ BAD: Blocking async call
public Product GetProduct(int id)
{
    return _repository.GetByIdAsync(id).Result; // Deadlock risk!
}

// ✅ CORRECT: Async all the way
public async Task<Product> GetProduct(int id)
{
    return await _repository.GetByIdAsync(id);
}

// ✅ CORRECT: Parallel async operations
public async Task<(Product Product, Customer Customer)> GetDataAsync(int productId, int customerId)
{
    var productTask = _productRepository.GetByIdAsync(productId);
    var customerTask = _customerRepository.GetByIdAsync(customerId);
    
    await Task.WhenAll(productTask, customerTask);
    
    return (productTask.Result, customerTask.Result);
}
```

### Exception Handling

```csharp
// ✅ CORRECT: Catch specific exceptions
try
{
    await _repository.SaveAsync(product);
}
catch (DbUpdateException ex)
{
    _logger.LogError(ex, "Failed to save product");
    throw new ApplicationException("Failed to save product", ex);
}

// ❌ BAD: Catching general exception
catch (Exception ex)
{
    // Too broad
}

// ✅ CORRECT: Let exceptions bubble up (in most cases)
public async Task<Product> GetProductAsync(int id)
{
    var product = await _repository.GetByIdAsync(id);
    
    if (product == null)
        throw new NotFoundException($"Product {id} not found");
    
    return product;
}
```

### Null Handling

```csharp
// ✅ CORRECT: Nullable reference types
public class ProductDto
{
    public string Name { get; set; } = null!;      // Required
    public string? Description { get; set; }        // Optional
}

// ✅ CORRECT: Null checking
if (product == null)
    throw new NotFoundException();

// ✅ CORRECT: Null-coalescing
var name = product.Name ?? "Unknown";

// ✅ CORRECT: Null-conditional
var categoryName = product?.Category?.Name;
```

---

## Design Patterns & Best Practices

### Repository Pattern

Already covered in Infrastructure section.

### Unit of Work Pattern

Already covered in Infrastructure section.

### Factory Pattern

```csharp
// File: Domain/Factories/OrderFactory.cs
public static class OrderFactory
{
    public static Order CreateDirectOrder(
        Customer customer,
        List<OrderItem> items,
        Address shippingAddress)
    {
        var order = new Order
        {
            CustomerId = customer.Id,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Approved, // Direct order
            ShippingAddress = shippingAddress
        };
        
        foreach (var item in items)
        {
            order.AddItem(item);
        }
        
        order.CalculateTotal();
        
        return order;
    }
    
    public static Order CreatePendingApprovalOrder(
        Customer customer,
        List<OrderItem> items,
        Address shippingAddress)
    {
        var order = new Order
        {
            CustomerId = customer.Id,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.PendingApproval, // Needs approval
            ShippingAddress = shippingAddress
        };
        
        foreach (var item in items)
        {
            order.AddItem(item);
        }
        
        order.CalculateTotal();
        
        return order;
    }
}
```

### Strategy Pattern (for Pricing)

```csharp
// File: Application/Services/Pricing/IPricingStrategy.cs
public interface IPricingStrategy
{
    decimal CalculatePrice(Product product, Customer customer);
}

// File: Application/Services/Pricing/ListPricingStrategy.cs
public class ListPricingStrategy : IPricingStrategy
{
    public decimal CalculatePrice(Product product, Customer customer)
    {
        return product.ListPrice;
    }
}

// File: Application/Services/Pricing/DealerPricingStrategy.cs
public class DealerPricingStrategy : IPricingStrategy
{
    public decimal CalculatePrice(Product product, Customer customer)
    {
        return product.DealerPrice;
    }
}

// File: Application/Services/Pricing/TierPricingStrategy.cs
public class TierPricingStrategy : IPricingStrategy
{
    private readonly IProductRepository _productRepository;
    
    public async Task<decimal> CalculatePriceAsync(Product product, Customer customer)
    {
        var tierPrice = await _productRepository
            .GetTierPriceAsync(product.Id, customer.PriceTier);
        
        return tierPrice ?? product.DealerPrice;
    }
}

// Usage in service
public class PricingService
{
    public decimal GetPrice(Product product, Customer customer)
    {
        IPricingStrategy strategy = customer.PriceTier switch
        {
            PriceTier.A => new TierPricingStrategy(),
            PriceTier.B => new TierPricingStrategy(),
            PriceTier.C => new TierPricingStrategy(),
            _ => customer.IsDealer 
                ? new DealerPricingStrategy() 
                : new ListPricingStrategy()
        };
        
        return strategy.CalculatePrice(product, customer);
    }
}
```

### Specification Pattern (for Queries)

```csharp
// File: Application/Specifications/ISpecification.cs
public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }
}

// File: Application/Specifications/BaseSpecification.cs
public abstract class BaseSpecification<T> : ISpecification<T>
{
    public Expression<Func<T, bool>> Criteria { get; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    
    protected BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }
    
    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }
    
    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }
    
    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
    {
        OrderByDescending = orderByDescExpression;
    }
}

// File: Application/Specifications/ActiveProductsSpecification.cs
public class ActiveProductsSpecification : BaseSpecification<Product>
{
    public ActiveProductsSpecification() 
        : base(p => p.IsActive && !p.IsDeleted)
    {
        AddInclude(p => p.Category);
        AddInclude(p => p.Brand);
        AddInclude(p => p.Images);
        ApplyOrderBy(p => p.Name);
    }
}

// Usage in repository
public async Task<List<Product>> GetProductsBySpecAsync(ISpecification<Product> spec)
{
    var query = ApplySpecification(spec);
    return await query.ToListAsync();
}

private IQueryable<Product> ApplySpecification(ISpecification<Product> spec)
{
    var query = _dbSet.Where(spec.Criteria);
    
    query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));
    
    if (spec.OrderBy != null)
        query = query.OrderBy(spec.OrderBy);
    else if (spec.OrderByDescending != null)
        query = query.OrderByDescending(spec.OrderByDescending);
    
    return query;
}
```

---

## Testing Strategy

### Unit Testing Example

```csharp
// File: Tests/Application.Tests/Services/ProductServiceTests.cs
public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ProductService>> _mockLogger;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly ProductService _service;
    
    public ProductServiceTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ProductService>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        
        _service = new ProductService(
            _mockRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockUnitOfWork.Object);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsProduct()
    {
        // Arrange
        var productId = 1;
        var product = new Product { Id = productId, Name = "Test Product" };
        var productDto = new ProductDto { Id = productId, Name = "Test Product" };
        
        _mockRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(product);
        _mockMapper.Setup(m => m.Map<ProductDto>(product))
            .Returns(productDto);
        
        // Act
        var result = await _service.GetByIdAsync(productId);
        
        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(productId);
        result.Name.Should().Be("Test Product");
    }
    
    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var productId = 999;
        _mockRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);
        
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _service.GetByIdAsync(productId));
    }
    
    [Fact]
    public async Task CreateAsync_WithValidData_CreatesProduct()
    {
        // Arrange
        var dto = new CreateProductDto 
        { 
            Name = "New Product",
            Sku = "SKU123",
            ListPrice = 100,
            DealerPrice = 80
        };
        
        var product = new Product { Id = 1, Name = dto.Name };
        var productDto = new ProductDto { Id = 1, Name = dto.Name };
        
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync(product);
        _mockMapper.Setup(m => m.Map<ProductDto>(It.IsAny<Product>()))
            .Returns(productDto);
        
        // Act
        var result = await _service.CreateAsync(dto);
        
        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(dto.Name);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
```

### Integration Testing Example

```csharp
// File: Tests/Integration.Tests/Repositories/ProductRepositoryTests.cs
public class ProductRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly ProductRepository _repository;
    
    public ProductRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new ProductRepository(_fixture.Context);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithExistingProduct_ReturnsProduct()
    {
        // Arrange
        var product = new Product 
        { 
            Name = "Test Product",
            Sku = "TEST123",
            ListPrice = 100,
            DealerPrice = 80
        };
        
        _fixture.Context.Products.Add(product);
        await _fixture.Context.SaveChangesAsync();
        
        // Act
        var result = await _repository.GetByIdAsync(product.Id);
        
        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Name.Should().Be(product.Name);
    }
}

// Database Fixture
public class DatabaseFixture : IDisposable
{
    public ApplicationDbContext Context { get; }
    
    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        Context = new ApplicationDbContext(options);
        Context.Database.EnsureCreated();
    }
    
    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}
```

---

## Common Pitfalls & Solutions

### Pitfall 1: Circular Dependencies

```csharp
// ❌ BAD: Circular dependency
public class OrderService
{
    private readonly ICustomerService _customerService;
}

public class CustomerService
{
    private readonly IOrderService _orderService; // Circular!
}

// ✅ GOOD: Extract shared logic or use events
public class OrderService
{
    private readonly ICustomerRepository _customerRepository; // Direct repository
}

public class CustomerService
{
    private readonly IOrderRepository _orderRepository; // Direct repository
}
```

### Pitfall 2: Leaking Domain Entities to API

```csharp
// ❌ BAD: Returning domain entity from controller
[HttpGet]
public async Task<Product> GetProduct(int id)
{
    return await _repository.GetByIdAsync(id);
}

// ✅ GOOD: Return DTO
[HttpGet]
public async Task<ProductDto> GetProduct(int id)
{
    return await _productService.GetByIdAsync(id);
}
```

### Pitfall 3: Not Using Async Properly

```csharp
// ❌ BAD: Blocking async call
public Product GetProduct(int id)
{
    return _repository.GetByIdAsync(id).Result; // Deadlock risk!
}

// ✅ GOOD: Async all the way
public async Task<Product> GetProduct(int id)
{
    return await _repository.GetByIdAsync(id);
}
```

### Pitfall 4: Putting Business Logic in Controllers

```csharp
// ❌ BAD: Business logic in controller
[HttpPost]
public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
{
    var order = new Order();
    order.CustomerId = dto.CustomerId;
    // ... lots of business logic here
    
    await _repository.AddAsync(order);
    return Ok();
}

// ✅ GOOD: Business logic in service
[HttpPost]
public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
{
    var order = await _orderService.CreateAsync(dto);
    return Ok(order);
}
```

### Pitfall 5: Not Validating Input

```csharp
// ❌ BAD: No validation
public async Task<Product> CreateAsync(CreateProductDto dto)
{
    var product = new Product();
    // ... create product without validation
}

// ✅ GOOD: Use FluentValidation
public async Task<Product> CreateAsync(CreateProductDto dto)
{
    var validator = new CreateProductValidator();
    var validationResult = await validator.ValidateAsync(dto);
    
    if (!validationResult.IsValid)
        throw new ValidationException(validationResult.Errors);
    
    // ... create product
}
```

---

## Conclusion

This Clean Architecture implementation guide provides:

- ✅ Clear layer separation and responsibilities
- ✅ Dependency rules enforcement
- ✅ Consistent coding standards
- ✅ Design patterns and best practices
- ✅ Testing strategies
- ✅ Common pitfalls to avoid

**Key Takeaways**:

1. **Dependencies flow inward** toward the domain
2. **Domain is pure** - no external dependencies
3. **Use interfaces** for dependency inversion
4. **DTOs for data transfer** - don't expose entities
5. **Validate early** - use FluentValidation
6. **Async all the way** - don't block async calls
7. **Test at all levels** - unit, integration, E2E

This architecture ensures:

- **Maintainability**: Easy to understand and modify
- **Testability**: Each layer can be tested independently
- **Scalability**: Can grow with business needs
- **Flexibility**: Easy to swap implementations

---

**Document Version**: 1.0\
**Last Updated**: November 2025\
**For**: All .NET backend projects in B2B E-Commerce solution\
**Maintained By**: Backend Development Team
