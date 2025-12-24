using B2BCommerce.Backend.Application.Interfaces.Services;
using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Domain.Entities.Integration;
using B2BCommerce.Backend.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace B2BCommerce.Backend.Infrastructure.Data;

/// <summary>
/// Application database context
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private readonly ICurrentUserService? _currentUserService;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    // Domain entities
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<CustomerAttribute> CustomerAttributes => Set<CustomerAttribute>();
    public DbSet<CustomerDocument> CustomerDocuments => Set<CustomerDocument>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<SystemConfiguration> SystemConfigurations => Set<SystemConfiguration>();
    public DbSet<CurrencyRate> CurrencyRates => Set<CurrencyRate>();
    public DbSet<NewsletterSubscription> NewsletterSubscriptions => Set<NewsletterSubscription>();

    // Product Attribute System entities
    public DbSet<AttributeDefinition> AttributeDefinitions => Set<AttributeDefinition>();
    public DbSet<AttributeValue> AttributeValues => Set<AttributeValue>();
    public DbSet<ProductType> ProductTypes => Set<ProductType>();
    public DbSet<ProductTypeAttribute> ProductTypeAttributes => Set<ProductTypeAttribute>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductAttributeValue> ProductAttributeValues => Set<ProductAttributeValue>();
    public DbSet<ProductRelation> ProductRelations => Set<ProductRelation>();

    // Integration API entities
    public DbSet<ApiClient> ApiClients => Set<ApiClient>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<ApiKeyPermission> ApiKeyPermissions => Set<ApiKeyPermission>();
    public DbSet<ApiKeyIpWhitelist> ApiKeyIpWhitelist => Set<ApiKeyIpWhitelist>();
    public DbSet<ApiKeyUsageLog> ApiKeyUsageLogs => Set<ApiKeyUsageLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configure Identity table names (remove AspNet prefix)
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
        });

        modelBuilder.Entity<ApplicationRole>(entity =>
        {
            entity.ToTable("Roles");
        });

        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>>(entity =>
        {
            entity.ToTable("UserRoles");
        });

        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<Guid>>(entity =>
        {
            entity.ToTable("UserClaims");
        });

        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<Guid>>(entity =>
        {
            entity.ToTable("UserLogins");
        });

        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>>(entity =>
        {
            entity.ToTable("RoleClaims");
        });

        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<Guid>>(entity =>
        {
            entity.ToTable("UserTokens");
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService?.UserId;

        // Update audit fields
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Domain.Common.BaseEntity &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (Domain.Common.BaseEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
                // Auto-set CreatedBy if not already set
                entity.CreatedBy ??= userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(Domain.Common.BaseEntity.CreatedAt)).IsModified = false;
                entry.Property(nameof(Domain.Common.BaseEntity.CreatedBy)).IsModified = false;
                entity.UpdatedAt = DateTime.UtcNow;
                // Auto-set UpdatedBy from current user
                entity.UpdatedBy = userId;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
