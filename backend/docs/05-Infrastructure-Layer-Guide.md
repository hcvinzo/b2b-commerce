# Infrastructure Layer Guide

## Overview

The Infrastructure layer implements data access, external services, and all technical concerns. It provides concrete implementations for interfaces defined in the Application layer.

## Namespace

```csharp
namespace B2BCommerce.Backend.Infrastructure.Data
namespace B2BCommerce.Backend.Infrastructure.Repositories
namespace B2BCommerce.Backend.Infrastructure.Services
namespace B2BCommerce.Backend.Infrastructure.Identity
namespace B2BCommerce.Backend.Infrastructure.ExternalApis
namespace B2BCommerce.Backend.Infrastructure.BackgroundJobs
```

## DbContext

### ApplicationDbContext

```csharp
// File: Infrastructure/Data/ApplicationDbContext.cs
namespace B2BCommerce.Backend.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
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
        
        // Customer Management
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
        
        // Order Management
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        
        // Payment Management
        public DbSet<Payment> Payments => Set<Payment>();
        
        // System Configuration
        public DbSet<SystemConfiguration> SystemConfigurations => Set<SystemConfiguration>();
        public DbSet<CurrencyRate> CurrencyRates => Set<CurrencyRate>();
        
        // Identity Extensions
        public DbSet<ApplicationAdminUser> AdminUsers => Set<ApplicationAdminUser>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        
        #endregion
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Apply all entity configurations from assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            
            // Global query filters for soft delete
            ApplySoftDeleteFilter(modelBuilder);
            
            // Configure decimal precision globally
            ConfigureDecimalPrecision(modelBuilder);
        }
        
        private void ApplySoftDeleteFilter(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                    var body = Expression.Equal(property, Expression.Constant(false));
                    var lambda = Expression.Lambda(body, parameter);
                    
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }
        
        private void ConfigureDecimalPrecision(ModelBuilder modelBuilder)
        {
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,2)");
            }
        }
        
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            HandleAuditFields();
            HandleSoftDeletes();
            
            return await base.SaveChangesAsync(cancellationToken);
        }
        
        private void HandleAuditFields()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();
            var now = DateTime.UtcNow;
            
            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.GetType().GetProperty("CreatedAt")
                            ?.SetValue(entry.Entity, now);
                        break;
                    case EntityState.Modified:
                        entry.Entity.GetType().GetProperty("UpdatedAt")
                            ?.SetValue(entry.Entity, now);
                        break;
                }
            }
        }
        
        private void HandleSoftDeletes()
        {
            var entries = ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Deleted);
            
            foreach (var entry in entries)
            {
                entry.State = EntityState.Modified;
                entry.Entity.Delete();
            }
        }
    }
}
```

## Entity Configurations

### Configuration Example

```csharp
// File: Infrastructure/Data/Configurations/ProductConfiguration.cs
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
            
            builder.Property(p => p.ExternalId)
                .HasMaxLength(100);
            
            // Product fields
            builder.Property(p => p.Sku)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);
            
            builder.Property(p => p.Description)
                .HasMaxLength(4000);
            
            builder.Property(p => p.Currency)
                .IsRequired()
                .HasMaxLength(3);
            
            // Indexes
            builder.HasIndex(p => p.Sku).IsUnique();
            builder.HasIndex(p => p.ExternalCode).IsUnique();
            builder.HasIndex(p => p.IsActive);
            builder.HasIndex(p => new { p.CategoryId, p.IsActive });
            
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
        }
    }
}

// File: Infrastructure/Data/Configurations/OrderConfiguration.cs
namespace B2BCommerce.Backend.Infrastructure.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");
            
            builder.HasKey(o => o.Id);
            
            builder.Property(o => o.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.Property(o => o.Currency)
                .IsRequired()
                .HasMaxLength(3);
            
            builder.Property(o => o.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
            
            // Indexes
            builder.HasIndex(o => o.OrderNumber).IsUnique();
            builder.HasIndex(o => o.CustomerId);
            builder.HasIndex(o => o.Status);
            builder.HasIndex(o => o.CreatedAt);
            
            // Relationships
            builder.HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
```

## Repository Implementations

### Generic Repository

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
        
        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }
        
        public virtual async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }
        
        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }
        
        public virtual void Update(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }
        
        public virtual void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }
        
        public virtual async Task<bool> ExistsAsync(int id)
        {
            return await _dbSet.AnyAsync(e => e.Id == id);
        }
    }
}
```

### Specific Repository

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
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.Sku == sku);
        }
        
        public async Task<Product?> GetByExternalCodeAsync(string externalCode)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.ExternalCode == externalCode);
        }
        
        public async Task<Product?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images.OrderBy(i => i.SortOrder))
                .Include(p => p.AttributeValues)
                    .ThenInclude(av => av.Attribute)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        
        public async Task<PaginatedList<Product>> GetPagedAsync(ProductQueryDto query)
        {
            var queryable = _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .AsQueryable();
            
            // Apply filters
            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var term = query.SearchTerm.ToLower();
                queryable = queryable.Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    p.Sku.ToLower().Contains(term));
            }
            
            if (query.CategoryId.HasValue)
            {
                queryable = queryable.Where(p => p.CategoryId == query.CategoryId);
            }
            
            if (query.BrandId.HasValue)
            {
                queryable = queryable.Where(p => p.BrandId == query.BrandId);
            }
            
            if (query.IsActive.HasValue)
            {
                queryable = queryable.Where(p => p.IsActive == query.IsActive);
            }
            
            if (query.MinPrice.HasValue)
            {
                queryable = queryable.Where(p => p.ListPrice >= query.MinPrice);
            }
            
            if (query.MaxPrice.HasValue)
            {
                queryable = queryable.Where(p => p.ListPrice <= query.MaxPrice);
            }
            
            // Apply sorting
            queryable = query.SortBy?.ToLower() switch
            {
                "name" => query.SortDescending 
                    ? queryable.OrderByDescending(p => p.Name)
                    : queryable.OrderBy(p => p.Name),
                "price" => query.SortDescending
                    ? queryable.OrderByDescending(p => p.ListPrice)
                    : queryable.OrderBy(p => p.ListPrice),
                "created" => query.SortDescending
                    ? queryable.OrderByDescending(p => p.CreatedAt)
                    : queryable.OrderBy(p => p.CreatedAt),
                _ => queryable.OrderByDescending(p => p.CreatedAt)
            };
            
            return await PaginatedList<Product>.CreateAsync(
                queryable, query.PageNumber, query.PageSize);
        }
        
        public async Task<bool> SkuExistsAsync(string sku, int? excludeId = null)
        {
            var query = _dbSet.Where(p => p.Sku == sku);
            
            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }
            
            return await query.AnyAsync();
        }
    }
}
```

## Unit of Work

```csharp
// File: Infrastructure/Data/UnitOfWork.cs
namespace B2BCommerce.Backend.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;
        
        // Lazy-loaded repositories
        private IProductRepository? _products;
        private ICategoryRepository? _categories;
        private IBrandRepository? _brands;
        private ICustomerRepository? _customers;
        private IOrderRepository? _orders;
        private IPaymentRepository? _payments;
        
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public IProductRepository Products =>
            _products ??= new ProductRepository(_context);
        
        public ICategoryRepository Categories =>
            _categories ??= new CategoryRepository(_context);
        
        public IBrandRepository Brands =>
            _brands ??= new BrandRepository(_context);
        
        public ICustomerRepository Customers =>
            _customers ??= new CustomerRepository(_context);
        
        public IOrderRepository Orders =>
            _orders ??= new OrderRepository(_context);
        
        public IPaymentRepository Payments =>
            _payments ??= new PaymentRepository(_context);
        
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }
        
        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        
        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        
        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
```

## External Services

### Token Service

```csharp
// File: Infrastructure/Services/TokenService.cs
namespace B2BCommerce.Backend.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        
        public TokenService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }
        
        public string GenerateAccessToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName!),
                new(ClaimTypes.Email, user.Email!),
                new("CustomerId", user.CustomerId.ToString()),
                new("FullName", user.FullName)
            };
            
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                signingCredentials: credentials);
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        
        public ClaimsPrincipal? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            
            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = false // Allow expired tokens for refresh
                }, out _);
                
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
```

### Cache Service

```csharp
// File: Infrastructure/Services/CacheService.cs
namespace B2BCommerce.Backend.Infrastructure.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheService> _logger;
        
        public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }
        
        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _cache.GetStringAsync(key);
            
            if (string.IsNullOrEmpty(value))
                return default;
            
            return JsonSerializer.Deserialize<T>(value);
        }
        
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30)
            };
            
            var json = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, json, options);
        }
        
        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }
        
        public async Task<T> GetOrSetAsync<T>(
            string key, 
            Func<Task<T>> factory, 
            TimeSpan? expiration = null)
        {
            var cached = await GetAsync<T>(key);
            
            if (cached != null)
                return cached;
            
            var value = await factory();
            await SetAsync(key, value, expiration);
            
            return value;
        }
    }
}
```

### Email Service

```csharp
// File: Infrastructure/Services/EmailService.cs
namespace B2BCommerce.Backend.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;
        
        public EmailService(
            IOptions<EmailSettings> settings, 
            ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }
        
        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort);
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
                
                var message = new MailMessage
                {
                    From = new MailAddress(_settings.FromEmail, _settings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };
                message.To.Add(to);
                
                await client.SendMailAsync(message);
                
                _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                throw;
            }
        }
        
        public async Task SendOrderConfirmationAsync(Order order)
        {
            var body = $@"
                <h2>Order Confirmation</h2>
                <p>Order Number: {order.OrderNumber}</p>
                <p>Total: {order.TotalAmount:C}</p>
                <p>Thank you for your order!</p>";
            
            await SendEmailAsync(
                order.Customer.PrimaryEmail.Value,
                $"Order Confirmation - {order.OrderNumber}",
                body);
        }
    }
}
```

## Dependency Injection

```csharp
// File: Infrastructure/DependencyInjection.cs
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
            
            // Identity
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
            
            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            // Repositories (registered through UnitOfWork)
            
            // Services
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ICacheService, CacheService>();
            
            // Redis
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis");
                options.InstanceName = "B2BCommerce_";
            });
            
            // HTTP Clients
            services.AddHttpClient<IPaymentGatewayClient, PaynetClient>(client =>
            {
                client.BaseAddress = new Uri(configuration["Paynet:BaseUrl"]!);
            })
            .AddTransientHttpErrorPolicy(builder => 
                builder.WaitAndRetryAsync(3, retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
            
            // Background Jobs
            services.AddHangfire(config =>
                config.UsePostgreSqlStorage(
                    configuration.GetConnectionString("DefaultConnection")));
            services.AddHangfireServer();
            
            return services;
        }
    }
}
```

## Configuration Classes

```csharp
// File: Infrastructure/Settings/JwtSettings.cs
namespace B2BCommerce.Backend.Infrastructure.Settings
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public int AccessTokenExpirationMinutes { get; set; } = 15;
        public int RefreshTokenExpirationDays { get; set; } = 7;
    }
}

// File: Infrastructure/Settings/EmailSettings.cs
namespace B2BCommerce.Backend.Infrastructure.Settings
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = null!;
        public int SmtpPort { get; set; } = 587;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FromEmail { get; set; } = null!;
        public string FromName { get; set; } = null!;
    }
}
```

## Query Optimization Guidelines

### Use AsNoTracking for Read-Only Queries

```csharp
// ✅ GOOD: No tracking for read-only
public async Task<List<ProductDto>> GetAllAsync()
{
    return await _dbSet
        .AsNoTracking()
        .Select(p => new ProductDto { Id = p.Id, Name = p.Name })
        .ToListAsync();
}
```

### Use Projection Instead of Full Entity Load

```csharp
// ✅ GOOD: Only select needed columns
public async Task<List<ProductListItemDto>> GetListAsync()
{
    return await _dbSet
        .AsNoTracking()
        .Select(p => new ProductListItemDto
        {
            Id = p.Id,
            Sku = p.Sku,
            Name = p.Name,
            ListPrice = p.ListPrice
        })
        .ToListAsync();
}

// ❌ BAD: Loading entire entity then mapping
public async Task<List<ProductListItemDto>> GetListAsync()
{
    var products = await _dbSet.ToListAsync();
    return _mapper.Map<List<ProductListItemDto>>(products);
}
```

### Use Include Explicitly

```csharp
// ✅ GOOD: Explicit includes
public async Task<Product?> GetByIdWithDetailsAsync(int id)
{
    return await _dbSet
        .Include(p => p.Category)
        .Include(p => p.Brand)
        .FirstOrDefaultAsync(p => p.Id == id);
}
```

## Rules Summary

| Rule | Description |
|------|-------------|
| Implement Application interfaces | All services implement interfaces from Application |
| No business logic | Only technical implementation |
| Use Entity Configurations | Define mappings in separate configuration classes |
| Use repository pattern | Abstract data access |
| Register in DI container | Centralized in DependencyInjection.cs |
| Use AsNoTracking | For read-only queries |
| Use projections | Select only needed columns |

---

**Next**: [06-API-Layer-Guide](06-API-Layer-Guide.md)
