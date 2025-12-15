# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

B2B E-Commerce Platform following Clean Architecture principles. A .NET 10 backend API with PostgreSQL (Supabase) for dealer portal, admin panel, and ERP integrations.

## Build & Run Commands

### Backend
```bash
# Build entire solution (from backend/)
cd backend && dotnet build

# Run API with hot reload
dotnet run --project backend/src/B2BCommerce.Backend.API

# Run all tests
cd backend && dotnet test

# Run specific test project
dotnet test backend/tests/B2BCommerce.Backend.Domain.Tests

# Run single test by name
dotnet test backend/tests/B2BCommerce.Backend.Domain.Tests --filter "FullyQualifiedName~TestMethodName"

# EF Core migrations (from backend/)
dotnet ef migrations add <MigrationName> --project src/B2BCommerce.Backend.Infrastructure --startup-project src/B2BCommerce.Backend.API

# Apply migrations
ASPNETCORE_ENVIRONMENT=Development dotnet ef database update --project src/B2BCommerce.Backend.Infrastructure --startup-project src/B2BCommerce.Backend.API
```

### Frontend
```bash
# Development
cd frontend && npm run dev

# Build for production
npm run build

# Build for test environment (uses .env.test)
npm run build:test

# Output: frontend/out/ (static export)
```

## Architecture

**Clean Architecture with 4 layers** - dependencies flow inward toward Domain:

```
API → Infrastructure → Application → Domain
```

| Layer | Project | Responsibility |
|-------|---------|----------------|
| **Domain** | `B2BCommerce.Backend.Domain` | Entities, Value Objects, Domain Services, Domain Events, Exceptions. Pure C#, NO external dependencies |
| **Application** | `B2BCommerce.Backend.Application` | DTOs, Service Interfaces, Repository Interfaces, CQRS Handlers, Validators, Domain Event Handlers. Depends only on Domain |
| **Infrastructure** | `B2BCommerce.Backend.Infrastructure` | EF Core DbContext, Repository implementations, Service implementations, Identity. Implements Application interfaces |
| **API** | `B2BCommerce.Backend.API` | Controllers, Middleware, Auth config. Thin layer, delegates via MediatR |
| **IntegrationAPI** | `B2BCommerce.Backend.IntegrationAPI` | External API for ERP integrations. API Key auth, delegates via MediatR |

---

## Domain Layer Rules

### Base Classes

**BaseEntity** - All entities inherit from this:
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }

    // Audit fields - PROTECTED setters (set by DbContext)
    public DateTime CreatedAt { get; protected set; }
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

    public void SoftDelete(string? deletedBy = null) { ... }
    public void Restore() { ... }
}
```

**ExternalEntity** - For entities synced from external systems (ERP):
```csharp
public abstract class ExternalEntity : BaseEntity, IExternalEntity
{
    public string? ExternalCode { get; protected set; }  // Code in external system
    public string? ExternalId { get; protected set; }    // ID in external system
    public DateTime? LastSyncedAt { get; protected set; }

    public void MarkAsSynced() => LastSyncedAt = DateTime.UtcNow;

    // Factory method pattern for external creation
    // Child entities implement: static T CreateFromExternal(string externalCode, ...)
}
```

**Entities extending ExternalEntity**: `Product`, `Category`, `Brand`, `Customer`

### Entity Rules

1. **Private constructors** - Never use `new Entity()` directly
2. **Factory methods** - Use `Entity.Create(...)` or `Entity.CreateFromExternal(...)`
3. **Protected setters** - All properties have `{ get; protected set; }`
4. **Domain events** - Raise events in factory methods: `AddDomainEvent(new ProductCreatedEvent(this))`
5. **Behavior methods** - Encapsulate state changes: `order.AddItem(...)`, `customer.UpdateCreditLimit(...)`

```csharp
public class Product : ExternalEntity, IAggregateRoot
{
    // Private constructor
    private Product() { }

    // Factory method for internal creation
    public static Product Create(string sku, string name, Guid categoryId, ...)
    {
        var product = new Product { ... };
        product.AddDomainEvent(new ProductCreatedEvent(product));
        return product;
    }

    // Factory method for external system sync
    public static Product CreateFromExternal(string externalCode, string sku, string name, ...)
    {
        var product = Create(sku, name, ...);
        product.ExternalCode = externalCode;
        product.MarkAsSynced();
        return product;
    }

    // Behavior methods
    public void UpdatePrice(Money newPrice) { ... }
    public void Deactivate() { ... }
}
```

### Marker Interfaces

- **IAggregateRoot** - Marks aggregate roots (only these should have repositories)
- **IExternalEntity** - Marks entities synced from external systems

### Value Objects

Immutable objects with value equality. Use for concepts with no identity:

```csharp
// Money - amount + currency with arithmetic
public record Money(decimal Amount, string Currency = "TRY")
{
    public static Money operator +(Money a, Money b) => ...
    public static Money operator *(Money m, decimal multiplier) => ...
}

// Email - validated wrapper
public record Email
{
    public string Value { get; }
    public Email(string value)
    {
        if (!IsValid(value)) throw new DomainException("Invalid email");
        Value = value;
    }
}

// Others: PhoneNumber, TaxNumber, Address
```

### Domain Events

Events raised when significant domain actions occur:

```csharp
// Interface
public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}

// Events
public record ProductCreatedEvent(Product Product) : IDomainEvent { ... }
public record OrderPlacedEvent(Order Order) : IDomainEvent { ... }
public record CustomerCreditLimitChangedEvent(Customer Customer, decimal OldLimit, decimal NewLimit) : IDomainEvent { ... }
```

### Domain Exceptions

Never use `System.InvalidOperationException` in Domain layer:

```csharp
// Base exception
public class DomainException : Exception { ... }

// Specific exceptions
public class InvalidOperationDomainException : DomainException { ... }
public class InsufficientCreditException : DomainException { ... }
public class InsufficientStockException : DomainException { ... }
public class OutOfStockException : DomainException { ... }
```

### Domain Services

For logic spanning multiple aggregates:

```csharp
public interface IPricingService
{
    Money CalculatePrice(Product product, Customer customer, int quantity);
}

public interface ICreditManagementService
{
    bool HasSufficientCredit(Customer customer, Money amount);
    void ReserveCredit(Customer customer, Order order);
    void ReleaseCredit(Customer customer, Order order);
}
```

---

## Application Layer Rules

### Repository Interfaces

```csharp
public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
}

// Specific repositories for aggregate roots only
public interface IProductRepository : IGenericRepository<Product>
{
    Task<Product?> GetBySKUAsync(string sku, CancellationToken ct = default);
    Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken ct = default);
}
```

### Unit of Work

```csharp
public interface IUnitOfWork : IDisposable
{
    // Repositories
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
    ICustomerRepository Customers { get; }
    IOrderRepository Orders { get; }

    // Transaction management
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}
```

### DTOs

Naming convention: `{Entity}Dto`, `Create{Entity}Dto`, `Update{Entity}Dto`

```csharp
public record ProductDto(Guid Id, string Sku, string Name, decimal Price, ...);
public record CreateProductDto(string Sku, string Name, Guid CategoryId, ...);
public record UpdateProductDto(string Name, string? Description, decimal Price, ...);
```

### Result Pattern

Explicit success/failure handling:

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? ErrorMessage { get; }
    public IEnumerable<string> Errors { get; }

    public static Result<T> Ok(T data) => new(true, data, null, null);
    public static Result<T> Fail(string error) => new(false, default, error, null);
    public static Result<T> Fail(IEnumerable<string> errors) => new(false, default, null, errors);
}
```

### CQRS with MediatR

**Commands** (write operations):
```csharp
public interface ICommand<TResponse> : IRequest<TResponse> { }
public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse> { }

public record CreateProductCommand(string Sku, string Name, ...) : ICommand<Result<ProductDto>>;
```

**Queries** (read operations):
```csharp
public interface IQuery<TResponse> : IRequest<TResponse> { }
public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse> { }

public record GetProductByIdQuery(Guid Id) : IQuery<Result<ProductDto>>;
```

### FluentValidation

Every command MUST have a validator:

```csharp
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU is required")
            .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters")
            .MustAsync(async (sku, ct) => !await unitOfWork.Products.ExistsBySkuAsync(sku, ct))
            .WithMessage("SKU already exists");

        RuleFor(x => x.CategoryId)
            .MustAsync(async (id, ct) => await unitOfWork.Categories.ExistsAsync(id, ct))
            .WithMessage("Category not found");
    }
}
```

### Domain Event Handlers

Handle domain events in Application layer:

```csharp
public class ProductCreatedEventHandler : INotificationHandler<ProductCreatedEvent>
{
    public async Task Handle(ProductCreatedEvent notification, CancellationToken ct)
    {
        // Send notification, update cache, etc.
    }
}
```

### Application Exceptions

```csharp
public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.") { }
}

public class ValidationException : Exception
{
    public IEnumerable<string> Errors { get; }
}
```

---

## Infrastructure Layer Rules

### DbContext

```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    // ...

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // Auto-set audit fields
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        // Dispatch domain events
        var domainEntities = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .ToList();
        // ... dispatch and clear events

        return await base.SaveChangesAsync(ct);
    }
}
```

### Entity Configurations

Each entity has `IEntityTypeConfiguration<T>`:

```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);

        // Global soft delete filter - REQUIRED
        builder.HasQueryFilter(p => !p.IsDeleted);

        // Value objects
        builder.OwnsOne(p => p.ListPrice, money => {
            money.Property(m => m.Amount).HasColumnName("ListPriceAmount");
            money.Property(m => m.Currency).HasColumnName("ListPriceCurrency");
        });

        // Value object conversions
        builder.Property(p => p.Sku)
            .HasMaxLength(50)
            .IsRequired();
    }
}
```

### Repositories

```csharp
public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    // Read operations use AsNoTracking()
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    // For update operations, get tracked entity
    public async Task<T?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id, ct);
    }
}
```

### Services

Services implement Application interfaces:

```csharp
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public async Task<Result<ProductDto>> CreateAsync(CreateProductDto dto, CancellationToken ct)
    {
        // Use factory method - NEVER new Product()
        var product = Product.Create(dto.Sku, dto.Name, dto.CategoryId, ...);

        await _unitOfWork.Products.AddAsync(product, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<ProductDto>.Ok(_mapper.Map<ProductDto>(product));
    }
}
```

---

## API Layer Rules

### Controllers

Thin controllers - delegate to MediatR:

```csharp
[ApiController]
[Route("api/v1/[controller]")]  // ALWAYS use api/v1/ prefix
[Authorize]
public class ProductsController : BaseApiController
{
    private readonly IMediator _mediator;

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id));

        if (!result.IsSuccess)
            return NotFoundResponse(result.ErrorMessage);

        return OkResponse(result.Data);
    }
}
```

### Middleware

**Required middleware** (in order):
1. `ExceptionHandlingMiddleware` - Global exception handling
2. `RequestLoggingMiddleware` - Request/response logging
3. Authentication/Authorization

```csharp
// Program.cs
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
```

### Authentication

**Main API**: JWT Bearer tokens
```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { ... });
```

**Integration API**: API Key authentication
```csharp
services.AddAuthentication("ApiKey")
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>("ApiKey", null);
```

### Response Format

Consistent response wrapper:

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public IEnumerable<string>? Errors { get; set; }
}
```

---

## Coding Standards

### Null Handling

```csharp
// ✅ GOOD - Pattern matching
if (product is null) return NotFound();
if (customer is not null) DoSomething();

// ❌ BAD - Equality operators
if (product == null) return NotFound();
if (customer != null) DoSomething();

// ⚠️ EXCEPTION - Expression trees (AutoMapper, EF LINQ)
.ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name != null ? s.Name : ""))
.Where(p => p.Category != null && p.Category.IsActive)
```

### Async/Await

```csharp
// ✅ All async methods suffixed with Async
public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct)

// ✅ Always use async/await
return await _repository.GetByIdAsync(id, ct);

// ❌ NEVER use .Result or .Wait()
return _repository.GetByIdAsync(id).Result;  // Deadlock risk!
```

### Exception Handling

```csharp
// ✅ GOOD - Specific exceptions
catch (DbUpdateConcurrencyException ex)
{
    _logger.LogWarning(ex, "Concurrency conflict for {EntityId}", id);
    throw new ConflictException("Record was modified");
}

// ❌ BAD - Generic exception
catch (Exception ex)
{
    _logger.LogError(ex, "Error");
    throw;
}
```

### Logging

```csharp
// ✅ Structured logging
_logger.LogInformation("Product created: {ProductId} - {Sku}", product.Id, product.Sku);

// ❌ String interpolation
_logger.LogInformation($"Product created: {product.Id} - {product.Sku}");
```

### Braces

```csharp
// ✅ GOOD - Always use braces
if (product is null)
{
    return NotFound();
}

// ❌ BAD - No braces
if (product is null)
    return NotFound();
```

---

## Project Structure

```
backend/src/
├── B2BCommerce.Backend.Domain/
│   ├── Common/              # BaseEntity, ExternalEntity, IAggregateRoot
│   ├── Entities/            # Product, Category, Order, etc.
│   ├── ValueObjects/        # Money, Address, Email, etc.
│   ├── Enums/               # OrderStatus, PaymentStatus, etc.
│   ├── Events/              # IDomainEvent, ProductCreatedEvent, etc.
│   ├── DomainServices/      # IPricingService, ICreditManagementService
│   └── Exceptions/          # DomainException, InvalidOperationDomainException
│
├── B2BCommerce.Backend.Application/
│   ├── Common/              # Result<T>, PagedResult<T>, ICommand, IQuery
│   ├── DTOs/                # CategoryDto, ProductDto, etc.
│   ├── Exceptions/          # NotFoundException, ValidationException
│   ├── Features/            # CQRS Commands & Queries by feature
│   │   └── Categories/
│   │       ├── Commands/    # CreateCategoryCommand, UpdateCategoryCommand
│   │       ├── Queries/     # GetCategoriesQuery, GetCategoryByIdQuery
│   │       └── Events/      # CategoryCreatedEventHandler
│   ├── Interfaces/
│   │   ├── Repositories/    # IGenericRepository, IProductRepository
│   │   └── Services/        # ICategoryService, IProductService
│   ├── Mappings/            # AutoMapper profiles
│   └── Validators/          # FluentValidation validators
│
├── B2BCommerce.Backend.Infrastructure/
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   ├── Configurations/  # EF Core entity configurations
│   │   ├── Repositories/    # Repository implementations
│   │   └── Migrations/
│   ├── Services/            # Service implementations
│   └── Identity/            # ApplicationUser, ApplicationRole
│
├── B2BCommerce.Backend.API/
│   ├── Controllers/         # MediatR-based controllers
│   ├── Middleware/          # ExceptionHandling, RequestLogging
│   └── Program.cs
│
└── B2BCommerce.Backend.IntegrationAPI/
    ├── Controllers/         # MediatR-based controllers
    ├── Authentication/      # ApiKeyAuthenticationHandler
    └── Program.cs
```

---

## Quick Reference

| Rule | Do | Don't |
|------|-----|-------|
| Entity creation | `Product.Create(...)` | `new Product(...)` |
| Null checks | `is null` / `is not null` | `== null` / `!= null` |
| Domain exceptions | `throw new InvalidOperationDomainException(...)` | `throw new InvalidOperationException(...)` |
| Controllers | Delegate to MediatR | Access DbContext directly |
| Async methods | Suffix with `Async` | Omit suffix |
| Repositories | Only for aggregate roots | For every entity |
| Soft delete | `HasQueryFilter(e => !e.IsDeleted)` | Manual filtering |
| Read queries | `AsNoTracking()` | Track read-only entities |

---

## Documentation

Detailed specifications in `backend/docs/`:
- `00-README.md` - Documentation index
- `01-Solution-Structure.md` - Project organization
- `02-Architecture-Guide.md` - Clean Architecture principles
- `03-Domain-Layer-Guide.md` - Entities, value objects, events
- `04-Application-Layer-Guide.md` - DTOs, CQRS, validation
- `05-Infrastructure-Layer-Guide.md` - DbContext, repositories
- `06-API-Layer-Guide.md` - Controllers, authentication
- `07-Coding-Standards.md` - Naming, formatting, best practices
- `08-Testing-Guide.md` - Test structure and patterns
