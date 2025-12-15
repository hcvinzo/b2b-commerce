# Integration API - External Entity Pattern Specification (v2)

## Document Purpose

Implementation specification for handling external system identities in the B2B E-Commerce Platform, specifically for LOGO ERP integration.

**Target**: .NET 8, EF Core 8, Clean Architecture, MediatR, CQRS  
**Version**: 2.0  
**Date**: December 2025

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              API Layer                                       │
├──────────────────────────────┬──────────────────────────────────────────────┤
│     B2B API (JWT Auth)       │        Integration API (API Key Auth)        │
│                              │                                              │
│  ProductController           │  IntegrationProductController               │
│  - Create/Update/Delete      │  - Sync (upsert by external code)           │
│  - Uses internal IDs         │  - Maps external codes → internal IDs       │
└──────────────┬───────────────┴────────────────────┬─────────────────────────┘
               │                                    │
               │         DTO Mapping                │
               ▼                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                         Application Layer                                    │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                    MediatR Commands (Single Source)                  │   │
│  │                                                                      │   │
│  │  UpsertProductCommand      ← Used by BOTH APIs                      │   │
│  │  UpdateProductPriceCommand ← Used by BOTH APIs                      │   │
│  │  UpdateProductStockCommand ← Used by BOTH APIs                      │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  Command Handlers contain ALL business logic (no duplication)              │
└──────────────────────────────────┬──────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                         Domain Layer                                         │
│                                                                             │
│  Entities with factory methods and business rules                           │
│  Product.CreateFromExternal(), Product.UpdatePricing(), etc.               │
└──────────────────────────────────┬──────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                      Infrastructure Layer                                    │
│                                                                             │
│  Repositories with GetByExternalCodeAsync()                                 │
│  DbContext, Configurations                                                  │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Key Principles

1. **Single Command Handlers** - Business logic lives in one place
2. **Thin API Controllers** - Just map DTOs and dispatch commands
3. **Repository Abstraction** - Both `GetByIdAsync` and `GetByExternalCodeAsync`
4. **Domain Logic in Entities** - Factory methods, validation, state changes

---

## Domain Layer

### IExternalEntity Interface

```csharp
// File: Domain/Common/IExternalEntity.cs
namespace B2BCommerce.Backend.Domain.Common
{
    /// <summary>
    /// Marker interface for entities that originate from external systems (LOGO ERP)
    /// </summary>
    public interface IExternalEntity
    {
        /// <summary>
        /// External system's unique code (e.g., LOGO product code)
        /// Used for matching during sync operations
        /// </summary>
        string ExternalCode { get; }
        
        /// <summary>
        /// External system's internal ID (optional reference)
        /// </summary>
        string? ExternalId { get; }
        
        /// <summary>
        /// Last successful sync timestamp
        /// </summary>
        DateTime? LastSyncedAt { get; }
        
        /// <summary>
        /// Mark entity as synced
        /// </summary>
        void MarkAsSynced(DateTime? syncTime = null);
    }
}
```

### BaseEntity (Updated)

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
    }
    
    /// <summary>
    /// Base for entities managed by external systems
    /// </summary>
    public abstract class ExternalEntity : BaseEntity, IExternalEntity
    {
        public string ExternalCode { get; protected set; } = null!;
        public string? ExternalId { get; protected set; }
        public DateTime? LastSyncedAt { get; protected set; }
        
        public void MarkAsSynced(DateTime? syncTime = null)
        {
            LastSyncedAt = syncTime ?? DateTime.UtcNow;
        }
        
        public void SetExternalId(string externalId)
        {
            ExternalId = externalId;
        }
    }
}
```

### Product Entity

```csharp
// File: Domain/Entities/Product.cs
using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Events;
using B2BCommerce.Backend.Domain.Exceptions;

namespace B2BCommerce.Backend.Domain.Entities
{
    public class Product : ExternalEntity, IAggregateRoot
    {
        private Product() { }
        
        #region Factory Methods
        
        /// <summary>
        /// Create product from external system (LOGO)
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
            ValidateCore(externalCode, name, sku, listPrice);
            
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
            ValidateCore(sku, name, sku, listPrice); // Use SKU as external code for internal creates
            
            var product = new Product
            {
                ExternalCode = sku, // Default external code to SKU for internal creates
                Name = name,
                Sku = sku,
                CategoryId = categoryId,
                BrandId = brandId,
                ListPrice = listPrice,
                DealerPrice = dealerPrice,
                Currency = currency,
                IsActive = true
            };
            
            product.AddDomainEvent(new ProductCreatedEvent(product.Id, product.Sku));
            return product;
        }
        
        private static void ValidateCore(string externalCode, string name, string sku, decimal listPrice)
        {
            if (string.IsNullOrWhiteSpace(externalCode))
                throw new DomainException("External code is required");
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Product name is required");
            if (string.IsNullOrWhiteSpace(sku))
                throw new DomainException("SKU is required");
            if (listPrice < 0)
                throw new DomainException("List price cannot be negative");
        }
        
        #endregion
        
        #region Properties
        
        public string Name { get; private set; } = null!;
        public string Sku { get; private set; } = null!;
        public string? Description { get; private set; }
        public string? ShortDescription { get; private set; }
        
        // Pricing
        public decimal ListPrice { get; private set; }
        public decimal DealerPrice { get; private set; }
        public decimal? CostPrice { get; private set; }
        public string Currency { get; private set; } = "TRY";
        
        // Stock
        public int StockQuantity { get; private set; }
        public int MinimumOrderQuantity { get; private set; } = 1;
        public bool TrackStock { get; private set; } = true;
        
        // Status
        public bool IsActive { get; private set; }
        public bool IsFeatured { get; private set; }
        
        // Serial/Warranty
        public bool RequiresSerialNumber { get; private set; }
        public int? WarrantyMonths { get; private set; }
        
        // Physical
        public decimal? Weight { get; private set; }
        public string? WeightUnit { get; private set; }
        
        // Tax
        public int? TaxRate { get; private set; }
        
        // Foreign Keys
        public int CategoryId { get; private set; }
        public int BrandId { get; private set; }
        public int? ProductTypeId { get; private set; }
        
        // Navigation
        public Category Category { get; private set; } = null!;
        public Brand Brand { get; private set; } = null!;
        public ProductType? ProductType { get; private set; }
        public ICollection<ProductImage> Images { get; private set; } = new List<ProductImage>();
        public ICollection<ProductAttributeValue> AttributeValues { get; private set; } = new List<ProductAttributeValue>();
        public ICollection<ProductSpecialPrice> SpecialPrices { get; private set; } = new List<ProductSpecialPrice>();
        
        #endregion
        
        #region Behavior Methods
        
        public void Update(
            string name,
            string? description,
            string? shortDescription,
            int categoryId,
            int brandId,
            bool isActive)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Product name is required");
            
            Name = name;
            Description = description;
            ShortDescription = shortDescription;
            CategoryId = categoryId;
            BrandId = brandId;
            IsActive = isActive;
            
            AddDomainEvent(new ProductUpdatedEvent(Id, Sku));
        }
        
        public void UpdatePricing(decimal listPrice, decimal dealerPrice, decimal? costPrice = null)
        {
            if (listPrice < 0)
                throw new DomainException("List price cannot be negative");
            if (dealerPrice < 0)
                throw new DomainException("Dealer price cannot be negative");
            
            ListPrice = listPrice;
            DealerPrice = dealerPrice;
            CostPrice = costPrice;
            
            AddDomainEvent(new ProductPriceUpdatedEvent(Id, Sku, listPrice, dealerPrice));
        }
        
        public void UpdateStock(int quantity)
        {
            if (quantity < 0)
                throw new DomainException("Stock quantity cannot be negative");
            
            var oldQuantity = StockQuantity;
            StockQuantity = quantity;
            
            AddDomainEvent(new ProductStockUpdatedEvent(Id, Sku, oldQuantity, quantity));
        }
        
        public void SetProductType(int? productTypeId)
        {
            ProductTypeId = productTypeId;
        }
        
        public void SetWarranty(int? months)
        {
            WarrantyMonths = months;
        }
        
        public void SetSerialTracking(bool required)
        {
            RequiresSerialNumber = required;
        }
        
        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
        public void SetFeatured(bool featured) => IsFeatured = featured;
        
        #endregion
    }
}
```

### Category Entity

```csharp
// File: Domain/Entities/Category.cs
using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Exceptions;

namespace B2BCommerce.Backend.Domain.Entities
{
    public class Category : ExternalEntity, IAggregateRoot
    {
        private Category() { }
        
        public static Category CreateFromExternal(
            string externalCode,
            string name,
            int? parentId = null,
            string? externalId = null)
        {
            if (string.IsNullOrWhiteSpace(externalCode))
                throw new DomainException("External code is required");
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Category name is required");
            
            return new Category
            {
                ExternalCode = externalCode,
                ExternalId = externalId,
                Name = name,
                Slug = GenerateSlug(name),
                ParentId = parentId,
                IsActive = true,
                SortOrder = 0,
                LastSyncedAt = DateTime.UtcNow
            };
        }
        
        public static Category Create(string name, int? parentId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Category name is required");
            
            var slug = GenerateSlug(name);
            
            return new Category
            {
                ExternalCode = slug, // Use slug as default external code
                Name = name,
                Slug = slug,
                ParentId = parentId,
                IsActive = true,
                SortOrder = 0
            };
        }
        
        public string Name { get; private set; } = null!;
        public string? Description { get; private set; }
        public string? ImageUrl { get; private set; }
        public string Slug { get; private set; } = null!;
        public int SortOrder { get; private set; }
        public bool IsActive { get; private set; }
        
        // Hierarchy
        public int? ParentId { get; private set; }
        public Category? Parent { get; private set; }
        public ICollection<Category> Children { get; private set; } = new List<Category>();
        
        public void Update(string name, string? description, int? parentId, bool isActive)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Category name is required");
            
            Name = name;
            Description = description;
            ParentId = parentId;
            IsActive = isActive;
            Slug = GenerateSlug(name);
        }
        
        public void SetSortOrder(int order) => SortOrder = order;
        public void SetImage(string? imageUrl) => ImageUrl = imageUrl;
        
        private static string GenerateSlug(string name)
        {
            return name.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("ı", "i").Replace("İ", "i")
                .Replace("ş", "s").Replace("Ş", "s")
                .Replace("ğ", "g").Replace("Ğ", "g")
                .Replace("ü", "u").Replace("Ü", "u")
                .Replace("ö", "o").Replace("Ö", "o")
                .Replace("ç", "c").Replace("Ç", "c");
        }
    }
}
```

### Brand Entity

```csharp
// File: Domain/Entities/Brand.cs
using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Exceptions;

namespace B2BCommerce.Backend.Domain.Entities
{
    public class Brand : ExternalEntity, IAggregateRoot
    {
        private Brand() { }
        
        public static Brand CreateFromExternal(
            string externalCode,
            string name,
            string? externalId = null)
        {
            if (string.IsNullOrWhiteSpace(externalCode))
                throw new DomainException("External code is required");
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Brand name is required");
            
            return new Brand
            {
                ExternalCode = externalCode,
                ExternalId = externalId,
                Name = name,
                IsActive = true,
                LastSyncedAt = DateTime.UtcNow
            };
        }
        
        public static Brand Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Brand name is required");
            
            var slug = name.ToLowerInvariant().Replace(" ", "-");
            
            return new Brand
            {
                ExternalCode = slug,
                Name = name,
                Slug = slug,
                IsActive = true
            };
        }
        
        public string Name { get; private set; } = null!;
        public string? Description { get; private set; }
        public string? LogoUrl { get; private set; }
        public string? WebsiteUrl { get; private set; }
        public string? Slug { get; private set; }
        public bool IsActive { get; private set; }
        
        // Navigation
        public ICollection<Product> Products { get; private set; } = new List<Product>();
        
        public void Update(string name, string? description, bool isActive)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Brand name is required");
            
            Name = name;
            Description = description;
            IsActive = isActive;
        }
        
        public void SetLogo(string? logoUrl) => LogoUrl = logoUrl;
        public void SetWebsite(string? websiteUrl) => WebsiteUrl = websiteUrl;
    }
}
```

---

## Application Layer

### Repository Interfaces

```csharp
// File: Application/Interfaces/Repositories/IRepository.cs
using System.Linq.Expressions;
using B2BCommerce.Backend.Domain.Common;

namespace B2BCommerce.Backend.Application.Interfaces.Repositories
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
    
    /// <summary>
    /// Extended repository for entities with external system integration
    /// </summary>
    public interface IExternalEntityRepository<T> : IRepository<T> where T : ExternalEntity
    {
        /// <summary>
        /// Find entity by external system code
        /// </summary>
        Task<T?> GetByExternalCodeAsync(string externalCode, CancellationToken ct = default);
        
        /// <summary>
        /// Check if entity exists by external code
        /// </summary>
        Task<bool> ExistsByExternalCodeAsync(string externalCode, CancellationToken ct = default);
        
        /// <summary>
        /// Get multiple entities by external codes (for bulk operations)
        /// </summary>
        Task<Dictionary<string, T>> GetByExternalCodesAsync(
            IEnumerable<string> externalCodes, 
            CancellationToken ct = default);
    }
}
```

```csharp
// File: Application/Interfaces/Repositories/IProductRepository.cs
using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Application.Interfaces.Repositories
{
    public interface IProductRepository : IExternalEntityRepository<Product>
    {
        Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);
        Task<Product?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId, CancellationToken ct = default);
        Task<IReadOnlyList<Product>> GetByBrandAsync(int brandId, CancellationToken ct = default);
        Task<IReadOnlyList<Product>> GetActiveProductsAsync(CancellationToken ct = default);
    }
    
    public interface ICategoryRepository : IExternalEntityRepository<Category>
    {
        Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Category>> GetChildrenAsync(int parentId, CancellationToken ct = default);
        Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default);
    }
    
    public interface IBrandRepository : IExternalEntityRepository<Brand>
    {
        Task<Brand?> GetBySlugAsync(string slug, CancellationToken ct = default);
    }
    
    public interface ICustomerRepository : IExternalEntityRepository<Customer>
    {
        Task<Customer?> GetByTaxNumberAsync(string taxNumber, CancellationToken ct = default);
    }
}
```

### Unit of Work

```csharp
// File: Application/Interfaces/IUnitOfWork.cs
namespace B2BCommerce.Backend.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Products { get; }
        ICategoryRepository Categories { get; }
        IBrandRepository Brands { get; }
        ICustomerRepository Customers { get; }
        // Add other repositories as needed
        
        Task<int> SaveChangesAsync(CancellationToken ct = default);
        Task BeginTransactionAsync(CancellationToken ct = default);
        Task CommitTransactionAsync(CancellationToken ct = default);
        Task RollbackTransactionAsync(CancellationToken ct = default);
    }
}
```

### Common Result Types

```csharp
// File: Application/Common/Result.cs
namespace B2BCommerce.Backend.Application.Common
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string? Error { get; }
        public List<string> Errors { get; } = new();
        
        protected Result(bool isSuccess, string? error = null)
        {
            IsSuccess = isSuccess;
            Error = error;
            if (error != null) Errors.Add(error);
        }
        
        protected Result(bool isSuccess, IEnumerable<string> errors)
        {
            IsSuccess = isSuccess;
            Errors = errors.ToList();
            Error = Errors.FirstOrDefault();
        }
        
        public static Result Success() => new(true);
        public static Result Failure(string error) => new(false, error);
        public static Result Failure(IEnumerable<string> errors) => new(false, errors);
        
        public static Result<T> Success<T>(T value) => new(value, true, null);
        public static Result<T> Failure<T>(string error) => new(default, false, error);
    }
    
    public class Result<T> : Result
    {
        public T? Value { get; }
        
        internal Result(T? value, bool isSuccess, string? error) : base(isSuccess, error)
        {
            Value = value;
        }
    }
    
    /// <summary>
    /// Result for upsert operations
    /// </summary>
    public class UpsertResult<T> : Result<T>
    {
        public bool IsNew { get; }
        public string? ExternalCode { get; }
        public int? EntityId { get; }
        
        private UpsertResult(T? value, bool isSuccess, bool isNew, string? externalCode, int? entityId, string? error) 
            : base(value, isSuccess, error)
        {
            IsNew = isNew;
            ExternalCode = externalCode;
            EntityId = entityId;
        }
        
        public static UpsertResult<T> Created(T value, string externalCode, int entityId) 
            => new(value, true, true, externalCode, entityId, null);
        
        public static UpsertResult<T> Updated(T value, string externalCode, int entityId) 
            => new(value, true, false, externalCode, entityId, null);
        
        public new static UpsertResult<T> Failure(string error) 
            => new(default, false, false, null, null, error);
    }
    
    /// <summary>
    /// Result for bulk operations
    /// </summary>
    public class BulkResult
    {
        public bool IsSuccess => FailedCount == 0;
        public int TotalCount { get; init; }
        public int CreatedCount { get; init; }
        public int UpdatedCount { get; init; }
        public int FailedCount { get; init; }
        public List<BulkItemResult> Items { get; init; } = new();
        public List<string> Errors { get; init; } = new();
    }
    
    public class BulkItemResult
    {
        public string ExternalCode { get; init; } = null!;
        public int? EntityId { get; init; }
        public bool IsNew { get; init; }
        public bool IsSuccess { get; init; }
        public string? Error { get; init; }
    }
}
```

### MediatR Commands (Single Source of Truth)

#### Product Commands

```csharp
// File: Application/Features/Products/Commands/UpsertProductCommand.cs
using MediatR;
using B2BCommerce.Backend.Application.Common;

namespace B2BCommerce.Backend.Application.Features.Products.Commands
{
    /// <summary>
    /// Upsert product - used by both B2B API and Integration API
    /// Matches by ExternalCode for integration, creates new for internal
    /// </summary>
    public record UpsertProductCommand : IRequest<UpsertResult<ProductDto>>
    {
        // Identification (one of these is required)
        public int? Id { get; init; }                    // For internal updates
        public string? ExternalCode { get; init; }       // For integration (upsert key)
        public string? ExternalId { get; init; }         // Optional LOGO internal ID
        
        // Required fields
        public required string Name { get; init; }
        public required string Sku { get; init; }
        
        // Category/Brand - support both ID and external code
        public int? CategoryId { get; init; }
        public string? CategoryExternalCode { get; init; }
        public int? BrandId { get; init; }
        public string? BrandExternalCode { get; init; }
        
        // Pricing
        public decimal ListPrice { get; init; }
        public decimal DealerPrice { get; init; }
        public decimal? CostPrice { get; init; }
        public string Currency { get; init; } = "TRY";
        
        // Optional fields
        public string? Description { get; init; }
        public string? ShortDescription { get; init; }
        public int StockQuantity { get; init; }
        public int MinimumOrderQuantity { get; init; } = 1;
        public bool IsActive { get; init; } = true;
        public bool RequiresSerialNumber { get; init; }
        public int? WarrantyMonths { get; init; }
        public decimal? Weight { get; init; }
        public string? WeightUnit { get; init; }
        public int? TaxRate { get; init; }
        public string? ProductTypeExternalCode { get; init; }
    }
}
```

```csharp
// File: Application/Features/Products/Commands/UpsertProductCommandHandler.cs
using MediatR;
using Microsoft.Extensions.Logging;
using B2BCommerce.Backend.Application.Common;
using B2BCommerce.Backend.Application.Interfaces;
using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Application.Features.Products.Commands
{
    public class UpsertProductCommandHandler 
        : IRequestHandler<UpsertProductCommand, UpsertResult<ProductDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpsertProductCommandHandler> _logger;
        
        public UpsertProductCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<UpsertProductCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        
        public async Task<UpsertResult<ProductDto>> Handle(
            UpsertProductCommand request, 
            CancellationToken cancellationToken)
        {
            // 1. Resolve Category
            int categoryId;
            if (request.CategoryId.HasValue)
            {
                categoryId = request.CategoryId.Value;
            }
            else if (!string.IsNullOrEmpty(request.CategoryExternalCode))
            {
                var category = await _unitOfWork.Categories
                    .GetByExternalCodeAsync(request.CategoryExternalCode, cancellationToken);
                
                if (category == null)
                    return UpsertResult<ProductDto>.Failure(
                        $"Category not found: {request.CategoryExternalCode}");
                
                categoryId = category.Id;
            }
            else
            {
                return UpsertResult<ProductDto>.Failure("Category is required");
            }
            
            // 2. Resolve Brand
            int brandId;
            if (request.BrandId.HasValue)
            {
                brandId = request.BrandId.Value;
            }
            else if (!string.IsNullOrEmpty(request.BrandExternalCode))
            {
                var brand = await _unitOfWork.Brands
                    .GetByExternalCodeAsync(request.BrandExternalCode, cancellationToken);
                
                if (brand == null)
                    return UpsertResult<ProductDto>.Failure(
                        $"Brand not found: {request.BrandExternalCode}");
                
                brandId = brand.Id;
            }
            else
            {
                return UpsertResult<ProductDto>.Failure("Brand is required");
            }
            
            // 3. Find or create product
            Product? product = null;
            bool isNew = false;
            
            // Try to find by ID first (internal update)
            if (request.Id.HasValue)
            {
                product = await _unitOfWork.Products
                    .GetByIdAsync(request.Id.Value, cancellationToken);
                
                if (product == null)
                    return UpsertResult<ProductDto>.Failure(
                        $"Product not found: {request.Id}");
            }
            // Then try by external code (integration upsert)
            else if (!string.IsNullOrEmpty(request.ExternalCode))
            {
                product = await _unitOfWork.Products
                    .GetByExternalCodeAsync(request.ExternalCode, cancellationToken);
            }
            
            if (product == null)
            {
                // Create new product
                isNew = true;
                var externalCode = request.ExternalCode ?? request.Sku;
                
                product = Product.CreateFromExternal(
                    externalCode: externalCode,
                    name: request.Name,
                    sku: request.Sku,
                    categoryId: categoryId,
                    brandId: brandId,
                    listPrice: request.ListPrice,
                    dealerPrice: request.DealerPrice,
                    currency: request.Currency,
                    externalId: request.ExternalId
                );
                
                _unitOfWork.Products.Add(product);
                
                _logger.LogInformation(
                    "Creating product: {ExternalCode} - {Name}", 
                    externalCode, request.Name);
            }
            else
            {
                // Update existing product
                product.Update(
                    name: request.Name,
                    description: request.Description,
                    shortDescription: request.ShortDescription,
                    categoryId: categoryId,
                    brandId: brandId,
                    isActive: request.IsActive
                );
                
                product.UpdatePricing(
                    request.ListPrice, 
                    request.DealerPrice, 
                    request.CostPrice);
                
                product.UpdateStock(request.StockQuantity);
                product.MarkAsSynced();
                
                if (request.ExternalId != null)
                    product.SetExternalId(request.ExternalId);
                
                _logger.LogInformation(
                    "Updating product: {ExternalCode} - {Name}", 
                    product.ExternalCode, request.Name);
            }
            
            // Apply optional fields
            product.SetWarranty(request.WarrantyMonths);
            product.SetSerialTracking(request.RequiresSerialNumber);
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            var dto = MapToDto(product);
            
            return isNew 
                ? UpsertResult<ProductDto>.Created(dto, product.ExternalCode, product.Id)
                : UpsertResult<ProductDto>.Updated(dto, product.ExternalCode, product.Id);
        }
        
        private static ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                ExternalCode = product.ExternalCode,
                Name = product.Name,
                Sku = product.Sku,
                ListPrice = product.ListPrice,
                DealerPrice = product.DealerPrice,
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive
            };
        }
    }
}
```

#### Price Update Command

```csharp
// File: Application/Features/Products/Commands/UpdateProductPriceCommand.cs
using MediatR;
using B2BCommerce.Backend.Application.Common;

namespace B2BCommerce.Backend.Application.Features.Products.Commands
{
    /// <summary>
    /// Update product pricing - lightweight command for price syncs
    /// </summary>
    public record UpdateProductPriceCommand : IRequest<Result>
    {
        // Identification (one required)
        public int? Id { get; init; }
        public string? ExternalCode { get; init; }
        
        public required decimal ListPrice { get; init; }
        public required decimal DealerPrice { get; init; }
        public decimal? CostPrice { get; init; }
    }
    
    public class UpdateProductPriceCommandHandler : IRequestHandler<UpdateProductPriceCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        
        public UpdateProductPriceCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        
        public async Task<Result> Handle(
            UpdateProductPriceCommand request, 
            CancellationToken cancellationToken)
        {
            Product? product = null;
            
            if (request.Id.HasValue)
            {
                product = await _unitOfWork.Products
                    .GetByIdAsync(request.Id.Value, cancellationToken);
            }
            else if (!string.IsNullOrEmpty(request.ExternalCode))
            {
                product = await _unitOfWork.Products
                    .GetByExternalCodeAsync(request.ExternalCode, cancellationToken);
            }
            
            if (product == null)
            {
                var identifier = request.Id?.ToString() ?? request.ExternalCode;
                return Result.Failure($"Product not found: {identifier}");
            }
            
            product.UpdatePricing(request.ListPrice, request.DealerPrice, request.CostPrice);
            product.MarkAsSynced();
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return Result.Success();
        }
    }
}
```

#### Stock Update Command

```csharp
// File: Application/Features/Products/Commands/UpdateProductStockCommand.cs
using MediatR;
using B2BCommerce.Backend.Application.Common;

namespace B2BCommerce.Backend.Application.Features.Products.Commands
{
    /// <summary>
    /// Update product stock - lightweight command for stock syncs
    /// </summary>
    public record UpdateProductStockCommand : IRequest<Result>
    {
        public int? Id { get; init; }
        public string? ExternalCode { get; init; }
        public required int Quantity { get; init; }
    }
    
    public class UpdateProductStockCommandHandler : IRequestHandler<UpdateProductStockCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        
        public UpdateProductStockCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        
        public async Task<Result> Handle(
            UpdateProductStockCommand request, 
            CancellationToken cancellationToken)
        {
            Product? product = null;
            
            if (request.Id.HasValue)
            {
                product = await _unitOfWork.Products
                    .GetByIdAsync(request.Id.Value, cancellationToken);
            }
            else if (!string.IsNullOrEmpty(request.ExternalCode))
            {
                product = await _unitOfWork.Products
                    .GetByExternalCodeAsync(request.ExternalCode, cancellationToken);
            }
            
            if (product == null)
            {
                var identifier = request.Id?.ToString() ?? request.ExternalCode;
                return Result.Failure($"Product not found: {identifier}");
            }
            
            product.UpdateStock(request.Quantity);
            product.MarkAsSynced();
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return Result.Success();
        }
    }
}
```

#### Bulk Commands

```csharp
// File: Application/Features/Products/Commands/BulkUpsertProductsCommand.cs
using MediatR;
using B2BCommerce.Backend.Application.Common;

namespace B2BCommerce.Backend.Application.Features.Products.Commands
{
    public record BulkUpsertProductsCommand : IRequest<BulkResult>
    {
        public required List<UpsertProductCommand> Products { get; init; }
    }
    
    public class BulkUpsertProductsCommandHandler : IRequestHandler<BulkUpsertProductsCommand, BulkResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        private readonly ILogger<BulkUpsertProductsCommandHandler> _logger;
        
        public BulkUpsertProductsCommandHandler(
            IUnitOfWork unitOfWork,
            IMediator mediator,
            ILogger<BulkUpsertProductsCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
            _logger = logger;
        }
        
        public async Task<BulkResult> Handle(
            BulkUpsertProductsCommand request, 
            CancellationToken cancellationToken)
        {
            var items = new List<BulkItemResult>();
            var errors = new List<string>();
            int created = 0, updated = 0, failed = 0;
            
            // Pre-load categories and brands for efficiency
            var categoryCodesNeeded = request.Products
                .Where(p => !string.IsNullOrEmpty(p.CategoryExternalCode))
                .Select(p => p.CategoryExternalCode!)
                .Distinct()
                .ToList();
            
            var brandCodesNeeded = request.Products
                .Where(p => !string.IsNullOrEmpty(p.BrandExternalCode))
                .Select(p => p.BrandExternalCode!)
                .Distinct()
                .ToList();
            
            var categories = await _unitOfWork.Categories
                .GetByExternalCodesAsync(categoryCodesNeeded, cancellationToken);
            
            var brands = await _unitOfWork.Brands
                .GetByExternalCodesAsync(brandCodesNeeded, cancellationToken);
            
            // Pre-load existing products
            var productCodesNeeded = request.Products
                .Where(p => !string.IsNullOrEmpty(p.ExternalCode))
                .Select(p => p.ExternalCode!)
                .ToList();
            
            var existingProducts = await _unitOfWork.Products
                .GetByExternalCodesAsync(productCodesNeeded, cancellationToken);
            
            foreach (var productCmd in request.Products)
            {
                try
                {
                    // Resolve category
                    int? categoryId = productCmd.CategoryId;
                    if (!categoryId.HasValue && !string.IsNullOrEmpty(productCmd.CategoryExternalCode))
                    {
                        if (categories.TryGetValue(productCmd.CategoryExternalCode, out var cat))
                            categoryId = cat.Id;
                        else
                        {
                            failed++;
                            errors.Add($"Category not found: {productCmd.CategoryExternalCode}");
                            items.Add(new BulkItemResult 
                            { 
                                ExternalCode = productCmd.ExternalCode ?? productCmd.Sku,
                                IsSuccess = false,
                                Error = $"Category not found: {productCmd.CategoryExternalCode}"
                            });
                            continue;
                        }
                    }
                    
                    // Resolve brand
                    int? brandId = productCmd.BrandId;
                    if (!brandId.HasValue && !string.IsNullOrEmpty(productCmd.BrandExternalCode))
                    {
                        if (brands.TryGetValue(productCmd.BrandExternalCode, out var brand))
                            brandId = brand.Id;
                        else
                        {
                            failed++;
                            errors.Add($"Brand not found: {productCmd.BrandExternalCode}");
                            items.Add(new BulkItemResult 
                            { 
                                ExternalCode = productCmd.ExternalCode ?? productCmd.Sku,
                                IsSuccess = false,
                                Error = $"Brand not found: {productCmd.BrandExternalCode}"
                            });
                            continue;
                        }
                    }
                    
                    var externalCode = productCmd.ExternalCode ?? productCmd.Sku;
                    bool isNew = !existingProducts.ContainsKey(externalCode);
                    
                    Product product;
                    if (isNew)
                    {
                        product = Product.CreateFromExternal(
                            externalCode: externalCode,
                            name: productCmd.Name,
                            sku: productCmd.Sku,
                            categoryId: categoryId!.Value,
                            brandId: brandId!.Value,
                            listPrice: productCmd.ListPrice,
                            dealerPrice: productCmd.DealerPrice,
                            currency: productCmd.Currency,
                            externalId: productCmd.ExternalId
                        );
                        
                        _unitOfWork.Products.Add(product);
                        existingProducts[externalCode] = product; // Track for subsequent iterations
                        created++;
                    }
                    else
                    {
                        product = existingProducts[externalCode];
                        product.Update(
                            productCmd.Name,
                            productCmd.Description,
                            productCmd.ShortDescription,
                            categoryId!.Value,
                            brandId!.Value,
                            productCmd.IsActive
                        );
                        product.UpdatePricing(
                            productCmd.ListPrice, 
                            productCmd.DealerPrice, 
                            productCmd.CostPrice);
                        product.UpdateStock(productCmd.StockQuantity);
                        product.MarkAsSynced();
                        updated++;
                    }
                    
                    items.Add(new BulkItemResult
                    {
                        ExternalCode = externalCode,
                        EntityId = product.Id,
                        IsNew = isNew,
                        IsSuccess = true
                    });
                }
                catch (Exception ex)
                {
                    failed++;
                    var code = productCmd.ExternalCode ?? productCmd.Sku;
                    errors.Add($"Error processing {code}: {ex.Message}");
                    items.Add(new BulkItemResult
                    {
                        ExternalCode = code,
                        IsSuccess = false,
                        Error = ex.Message
                    });
                    
                    _logger.LogError(ex, "Error processing product: {Code}", code);
                }
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return new BulkResult
            {
                TotalCount = request.Products.Count,
                CreatedCount = created,
                UpdatedCount = updated,
                FailedCount = failed,
                Items = items,
                Errors = errors
            };
        }
    }
}
```

---

## API Layer

### Integration API - Thin Controllers

```csharp
// File: IntegrationApi/Controllers/ProductsController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using B2BCommerce.Backend.Application.Features.Products.Commands;
using B2BCommerce.Backend.IntegrationApi.DTOs;

namespace B2BCommerce.Backend.IntegrationApi.Controllers
{
    /// <summary>
    /// Product sync endpoints for LOGO ERP integration
    /// </summary>
    [ApiController]
    [Route("api/integration/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;
        
        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        /// <summary>
        /// Sync a single product from LOGO
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SyncProduct(
            [FromBody] ProductSyncRequest request,
            CancellationToken cancellationToken)
        {
            // Map integration DTO to application command
            var command = new UpsertProductCommand
            {
                ExternalCode = request.Code,
                ExternalId = request.LogoId,
                Name = request.Name,
                Sku = request.Sku,
                Description = request.Description,
                CategoryExternalCode = request.CategoryCode,
                BrandExternalCode = request.BrandCode,
                ListPrice = request.ListPrice,
                DealerPrice = request.DealerPrice,
                CostPrice = request.CostPrice,
                Currency = request.Currency,
                StockQuantity = request.StockQuantity,
                IsActive = request.IsActive,
                WarrantyMonths = request.WarrantyMonths,
                RequiresSerialNumber = request.RequiresSerialNumber
            };
            
            var result = await _mediator.Send(command, cancellationToken);
            
            if (result.IsFailure)
                return BadRequest(new { success = false, error = result.Error });
            
            return Ok(new 
            { 
                success = true,
                externalCode = result.ExternalCode,
                vesmarketId = result.EntityId,
                isNew = result.IsNew
            });
        }
        
        /// <summary>
        /// Bulk sync products from LOGO
        /// </summary>
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkSyncProducts(
            [FromBody] BulkProductSyncRequest request,
            CancellationToken cancellationToken)
        {
            var commands = request.Products.Select(p => new UpsertProductCommand
            {
                ExternalCode = p.Code,
                ExternalId = p.LogoId,
                Name = p.Name,
                Sku = p.Sku,
                Description = p.Description,
                CategoryExternalCode = p.CategoryCode,
                BrandExternalCode = p.BrandCode,
                ListPrice = p.ListPrice,
                DealerPrice = p.DealerPrice,
                CostPrice = p.CostPrice,
                Currency = p.Currency,
                StockQuantity = p.StockQuantity,
                IsActive = p.IsActive
            }).ToList();
            
            var result = await _mediator.Send(
                new BulkUpsertProductsCommand { Products = commands }, 
                cancellationToken);
            
            return Ok(result);
        }
        
        /// <summary>
        /// Update product prices
        /// </summary>
        [HttpPost("prices")]
        public async Task<IActionResult> UpdatePrice(
            [FromBody] PriceSyncRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateProductPriceCommand
            {
                ExternalCode = request.Code,
                ListPrice = request.ListPrice,
                DealerPrice = request.DealerPrice,
                CostPrice = request.CostPrice
            };
            
            var result = await _mediator.Send(command, cancellationToken);
            
            if (result.IsFailure)
                return BadRequest(new { success = false, error = result.Error });
            
            return Ok(new { success = true });
        }
        
        /// <summary>
        /// Update product stock
        /// </summary>
        [HttpPost("stock")]
        public async Task<IActionResult> UpdateStock(
            [FromBody] StockSyncRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateProductStockCommand
            {
                ExternalCode = request.Code,
                Quantity = request.Quantity
            };
            
            var result = await _mediator.Send(command, cancellationToken);
            
            if (result.IsFailure)
                return BadRequest(new { success = false, error = result.Error });
            
            return Ok(new { success = true });
        }
    }
}
```

### Integration API DTOs (Separate from Application DTOs)

```csharp
// File: IntegrationApi/DTOs/ProductSyncRequest.cs
using System.ComponentModel.DataAnnotations;

namespace B2BCommerce.Backend.IntegrationApi.DTOs
{
    /// <summary>
    /// LOGO's product sync request format
    /// </summary>
    public class ProductSyncRequest
    {
        [Required, MaxLength(100)]
        public string Code { get; set; } = null!;
        
        [MaxLength(100)]
        public string? LogoId { get; set; }
        
        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;
        
        [Required, MaxLength(50)]
        public string Sku { get; set; } = null!;
        
        [MaxLength(4000)]
        public string? Description { get; set; }
        
        [Required, MaxLength(100)]
        public string CategoryCode { get; set; } = null!;
        
        [Required, MaxLength(100)]
        public string BrandCode { get; set; } = null!;
        
        [Range(0, double.MaxValue)]
        public decimal ListPrice { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal DealerPrice { get; set; }
        
        public decimal? CostPrice { get; set; }
        
        [MaxLength(3)]
        public string Currency { get; set; } = "TRY";
        
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public bool RequiresSerialNumber { get; set; }
        
        public int? WarrantyMonths { get; set; }
    }
    
    public class BulkProductSyncRequest
    {
        [Required, MinLength(1), MaxLength(1000)]
        public List<ProductSyncRequest> Products { get; set; } = new();
    }
    
    public class PriceSyncRequest
    {
        [Required, MaxLength(100)]
        public string Code { get; set; } = null!;
        
        [Range(0, double.MaxValue)]
        public decimal ListPrice { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal DealerPrice { get; set; }
        
        public decimal? CostPrice { get; set; }
    }
    
    public class StockSyncRequest
    {
        [Required, MaxLength(100)]
        public string Code { get; set; } = null!;
        
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
```

### B2B API - Same Commands, Different DTOs

```csharp
// File: B2BApi/Controllers/ProductsController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using B2BCommerce.Backend.Application.Features.Products.Commands;
using B2BCommerce.Backend.B2BApi.DTOs;

namespace B2BCommerce.Backend.B2BApi.Controllers
{
    [ApiController]
    [Route("api/products")]
    [Authorize(Roles = "Admin,ContentManager")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;
        
        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        /// <summary>
        /// Create or update a product (Admin panel)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateOrUpdate(
            [FromBody] ProductRequest request,
            CancellationToken cancellationToken)
        {
            // Map B2B DTO to same application command
            var command = new UpsertProductCommand
            {
                Id = request.Id,               // Use ID for internal updates
                Name = request.Name,
                Sku = request.Sku,
                Description = request.Description,
                CategoryId = request.CategoryId,  // Use IDs internally
                BrandId = request.BrandId,
                ListPrice = request.ListPrice,
                DealerPrice = request.DealerPrice,
                CostPrice = request.CostPrice,
                Currency = request.Currency,
                StockQuantity = request.StockQuantity,
                IsActive = request.IsActive
            };
            
            var result = await _mediator.Send(command, cancellationToken);
            
            if (result.IsFailure)
                return BadRequest(new { error = result.Error });
            
            return Ok(result.Value);
        }
        
        /// <summary>
        /// Update product price
        /// </summary>
        [HttpPut("{id}/price")]
        public async Task<IActionResult> UpdatePrice(
            int id,
            [FromBody] PriceUpdateRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateProductPriceCommand
            {
                Id = id,  // Use internal ID
                ListPrice = request.ListPrice,
                DealerPrice = request.DealerPrice,
                CostPrice = request.CostPrice
            };
            
            var result = await _mediator.Send(command, cancellationToken);
            
            if (result.IsFailure)
                return BadRequest(new { error = result.Error });
            
            return Ok();
        }
    }
}
```

---

## Infrastructure Layer

### Repository Implementation

```csharp
// File: Infrastructure/Repositories/ExternalEntityRepository.cs
using Microsoft.EntityFrameworkCore;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Infrastructure.Data;

namespace B2BCommerce.Backend.Infrastructure.Repositories
{
    public class ExternalEntityRepository<T> : Repository<T>, IExternalEntityRepository<T> 
        where T : ExternalEntity
    {
        public ExternalEntityRepository(ApplicationDbContext context) : base(context)
        {
        }
        
        public async Task<T?> GetByExternalCodeAsync(string externalCode, CancellationToken ct = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(e => e.ExternalCode == externalCode, ct);
        }
        
        public async Task<bool> ExistsByExternalCodeAsync(string externalCode, CancellationToken ct = default)
        {
            return await _dbSet
                .AnyAsync(e => e.ExternalCode == externalCode, ct);
        }
        
        public async Task<Dictionary<string, T>> GetByExternalCodesAsync(
            IEnumerable<string> externalCodes, 
            CancellationToken ct = default)
        {
            var codeList = externalCodes.ToList();
            
            return await _dbSet
                .Where(e => codeList.Contains(e.ExternalCode))
                .ToDictionaryAsync(e => e.ExternalCode, ct);
        }
    }
}
```

```csharp
// File: Infrastructure/Repositories/ProductRepository.cs
using Microsoft.EntityFrameworkCore;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Infrastructure.Data;

namespace B2BCommerce.Backend.Infrastructure.Repositories
{
    public class ProductRepository : ExternalEntityRepository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }
        
        public async Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.Sku == sku, ct);
        }
        
        public async Task<Product?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Include(p => p.SpecialPrices)
                .FirstOrDefaultAsync(p => p.Id == id, ct);
        }
        
        public async Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId, CancellationToken ct = default)
        {
            return await _dbSet
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .ToListAsync(ct);
        }
        
        public async Task<IReadOnlyList<Product>> GetByBrandAsync(int brandId, CancellationToken ct = default)
        {
            return await _dbSet
                .Where(p => p.BrandId == brandId && p.IsActive)
                .ToListAsync(ct);
        }
        
        public async Task<IReadOnlyList<Product>> GetActiveProductsAsync(CancellationToken ct = default)
        {
            return await _dbSet
                .Where(p => p.IsActive)
                .ToListAsync(ct);
        }
    }
}
```

### Entity Configuration

```csharp
// File: Infrastructure/Data/Configurations/ProductConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");
            
            builder.HasKey(p => p.Id);
            
            // External entity fields
            builder.Property(p => p.ExternalCode)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.HasIndex(p => p.ExternalCode)
                .IsUnique();
            
            builder.Property(p => p.ExternalId)
                .HasMaxLength(100);
            
            builder.Property(p => p.LastSyncedAt);
            
            // Product fields
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);
            
            builder.Property(p => p.Sku)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.HasIndex(p => p.Sku)
                .IsUnique();
            
            builder.Property(p => p.Description)
                .HasMaxLength(4000);
            
            builder.Property(p => p.ListPrice)
                .HasColumnType("decimal(18,4)")
                .IsRequired();
            
            builder.Property(p => p.DealerPrice)
                .HasColumnType("decimal(18,4)")
                .IsRequired();
            
            builder.Property(p => p.CostPrice)
                .HasColumnType("decimal(18,4)");
            
            builder.Property(p => p.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("TRY");
            
            builder.Property(p => p.StockQuantity)
                .IsRequired()
                .HasDefaultValue(0);
            
            builder.Property(p => p.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            
            // Relationships
            builder.HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Indexes
            builder.HasIndex(p => p.CategoryId);
            builder.HasIndex(p => p.BrandId);
            builder.HasIndex(p => p.IsActive);
            builder.HasIndex(p => p.LastSyncedAt);
        }
    }
}
```

---

## Summary

### Architecture Benefits

| Aspect | Implementation |
|--------|---------------|
| **Single Source of Truth** | MediatR commands contain all business logic |
| **No Code Duplication** | Both APIs use same command handlers |
| **SOLID - Single Responsibility** | Controllers map DTOs, Handlers execute logic |
| **SOLID - Open/Closed** | Add new commands without modifying existing |
| **SOLID - Dependency Inversion** | Depend on IUnitOfWork, IRepository abstractions |
| **Clean Architecture** | Clear layer separation, dependencies point inward |

### Flow Comparison

```
Integration API:
ProductSyncRequest → Map → UpsertProductCommand → Handler → Result → Map → Response

B2B API:
ProductRequest → Map → UpsertProductCommand → Handler → Result → Map → Response
                              ↑
                      Same Handler!
```

### Key Design Decisions

1. **Commands accept both ID and ExternalCode** - Handlers resolve appropriately
2. **Integration DTOs separate from Application DTOs** - API-specific validation
3. **Repository has GetByExternalCodeAsync** - Clean abstraction for external lookups
4. **Bulk operations optimize with pre-loading** - Efficient database access
5. **Result types carry context** - IsNew, ExternalCode, EntityId for responses
