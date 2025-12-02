# Infrastructure Layer Specification - B2B E-Commerce Platform

## Document Overview

**Purpose**: This document provides detailed specifications and implementation
guidelines for the Infrastructure Layer of the B2B E-Commerce Platform. The
Infrastructure Layer implements data access, external service integrations, and
all technical concerns that interact with external systems.

**Target Audience**: Backend developers, DevOps engineers, system architects

**Framework**: .NET 8, Entity Framework Core 8, PostgreSQL, Clean Architecture

**Last Updated**: November 2025

---

## Table of Contents

1. [Infrastructure Layer Overview](#infrastructure-layer-overview)
2. [Database Infrastructure](#database-infrastructure)
3. [Repository Pattern Implementation](#repository-pattern-implementation)
4. [Unit of Work Pattern](#unit-of-work-pattern)
5. [External Service Integrations](#external-service-integrations)
6. [Caching Infrastructure](#caching-infrastructure)
7. [File Storage Infrastructure](#file-storage-infrastructure)
8. [Background Jobs Infrastructure](#background-jobs-infrastructure)
9. [Logging Infrastructure](#logging-infrastructure)
10. [Dependency Injection Configuration](#dependency-injection-configuration)

---

## Infrastructure Layer Overview

### Purpose

The Infrastructure Layer serves as the technical implementation layer that:

- Implements data access logic through repositories
- Manages database connections and transactions
- Integrates with external services (payment gateways, shipping APIs, email/SMS
  services)
- Handles file storage operations
- Manages caching strategies
- Configures background job processing
- Implements logging and monitoring

### Characteristics

- **Dependencies**: Can reference Application Layer (interfaces) and Domain
  Layer (entities)
- **External Libraries**: Uses EF Core, HTTP clients, AWS SDK, Redis clients,
  etc.
- **No Business Logic**: Contains only technical implementation, no business
  rules
- **Implements Interfaces**: All services implement interfaces defined in
  Application Layer

### Project Structure

```
B2BCommerce.Backend.Infrastructure/
│
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── UnitOfWork.cs
│   ├── Configurations/
│   │   ├── ProductConfiguration.cs
│   │   ├── OrderConfiguration.cs
│   │   ├── CustomerConfiguration.cs
│   │   └── ... (entity configurations)
│   ├── Migrations/
│   │   └── ... (EF Core migrations)
│   └── Interceptors/
│       ├── AuditInterceptor.cs
│       └── SoftDeleteInterceptor.cs
│
├── Repositories/
│   ├── GenericRepository.cs
│   ├── ProductRepository.cs
│   ├── OrderRepository.cs
│   ├── CustomerRepository.cs
│   ├── CategoryRepository.cs
│   ├── BrandRepository.cs
│   ├── PaymentRepository.cs
│   └── ... (other repositories)
│
├── Services/
│   ├── Email/
│   │   ├── EmailService.cs
│   │   ├── EmailTemplateService.cs
│   │   └── Models/
│   ├── SMS/
│   │   └── SmsService.cs
│   ├── FileStorage/
│   │   ├── S3FileStorageService.cs
│   │   └── LocalFileStorageService.cs
│   └── Cache/
│       ├── RedisCacheService.cs
│       └── MemoryCacheService.cs
│
├── ExternalApis/
│   ├── Payment/
│   │   ├── PaynetClient.cs
│   │   ├── Models/
│   │   └── Responses/
│   ├── Shipping/
│   │   ├── CargoApiClient.cs
│   │   └── Models/
│   ├── Erp/
│   │   ├── LogoErpClient.cs
│   │   └── Models/
│   └── Common/
│       ├── HttpClientBase.cs
│       └── HttpRetryPolicy.cs
│
├── BackgroundJobs/
│   ├── OrderSyncJob.cs
│   ├── StockUpdateJob.cs
│   ├── PriceUpdateJob.cs
│   └── EmailNotificationJob.cs
│
├── Identity/
│   ├── ApplicationUser.cs
│   ├── ApplicationRole.cs
│   └── JwtTokenService.cs
│
└── DependencyInjection.cs
```

---

## Database Infrastructure

### ApplicationDbContext

The DbContext is the primary class for database operations, managing entity
configurations, change tracking, and database connections.

```csharp
// File: Infrastructure/Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Common;

namespace B2BCommerce.Backend.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        #region DbSets
        
        // Product Management
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Brand> Brands => Set<Brand>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<ProductSpecialPrice> ProductSpecialPrices => Set<ProductSpecialPrice>();
        public DbSet<ProductTierPrice> ProductTierPrices => Set<ProductTierPrice>();
        public DbSet<ProductStock> ProductStocks => Set<ProductStock>();
        
        // Customer Management
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
        public DbSet<CustomerCredit> CustomerCredits => Set<CustomerCredit>();
        public DbSet<CustomerPriceList> CustomerPriceLists => Set<CustomerPriceList>();
        
        // Order Management
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<OrderApproval> OrderApprovals => Set<OrderApproval>();
        public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
        
        // Payment Management
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
        public DbSet<RefundRequest> RefundRequests => Set<RefundRequest>();
        
        // Shipping Management
        public DbSet<Shipment> Shipments => Set<Shipment>();
        public DbSet<ShippingMethod> ShippingMethods => Set<ShippingMethod>();
        public DbSet<ShippingAddress> ShippingAddresses => Set<ShippingAddress>();
        
        // System Configuration
        public DbSet<SystemConfiguration> SystemConfigurations => Set<SystemConfiguration>();
        public DbSet<CurrencyRate> CurrencyRates => Set<CurrencyRate>();
        public DbSet<TaxRate> TaxRates => Set<TaxRate>();
        public DbSet<DiscountRule> DiscountRules => Set<DiscountRule>();
        
        // Audit & Logging
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<ErrorLog> ErrorLogs => Set<ErrorLog>();
        
        // Integration
        public DbSet<ErpSyncLog> ErpSyncLogs => Set<ErpSyncLog>();
        public DbSet<WebhookLog> WebhookLogs => Set<WebhookLog>();
        
        #endregion
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Apply all entity configurations from assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            
            // Global query filters for soft deletes
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .HasQueryFilter(BuildSoftDeleteFilter(entityType.ClrType));
                }
            }
            
            // Configure decimal precision globally
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,2)");
            }
        }
        
        private static LambdaExpression BuildSoftDeleteFilter(Type entityType)
        {
            var parameter = Expression.Parameter(entityType, "e");
            var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
            var body = Expression.Equal(property, Expression.Constant(false));
            return Expression.Lambda(body, parameter);
        }
        
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Handle audit fields automatically
            HandleAuditFields();
            
            // Handle soft deletes
            HandleSoftDeletes();
            
            return await base.SaveChangesAsync(cancellationToken);
        }
        
        private void HandleAuditFields()
        {
            var entries = ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
            
            var currentUserId = GetCurrentUserId(); // Implement based on your auth system
            var currentTime = DateTime.UtcNow;
            
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = currentTime;
                    entry.Entity.CreatedBy = currentUserId;
                }
                
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = currentTime;
                    entry.Entity.UpdatedBy = currentUserId;
                }
            }
        }
        
        private void HandleSoftDeletes()
        {
            var deletedEntries = ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Deleted);
            
            foreach (var entry in deletedEntries)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = DateTime.UtcNow;
                entry.Entity.DeletedBy = GetCurrentUserId();
            }
        }
        
        private string? GetCurrentUserId()
        {
            // TODO: Implement based on your authentication system
            // This could come from IHttpContextAccessor or a scoped service
            return null;
        }
    }
}
```

### Entity Configuration Examples

#### Product Configuration

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
            
            // Primary Key
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
            
            builder.Property(p => p.CostPrice)
                .HasColumnType("decimal(18,2)");
            
            builder.Property(p => p.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("TRY");
            
            builder.Property(p => p.StockQuantity)
                .IsRequired()
                .HasDefaultValue(0);
            
            builder.Property(p => p.MinimumOrderQuantity)
                .HasDefaultValue(1);
            
            builder.Property(p => p.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            
            builder.Property(p => p.Weight)
                .HasColumnType("decimal(10,2)");
            
            builder.Property(p => p.WarrantyMonths)
                .HasDefaultValue(0);
            
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
            
            builder.HasMany(p => p.SpecialPrices)
                .WithOne(sp => sp.Product)
                .HasForeignKey(sp => sp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            builder.HasIndex(p => p.CategoryId);
            builder.HasIndex(p => p.BrandId);
            builder.HasIndex(p => p.IsActive);
            builder.HasIndex(p => p.CreatedAt);
            builder.HasIndex(p => new { p.Name, p.Sku });
        }
    }
}
```

#### Order Configuration

```csharp
// File: Infrastructure/Data/Configurations/OrderConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");
            
            // Primary Key
            builder.HasKey(o => o.Id);
            
            // Properties
            builder.Property(o => o.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.HasIndex(o => o.OrderNumber)
                .IsUnique();
            
            builder.Property(o => o.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
            
            builder.Property(o => o.Subtotal)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            
            builder.Property(o => o.TaxAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            
            builder.Property(o => o.ShippingCost)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            
            builder.Property(o => o.DiscountAmount)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);
            
            builder.Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            
            builder.Property(o => o.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("TRY");
            
            builder.Property(o => o.ExchangeRate)
                .HasColumnType("decimal(10,4)")
                .HasDefaultValue(1);
            
            // Relationships
            builder.HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(o => o.ShippingAddress)
                .WithMany()
                .HasForeignKey(o => o.ShippingAddressId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasMany(o => o.Items)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasMany(o => o.StatusHistory)
                .WithOne(sh => sh.Order)
                .HasForeignKey(sh => sh.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            builder.HasIndex(o => o.CustomerId);
            builder.HasIndex(o => o.Status);
            builder.HasIndex(o => o.OrderDate);
            builder.HasIndex(o => new { o.CustomerId, o.Status });
        }
    }
}
```

#### Customer Configuration

```csharp
// File: Infrastructure/Data/Configurations/CustomerConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using B2BCommerce.Backend.Domain.Entities;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customers");
            
            // Primary Key
            builder.HasKey(c => c.Id);
            
            // Properties
            builder.Property(c => c.CompanyName)
                .IsRequired()
                .HasMaxLength(200);
            
            builder.Property(c => c.TaxNumber)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.HasIndex(c => c.TaxNumber)
                .IsUnique();
            
            builder.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.HasIndex(c => c.Email);
            
            builder.Property(c => c.CreditLimit)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);
            
            builder.Property(c => c.UsedCredit)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);
            
            builder.Property(c => c.PaymentTermDays)
                .HasDefaultValue(30);
            
            builder.Property(c => c.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            
            builder.Property(c => c.IsApproved)
                .IsRequired()
                .HasDefaultValue(false);
            
            // Relationships
            builder.HasMany(c => c.Addresses)
                .WithOne(a => a.Customer)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasMany(c => c.Orders)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(c => c.CreditInfo)
                .WithOne(ci => ci.Customer)
                .HasForeignKey<CustomerCredit>(ci => ci.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            builder.HasIndex(c => c.CompanyName);
            builder.HasIndex(c => c.IsActive);
            builder.HasIndex(c => c.IsApproved);
        }
    }
}
```

### Database Migration Commands

```bash
# Create initial migration
dotnet ef migrations add InitialCreate \
  --project src/B2BCommerce.Backend.Infrastructure \
  --startup-project src/B2BCommerce.Backend.API

# Add subsequent migrations
dotnet ef migrations add AddProductTierPricing \
  --project src/B2BCommerce.Backend.Infrastructure \
  --startup-project src/B2BCommerce.Backend.API

# Update database
dotnet ef database update \
  --project src/B2BCommerce.Backend.Infrastructure \
  --startup-project src/B2BCommerce.Backend.API

# Remove last migration (if not applied)
dotnet ef migrations remove \
  --project src/B2BCommerce.Backend.Infrastructure \
  --startup-project src/B2BCommerce.Backend.API

# Generate SQL script
dotnet ef migrations script \
  --project src/B2BCommerce.Backend.Infrastructure \
  --startup-project src/B2BCommerce.Backend.API \
  --output migration.sql

# Update to specific migration
dotnet ef database update MigrationName \
  --project src/B2BCommerce.Backend.Infrastructure \
  --startup-project src/B2BCommerce.Backend.API
```

---

## Repository Pattern Implementation

### Generic Repository

The generic repository provides common CRUD operations for all entities.

```csharp
// File: Infrastructure/Repositories/GenericRepository.cs
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Infrastructure.Data;

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
        
        public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }
        
        public async Task<T?> GetByIdWithIncludesAsync(
            int id,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            
            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }
        
        public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }
        
        public async Task<List<T>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            
            if (filter != null)
            {
                query = query.Where(filter);
            }
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            
            if (orderBy != null)
            {
                query = orderBy(query);
            }
            
            return await query.ToListAsync();
        }
        
        public async Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> filter,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            
            return await query.FirstOrDefaultAsync(filter);
        }
        
        public async Task<bool> AnyAsync(
            Expression<Func<T, bool>> filter,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(filter, cancellationToken);
        }
        
        public async Task<int> CountAsync(
            Expression<Func<T, bool>>? filter = null,
            CancellationToken cancellationToken = default)
        {
            if (filter == null)
                return await _dbSet.CountAsync(cancellationToken);
            
            return await _dbSet.CountAsync(filter, cancellationToken);
        }
        
        public async Task<PaginatedList<T>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            
            if (filter != null)
            {
                query = query.Where(filter);
            }
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            
            var totalCount = await query.CountAsync();
            
            if (orderBy != null)
            {
                query = orderBy(query);
            }
            
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return new PaginatedList<T>(items, totalCount, pageNumber, pageSize);
        }
        
        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
        }
        
        public async Task AddRangeAsync(
            IEnumerable<T> entities,
            CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
        }
        
        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }
        
        public void UpdateRange(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
        }
        
        public void Delete(T entity)
        {
            // Soft delete is handled in DbContext.SaveChangesAsync()
            _dbSet.Remove(entity);
        }
        
        public void DeleteRange(IEnumerable<T> entities)
        {
            // Soft delete is handled in DbContext.SaveChangesAsync()
            _dbSet.RemoveRange(entities);
        }
        
        public async Task<bool> ExistsAsync(
            Expression<Func<T, bool>> filter,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(filter, cancellationToken);
        }
    }
}
```

### Specific Repository Implementations

#### Product Repository

```csharp
// File: Infrastructure/Repositories/ProductRepository.cs
using Microsoft.EntityFrameworkCore;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Infrastructure.Data;

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
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Sku == sku);
        }
        
        public async Task<Product?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Include(p => p.TierPrices)
                .Include(p => p.SpecialPrices)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        
        public async Task<List<Product>> GetByCategoryAsync(int categoryId)
        {
            return await _dbSet
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }
        
        public async Task<List<Product>> GetByBrandAsync(int brandId)
        {
            return await _dbSet
                .Where(p => p.BrandId == brandId && p.IsActive)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }
        
        public async Task<List<Product>> SearchAsync(string searchTerm)
        {
            var lowerSearchTerm = searchTerm.ToLower();
            
            return await _dbSet
                .Where(p => p.IsActive && 
                           (p.Name.ToLower().Contains(lowerSearchTerm) || 
                            p.Sku.ToLower().Contains(lowerSearchTerm) ||
                            (p.Description != null && p.Description.ToLower().Contains(lowerSearchTerm))))
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }
        
        public async Task<decimal?> GetSpecialPriceAsync(int productId, int customerId)
        {
            var specialPrice = await _context.ProductSpecialPrices
                .Where(sp => sp.ProductId == productId && 
                            sp.CustomerId == customerId &&
                            sp.IsActive)
                .OrderByDescending(sp => sp.CreatedAt)
                .FirstOrDefaultAsync();
            
            return specialPrice?.Price;
        }
        
        public async Task<decimal?> GetTierPriceAsync(int productId, PriceTier tier)
        {
            var tierPrice = await _context.ProductTierPrices
                .Where(tp => tp.ProductId == productId && 
                            tp.Tier == tier)
                .FirstOrDefaultAsync();
            
            return tierPrice?.Price;
        }
        
        public async Task<List<Product>> GetLowStockProductsAsync(int threshold = 10)
        {
            return await _dbSet
                .Where(p => p.IsActive && p.StockQuantity <= threshold)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();
        }
        
        public async Task UpdateStockAsync(int productId, int quantity)
        {
            var product = await _dbSet.FindAsync(productId);
            if (product != null)
            {
                product.UpdateStock(quantity);
            }
        }
        
        public async Task<bool> IsSkuUniqueAsync(string sku, int? excludeProductId = null)
        {
            var query = _dbSet.Where(p => p.Sku == sku);
            
            if (excludeProductId.HasValue)
            {
                query = query.Where(p => p.Id != excludeProductId.Value);
            }
            
            return !await query.AnyAsync();
        }
    }
}
```

#### Order Repository

```csharp
// File: Infrastructure/Repositories/OrderRepository.cs
using Microsoft.EntityFrameworkCore;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Infrastructure.Data;

namespace B2BCommerce.Backend.Infrastructure.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }
        
        public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
        {
            return await _dbSet
                .Include(o => o.Customer)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.ShippingAddress)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }
        
        public async Task<Order?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(o => o.Customer)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Images)
                .Include(o => o.ShippingAddress)
                .Include(o => o.StatusHistory)
                .Include(o => o.Approval)
                .FirstOrDefaultAsync(o => o.Id == id);
        }
        
        public async Task<List<Order>> GetByCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.Items)
                .Include(o => o.ShippingAddress)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
        
        public async Task<List<Order>> GetPendingOrdersAsync()
        {
            return await _dbSet
                .Where(o => o.Status == OrderStatus.Pending)
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .OrderBy(o => o.OrderDate)
                .ToListAsync();
        }
        
        public async Task<List<Order>> GetPendingOrdersByCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(o => o.CustomerId == customerId && o.Status == OrderStatus.Pending)
                .Include(o => o.Items)
                .OrderBy(o => o.OrderDate)
                .ToListAsync();
        }
        
        public async Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            return await _dbSet
                .Where(o => o.Status == status)
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
        
        public async Task<List<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
        
        public async Task<decimal> GetTotalOrderAmountByCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(o => o.CustomerId == customerId && 
                           (o.Status == OrderStatus.Approved || o.Status == OrderStatus.Shipped))
                .SumAsync(o => o.TotalAmount);
        }
        
        public async Task<int> GetOrderCountByStatusAsync(OrderStatus status)
        {
            return await _dbSet
                .CountAsync(o => o.Status == status);
        }
        
        public async Task<string> GenerateOrderNumberAsync()
        {
            var today = DateTime.UtcNow;
            var prefix = $"ORD{today:yyyyMMdd}";
            
            var lastOrder = await _dbSet
                .Where(o => o.OrderNumber.StartsWith(prefix))
                .OrderByDescending(o => o.OrderNumber)
                .FirstOrDefaultAsync();
            
            if (lastOrder == null)
            {
                return $"{prefix}0001";
            }
            
            var lastNumber = int.Parse(lastOrder.OrderNumber.Substring(prefix.Length));
            var newNumber = lastNumber + 1;
            
            return $"{prefix}{newNumber:D4}";
        }
    }
}
```

#### Customer Repository

```csharp
// File: Infrastructure/Repositories/CustomerRepository.cs
using Microsoft.EntityFrameworkCore;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Infrastructure.Data;

namespace B2BCommerce.Backend.Infrastructure.Repositories
{
    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(ApplicationDbContext context) : base(context)
        {
        }
        
        public async Task<Customer?> GetByTaxNumberAsync(string taxNumber)
        {
            return await _dbSet
                .Include(c => c.Addresses)
                .Include(c => c.CreditInfo)
                .FirstOrDefaultAsync(c => c.TaxNumber == taxNumber);
        }
        
        public async Task<Customer?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .Include(c => c.Addresses)
                .FirstOrDefaultAsync(c => c.Email == email);
        }
        
        public async Task<Customer?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Addresses)
                .Include(c => c.CreditInfo)
                .Include(c => c.Orders.OrderByDescending(o => o.OrderDate).Take(10))
                .FirstOrDefaultAsync(c => c.Id == id);
        }
        
        public async Task<List<Customer>> GetPendingApprovalAsync()
        {
            return await _dbSet
                .Where(c => !c.IsApproved && c.IsActive)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }
        
        public async Task<List<Customer>> GetActiveCustomersAsync()
        {
            return await _dbSet
                .Where(c => c.IsActive && c.IsApproved)
                .OrderBy(c => c.CompanyName)
                .ToListAsync();
        }
        
        public async Task<bool> IsTaxNumberUniqueAsync(string taxNumber, int? excludeCustomerId = null)
        {
            var query = _dbSet.Where(c => c.TaxNumber == taxNumber);
            
            if (excludeCustomerId.HasValue)
            {
                query = query.Where(c => c.Id != excludeCustomerId.Value);
            }
            
            return !await query.AnyAsync();
        }
        
        public async Task<bool> IsEmailUniqueAsync(string email, int? excludeCustomerId = null)
        {
            var query = _dbSet.Where(c => c.Email == email);
            
            if (excludeCustomerId.HasValue)
            {
                query = query.Where(c => c.Id != excludeCustomerId.Value);
            }
            
            return !await query.AnyAsync();
        }
        
        public async Task UpdateCreditAsync(int customerId, decimal amount)
        {
            var customer = await _dbSet
                .Include(c => c.CreditInfo)
                .FirstOrDefaultAsync(c => c.Id == customerId);
            
            if (customer != null)
            {
                customer.UpdateCredit(amount);
            }
        }
        
        public async Task<List<Customer>> GetCustomersExceedingCreditLimitAsync()
        {
            return await _dbSet
                .Where(c => c.IsActive && c.UsedCredit > c.CreditLimit * 0.9m) // 90% threshold
                .Include(c => c.CreditInfo)
                .OrderByDescending(c => c.UsedCredit)
                .ToListAsync();
        }
    }
}
```

---

## Unit of Work Pattern

The Unit of Work pattern manages transactions across multiple repositories.

```csharp
// File: Infrastructure/Data/UnitOfWork.cs
using B2BCommerce.Backend.Application.Interfaces;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        
        // Repository instances (lazy initialization)
        private IProductRepository? _products;
        private IOrderRepository? _orders;
        private ICustomerRepository? _customers;
        private ICategoryRepository? _categories;
        private IBrandRepository? _brands;
        private IPaymentRepository? _payments;
        private IShipmentRepository? _shipments;
        
        public UnitOfWork(
            ApplicationDbContext context,
            ILogger<UnitOfWork> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        // Repository properties with lazy initialization
        public IProductRepository Products =>
            _products ??= new ProductRepository(_context);
        
        public IOrderRepository Orders =>
            _orders ??= new OrderRepository(_context);
        
        public ICustomerRepository Customers =>
            _customers ??= new CustomerRepository(_context);
        
        public ICategoryRepository Categories =>
            _categories ??= new CategoryRepository(_context);
        
        public IBrandRepository Brands =>
            _brands ??= new BrandRepository(_context);
        
        public IPaymentRepository Payments =>
            _payments ??= new PaymentRepository(_context);
        
        public IShipmentRepository Shipments =>
            _shipments ??= new ShipmentRepository(_context);
        
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving changes to database");
                throw;
            }
        }
        
        public async Task<bool> SaveChangesReturnBoolAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving changes to database");
                return false;
            }
        }
        
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
```

### Benefits of Unit of Work

1. **Atomic Transactions**: All repository operations are saved together or
   rolled back together
2. **Coordinated Saves**: Single SaveChanges call for multiple entity operations
3. **Better Performance**: Reduces database round trips
4. **Centralized Error Handling**: Single point for transaction error handling
5. **Resource Management**: Proper disposal of DbContext

---

## External Service Integrations

### HTTP Client Base Class

```csharp
// File: Infrastructure/ExternalApis/Common/HttpClientBase.cs
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Infrastructure.ExternalApis.Common
{
    public abstract class HttpClientBase
    {
        protected readonly HttpClient _httpClient;
        protected readonly ILogger _logger;
        protected readonly JsonSerializerOptions _jsonOptions;
        
        protected HttpClientBase(HttpClient httpClient, ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }
        
        protected async Task<TResponse?> GetAsync<TResponse>(
            string endpoint,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("GET request to {Endpoint}", endpoint);
                
                var response = await _httpClient.GetAsync(endpoint, cancellationToken);
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GET request to {Endpoint}", endpoint);
                throw;
            }
        }
        
        protected async Task<TResponse?> PostAsync<TRequest, TResponse>(
            string endpoint,
            TRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("POST request to {Endpoint}", endpoint);
                
                var response = await _httpClient.PostAsJsonAsync(endpoint, request, _jsonOptions, cancellationToken);
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in POST request to {Endpoint}", endpoint);
                throw;
            }
        }
    }
}
```

### Payment Gateway Integration (Paynet)

```csharp
// File: Infrastructure/ExternalApis/Payment/PaynetClient.cs
using B2BCommerce.Backend.Application.Interfaces.ExternalServices;
using B2BCommerce.Backend.Infrastructure.ExternalApis.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Infrastructure.ExternalApis.Payment
{
    public class PaynetClient : HttpClientBase, IPaymentGatewayService
    {
        private readonly string _merchantId;
        private readonly string _merchantKey;
        
        public PaynetClient(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<PaynetClient> logger) : base(httpClient, logger)
        {
            _merchantId = configuration["Paynet:MerchantId"]!;
            _merchantKey = configuration["Paynet:MerchantKey"]!;
            _httpClient.BaseAddress = new Uri(configuration["Paynet:BaseUrl"]!);
        }
        
        public async Task<PaymentInitiationResult> InitiatePaymentAsync(
            string orderId,
            decimal amount,
            string currency,
            string customerEmail,
            string returnUrl,
            CancellationToken cancellationToken = default)
        {
            var request = new PaynetPaymentRequest
            {
                MerchantId = _merchantId,
                OrderId = orderId,
                Amount = amount,
                Currency = currency,
                CustomerEmail = customerEmail,
                ReturnUrl = returnUrl,
                Signature = GenerateSignature(orderId, amount, currency)
            };
            
            var response = await PostAsync<PaynetPaymentRequest, PaynetPaymentResponse>(
                "/api/payment/initiate",
                request,
                cancellationToken);
            
            return new PaymentInitiationResult
            {
                Success = response.Success,
                TransactionId = response.TransactionId,
                PaymentUrl = response.PaymentUrl
            };
        }
        
        public async Task<PaymentVerificationResult> VerifyPaymentAsync(
            string transactionId,
            CancellationToken cancellationToken = default)
        {
            var response = await GetAsync<PaynetVerificationResponse>(
                $"/api/payment/verify/{transactionId}",
                cancellationToken);
            
            return new PaymentVerificationResult
            {
                Success = response.Status == "APPROVED",
                TransactionId = response.TransactionId,
                OrderId = response.OrderId,
                Amount = response.Amount,
                Status = response.Status
            };
        }
        
        private string GenerateSignature(string orderId, decimal amount, string currency)
        {
            var data = $"{_merchantId}{orderId}{amount}{currency}{_merchantKey}";
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hash);
        }
    }
}
```

### Email Service Implementation

```csharp
// File: Infrastructure/Services/Email/EmailService.cs
using System.Net;
using System.Net.Mail;
using B2BCommerce.Backend.Application.Interfaces.ExternalServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Infrastructure.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly IEmailTemplateService _templateService;
        
        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger,
            IEmailTemplateService templateService)
        {
            _configuration = configuration;
            _logger = logger;
            _templateService = templateService;
        }
        
        public async Task SendOrderConfirmationEmailAsync(
            string toEmail,
            string orderNumber,
            decimal totalAmount,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var subject = $"Order Confirmation - {orderNumber}";
                var body = await _templateService.GetOrderConfirmationTemplateAsync(
                    orderNumber, totalAmount);
                
                await SendEmailAsync(toEmail, subject, body, cancellationToken);
                
                _logger.LogInformation(
                    "Order confirmation email sent to {Email} for order {OrderNumber}", 
                    toEmail, orderNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send order confirmation email to {Email}", toEmail);
                throw;
            }
        }
        
        public async Task SendOrderApprovedEmailAsync(
            string toEmail,
            string orderNumber,
            CancellationToken cancellationToken = default)
        {
            var subject = $"Order Approved - {orderNumber}";
            var body = await _templateService.GetOrderApprovedTemplateAsync(orderNumber);
            await SendEmailAsync(toEmail, subject, body, cancellationToken);
        }
        
        public async Task SendPaymentConfirmationEmailAsync(
            string toEmail,
            string transactionId,
            decimal amount,
            CancellationToken cancellationToken = default)
        {
            var subject = $"Payment Confirmation - {transactionId}";
            var body = await _templateService.GetPaymentConfirmationTemplateAsync(
                transactionId, amount);
            await SendEmailAsync(toEmail, subject, body, cancellationToken);
        }
        
        private async Task SendEmailAsync(
            string toEmail,
            string subject,
            string body,
            CancellationToken cancellationToken = default)
        {
            var smtpHost = _configuration["Email:SmtpHost"]!;
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUsername = _configuration["Email:Username"]!;
            var smtpPassword = _configuration["Email:Password"]!;
            var fromEmail = _configuration["Email:FromEmail"]!;
            var fromName = _configuration["Email:FromName"] ?? "B2B Commerce";
            
            using var smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                EnableSsl = true
            };
            
            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            
            mailMessage.To.Add(toEmail);
            
            await smtpClient.SendMailAsync(mailMessage, cancellationToken);
        }
    }
}
```

### File Storage Service (AWS S3)

```csharp
// File: Infrastructure/Services/FileStorage/S3FileStorageService.cs
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using B2BCommerce.Backend.Application.Interfaces.ExternalServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Infrastructure.Services.FileStorage
{
    public class S3FileStorageService : IFileStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly ILogger<S3FileStorageService> _logger;
        private readonly string _bucketName;
        
        public S3FileStorageService(
            IAmazonS3 s3Client,
            IConfiguration configuration,
            ILogger<S3FileStorageService> logger)
        {
            _s3Client = s3Client;
            _logger = logger;
            _bucketName = configuration["AWS:S3:BucketName"]!;
        }
        
        public async Task<string> UploadFileAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var key = GenerateFileKey(fileName);
                
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = fileStream,
                    Key = key,
                    BucketName = _bucketName,
                    ContentType = contentType,
                    CannedACL = S3CannedACL.Private
                };
                
                var transferUtility = new TransferUtility(_s3Client);
                await transferUtility.UploadAsync(uploadRequest, cancellationToken);
                
                var url = $"https://{_bucketName}.s3.amazonaws.com/{key}";
                
                _logger.LogInformation("File uploaded: {FileName} to {Url}", fileName, url);
                
                return url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file {FileName}", fileName);
                throw;
            }
        }
        
        public async Task<Stream> DownloadFileAsync(
            string fileUrl,
            CancellationToken cancellationToken = default)
        {
            var key = ExtractKeyFromUrl(fileUrl);
            
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };
            
            var response = await _s3Client.GetObjectAsync(request, cancellationToken);
            return response.ResponseStream;
        }
        
        public async Task<bool> DeleteFileAsync(
            string fileUrl,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var key = ExtractKeyFromUrl(fileUrl);
                
                var request = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };
                
                await _s3Client.DeleteObjectAsync(request, cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FileUrl}", fileUrl);
                return false;
            }
        }
        
        public async Task<string> GetPresignedUrlAsync(
            string fileUrl,
            int expirationMinutes = 60,
            CancellationToken cancellationToken = default)
        {
            var key = ExtractKeyFromUrl(fileUrl);
            
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = key,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };
            
            return _s3Client.GetPreSignedURL(request);
        }
        
        private string GenerateFileKey(string fileName)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
            var guid = Guid.NewGuid().ToString("N");
            var extension = Path.GetExtension(fileName);
            return $"{timestamp}/{guid}{extension}";
        }
        
        private string ExtractKeyFromUrl(string fileUrl)
        {
            var uri = new Uri(fileUrl);
            return uri.AbsolutePath.TrimStart('/');
        }
    }
}
```

---

## Caching Infrastructure

### Redis Cache Service

```csharp
// File: Infrastructure/Services/Cache/RedisCacheService.cs
using System.Text.Json;
using B2BCommerce.Backend.Application.Interfaces.ExternalServices;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Infrastructure.Services.Cache
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        
        public RedisCacheService(
            IDistributedCache cache,
            ILogger<RedisCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }
        
        public async Task<T?> GetAsync<T>(
            string key,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var value = await _cache.GetStringAsync(key, cancellationToken);
                
                if (string.IsNullOrEmpty(value))
                {
                    _logger.LogDebug("Cache miss for key: {Key}", key);
                    return default;
                }
                
                _logger.LogDebug("Cache hit for key: {Key}", key);
                return JsonSerializer.Deserialize<T>(value, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cache for key: {Key}", key);
                return default;
            }
        }
        
        public async Task SetAsync<T>(
            string key,
            T value,
            TimeSpan? expiration = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var serialized = JsonSerializer.Serialize(value, _jsonOptions);
                
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(1)
                };
                
                await _cache.SetStringAsync(key, serialized, options, cancellationToken);
                
                _logger.LogDebug("Cache set for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for key: {Key}", key);
            }
        }
        
        public async Task RemoveAsync(
            string key,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _cache.RemoveAsync(key, cancellationToken);
                _logger.LogDebug("Cache removed for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache for key: {Key}", key);
            }
        }
        
        public async Task<T> GetOrSetAsync<T>(
            string key,
            Func<Task<T>> factory,
            TimeSpan? expiration = null,
            CancellationToken cancellationToken = default)
        {
            var cachedValue = await GetAsync<T>(key, cancellationToken);
            
            if (cachedValue != null)
            {
                return cachedValue;
            }
            
            var value = await factory();
            await SetAsync(key, value, expiration, cancellationToken);
            
            return value;
        }
    }
}
```

### Cache Strategy

| Data Type            | Expiration | Strategy      |
| -------------------- | ---------- | ------------- |
| Product Catalog      | 1 hour     | Cache-aside   |
| Customer Data        | 30 minutes | Cache-aside   |
| Prices               | 15 minutes | Cache-aside   |
| System Configuration | 24 hours   | Write-through |
| Session Data         | 2 hours    | Redis native  |

---

## File Storage Infrastructure

### Storage Strategy

1. **Product Images**: AWS S3 with CloudFront CDN
2. **Documents**: AWS S3 with presigned URLs
3. **Temporary Files**: Local storage, cleaned daily
4. **Backups**: S3 with lifecycle policies

### File Organization

```
/product-images/{yyyyMMdd}/{guid}.{ext}
/documents/invoices/{yyyyMMdd}/{guid}.pdf
/documents/contracts/{yyyyMMdd}/{guid}.pdf
/documents/reports/{yyyyMMdd}/{guid}.pdf
```

---

## Background Jobs Infrastructure

### Hangfire Configuration

```csharp
// File: Infrastructure/BackgroundJobs/HangfireConfiguration.cs
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace B2BCommerce.Backend.Infrastructure.BackgroundJobs
{
    public static class HangfireConfiguration
    {
        public static IServiceCollection AddHangfireJobs(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Add Hangfire services
            services.AddHangfire(config =>
            {
                config.UsePostgreSqlStorage(
                    configuration.GetConnectionString("DefaultConnection"));
                config.UseSimpleAssemblyNameTypeSerializer();
                config.UseRecommendedSerializerSettings();
            });
            
            // Add Hangfire server
            services.AddHangfireServer(options =>
            {
                options.WorkerCount = 5;
            });
            
            return services;
        }
        
        public static void ConfigureRecurringJobs()
        {
            // Order synchronization with ERP - every 15 minutes
            RecurringJob.AddOrUpdate<OrderSyncJob>(
                "order-sync",
                job => job.SyncPendingOrdersAsync(),
                "*/15 * * * *");
            
            // Stock update from ERP - every 30 minutes
            RecurringJob.AddOrUpdate<StockUpdateJob>(
                "stock-update",
                job => job.UpdateStockLevelsAsync(),
                "*/30 * * * *");
            
            // Price update from ERP - every hour
            RecurringJob.AddOrUpdate<PriceUpdateJob>(
                "price-update",
                job => job.UpdatePricesAsync(),
                "0 * * * *");
            
            // Send email notifications - every 5 minutes
            RecurringJob.AddOrUpdate<EmailNotificationJob>(
                "email-notifications",
                job => job.ProcessPendingEmailsAsync(),
                "*/5 * * * *");
        }
    }
}
```

### Example Job Implementation

```csharp
// File: Infrastructure/BackgroundJobs/OrderSyncJob.cs
using B2BCommerce.Backend.Application.Interfaces;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Infrastructure.BackgroundJobs
{
    public class OrderSyncJob
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderSyncJob> _logger;
        
        public OrderSyncJob(
            IOrderService orderService,
            ILogger<OrderSyncJob> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }
        
        [AutomaticRetry(Attempts = 3)]
        public async Task SyncPendingOrdersAsync()
        {
            try
            {
                _logger.LogInformation("Starting order sync job");
                
                // Implementation for syncing pending orders with ERP
                // await _orderService.SyncPendingOrdersWithErpAsync();
                
                _logger.LogInformation("Order sync job completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in order sync job");
                throw;
            }
        }
    }
}
```

---

## Logging Infrastructure

### Serilog Configuration

```csharp
// File: Infrastructure/Logging/LoggingConfiguration.cs
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Configuration;

namespace B2BCommerce.Backend.Infrastructure.Logging
{
    public static class LoggingConfiguration
    {
        public static ILogger CreateLogger(IConfiguration configuration)
        {
            return new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "B2B-ECommerce")
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.PostgreSQL(
                    connectionString: configuration.GetConnectionString("DefaultConnection"),
                    tableName: "Logs",
                    restrictedToMinimumLevel: LogEventLevel.Warning)
                .CreateLogger();
        }
    }
}
```

### Log Levels Usage

- **Verbose**: Detailed debugging information
- **Debug**: Internal system events
- **Information**: General application events
- **Warning**: Abnormal but recoverable events
- **Error**: Errors and exceptions
- **Fatal**: Critical errors causing shutdown

---

## Dependency Injection Configuration

### Main DI Configuration

```csharp
// File: Infrastructure/DependencyInjection.cs
using Amazon.S3;
using B2BCommerce.Backend.Application.Interfaces;
using B2BCommerce.Backend.Application.Interfaces.ExternalServices;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Infrastructure.Data;
using B2BCommerce.Backend.Infrastructure.ExternalApis.Payment;
using B2BCommerce.Backend.Infrastructure.Repositories;
using B2BCommerce.Backend.Infrastructure.Services.Cache;
using B2BCommerce.Backend.Infrastructure.Services.Email;
using B2BCommerce.Backend.Infrastructure.Services.FileStorage;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace B2BCommerce.Backend.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
            
            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            // Repositories
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IBrandRepository, BrandRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IShipmentRepository, ShipmentRepository>();
            
            // External API Clients
            services.AddHttpClient<IPaymentGatewayService, PaynetClient>();
            
            // File Storage
            services.AddAWSService<IAmazonS3>();
            services.AddScoped<IFileStorageService, S3FileStorageService>();
            
            // Caching
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis");
                options.InstanceName = "B2B_";
            });
            services.AddScoped<ICacheService, RedisCacheService>();
            
            // Email Service
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();
            
            // Background Jobs (Hangfire)
            services.AddHangfire(config =>
                config.UsePostgreSqlStorage(
                    configuration.GetConnectionString("DefaultConnection")));
            services.AddHangfireServer();
            
            return services;
        }
    }
}
```

### Service Lifetimes

| Service Type         | Lifetime        | Reason                |
| -------------------- | --------------- | --------------------- |
| DbContext            | Scoped          | Per HTTP request      |
| Repositories         | Scoped          | Tied to DbContext     |
| Unit of Work         | Scoped          | Tied to DbContext     |
| Application Services | Scoped          | Per request state     |
| Cache Service        | Scoped          | Connection pooling    |
| HTTP Clients         | Transient/Typed | Connection management |
| Background Jobs      | Transient       | New instance per job  |
| Logging              | Singleton       | Shared across app     |

---

## Configuration Management

### appsettings.json Structure

```json
{
    "ConnectionStrings": {
        "DefaultConnection": "Host=localhost;Database=b2b_db;Username=postgres;Password=***",
        "Redis": "localhost:6379"
    },
    "Paynet": {
        "BaseUrl": "https://api.paynet.com",
        "MerchantId": "***",
        "MerchantKey": "***"
    },
    "AWS": {
        "Region": "eu-west-1",
        "S3": {
            "BucketName": "b2b-commerce-storage"
        }
    },
    "Email": {
        "SmtpHost": "smtp.gmail.com",
        "SmtpPort": 587,
        "Username": "***",
        "Password": "***",
        "FromEmail": "noreply@b2bcommerce.com",
        "FromName": "B2B Commerce"
    },
    "Serilog": {
        "MinimumLevel": {
            "Default": "Information",
            "Override": {
                "Microsoft": "Warning",
                "System": "Warning"
            }
        }
    }
}
```

---

## Performance Considerations

### Database Optimization

1. **Indexes**: Strategic indexes on frequently queried columns
2. **Query Optimization**: Use `AsNoTracking()` for read-only queries
3. **Eager Loading**: Use `Include()` to prevent N+1 queries
4. **Pagination**: Always paginate large result sets
5. **Connection Pooling**: Managed by EF Core

### Caching Strategy

1. **Cache frequently accessed data**: Product catalog, categories, brands
2. **Short TTL for dynamic data**: Prices, stock levels
3. **Cache invalidation**: On updates and deletes
4. **Distributed caching**: Redis for multi-instance scenarios

### External API Resilience

1. **Retry policies**: Exponential backoff with Polly
2. **Circuit breakers**: Fail fast when service is down
3. **Timeouts**: Reasonable timeout values
4. **Fallback strategies**: Graceful degradation

---

## Security Considerations

### Database Security

1. **Parameterized queries**: EF Core prevents SQL injection
2. **Encrypted connections**: SSL/TLS for database connections
3. **Least privilege**: Database users with minimal permissions
4. **Audit logging**: Track all data changes

### API Security

1. **API keys**: Secure storage in environment variables
2. **HTTPS only**: All external API calls over HTTPS
3. **Request signing**: For payment gateway integration
4. **Rate limiting**: Prevent abuse of external services

### File Storage Security

1. **Private buckets**: No public access by default
2. **Presigned URLs**: Temporary access with expiration
3. **Virus scanning**: Scan uploaded files
4. **Access logging**: S3 access logs enabled

---

## Monitoring & Observability

### Metrics to Track

1. **Database**: Query performance, connection pool usage
2. **Cache**: Hit rate, eviction rate
3. **External APIs**: Response time, error rate
4. **Background Jobs**: Success rate, execution time
5. **File Storage**: Upload/download rate, storage usage

### Logging Best Practices

1. **Structured logging**: Use Serilog with structured properties
2. **Correlation IDs**: Track requests across services
3. **Performance logging**: Log slow queries and operations
4. **Error context**: Include relevant context in error logs
5. **PII protection**: Never log sensitive data

---

## Testing Strategy

### Unit Testing Repositories

```csharp
// Example: ProductRepository Unit Test
public class ProductRepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly ProductRepository _repository;
    
    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        
        _context = new ApplicationDbContext(options);
        _repository = new ProductRepository(_context);
    }
    
    [Fact]
    public async Task GetBySkuAsync_ReturnsProduct_WhenSkuExists()
    {
        // Arrange
        var product = new Product { Sku = "TEST-001", Name = "Test Product" };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _repository.GetBySkuAsync("TEST-001");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("TEST-001", result.Sku);
    }
}
```

### Integration Testing

```csharp
// Example: External API Integration Test
public class PaynetClientTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    
    public PaynetClientTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task InitiatePayment_ReturnsSuccess_WhenValidRequest()
    {
        // Integration test with mocked Paynet API
    }
}
```

---

## Summary

This Infrastructure Layer specification provides:

1. **Complete Database Setup**: DbContext, entity configurations, migrations
2. **Repository Pattern**: Generic and specific repository implementations
3. **Unit of Work**: Transaction management across repositories
4. **External Integrations**: Payment gateway, email, file storage, caching
5. **Background Jobs**: Hangfire configuration for asynchronous tasks
6. **Logging**: Structured logging with Serilog
7. **Dependency Injection**: Centralized service registration
8. **Performance**: Optimization strategies and best practices
9. **Security**: Secure implementation of external integrations
10. **Testing**: Unit and integration testing approaches

All implementations follow Clean Architecture principles, maintain separation of
concerns, and support testability and maintainability.

---

**Document Version**: 1.0\
**Created**: November 2025\
**Target**: .NET 8 Development Team\
**Framework**: Clean Architecture, Entity Framework Core 8, PostgreSQL
