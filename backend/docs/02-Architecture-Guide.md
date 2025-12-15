# Architecture Guide

## Overview

B2BCommerce follows **Clean Architecture** (also known as Onion Architecture or Hexagonal Architecture) principles. This guide explains the core concepts and rules developers must follow.

## Core Principles

### 1. The Dependency Rule

**Dependencies must point inward toward the domain.**

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│              (Controllers, Middleware, Filters)              │
│                      Dependencies: ↓                         │
└─────────────────────────────┬───────────────────────────────┘
                              │
┌─────────────────────────────▼───────────────────────────────┐
│                   Infrastructure Layer                       │
│            (EF Core, Repositories, External APIs)            │
│                      Dependencies: ↓                         │
└─────────────────────────────┬───────────────────────────────┘
                              │
┌─────────────────────────────▼───────────────────────────────┐
│                    Application Layer                         │
│          (Services, DTOs, Interfaces, Validators)            │
│                      Dependencies: ↓                         │
└─────────────────────────────┬───────────────────────────────┘
                              │
┌─────────────────────────────▼───────────────────────────────┐
│                      Domain Layer                            │
│          (Entities, Value Objects, Domain Events)            │
│                     NO DEPENDENCIES                          │
└─────────────────────────────────────────────────────────────┘
```

### 2. Layer Isolation

| Principle | Description |
|-----------|-------------|
| **Independence of Frameworks** | Business logic doesn't depend on external libraries |
| **Testability** | Business logic can be tested without UI, database, or external services |
| **Independence of UI** | UI can change without affecting business logic |
| **Independence of Database** | Can swap databases without changing business rules |
| **Independence of External Services** | Business logic doesn't know about external APIs |

### 3. Dependency Inversion

Inner layers define interfaces; outer layers implement them.

```csharp
// ✅ CORRECT: Application defines interface
// File: Application/Interfaces/IProductRepository.cs
namespace B2BCommerce.Backend.Application.Interfaces
{
    public interface IProductRepository
    {
        Task<Product> GetByIdAsync(int id);
        Task AddAsync(Product product);
    }
}

// ✅ CORRECT: Infrastructure implements interface
// File: Infrastructure/Repositories/ProductRepository.cs
namespace B2BCommerce.Backend.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;
        
        public async Task<Product> GetByIdAsync(int id)
            => await _context.Products.FindAsync(id);
    }
}

// ✅ CORRECT: Service depends on interface
// File: Application/Services/ProductService.cs
namespace B2BCommerce.Backend.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        
        public ProductService(IProductRepository repository)
        {
            _repository = repository;
        }
    }
}
```

## Layer Responsibilities

### Domain Layer

**Purpose**: Pure business logic and domain models

**Contains**:
- Domain Entities (aggregate roots)
- Value Objects
- Domain Events
- Domain Services (interfaces)
- Domain Exceptions
- Enumerations

**Rules**:
- ❌ NO references to other layers
- ❌ NO external libraries (except .NET BCL)
- ❌ NO knowledge of databases
- ❌ NO knowledge of HTTP/UI
- ❌ NO knowledge of external services

### Application Layer

**Purpose**: Application business logic and use cases

**Contains**:
- Service Interfaces
- Service Implementations
- DTOs (Data Transfer Objects)
- Repository Interfaces
- Validation Rules (FluentValidation)
- AutoMapper Profiles
- MediatR Commands/Queries/Handlers
- Application Exceptions

**Rules**:
- ✅ CAN reference Domain layer
- ❌ NO knowledge of HTTP/UI specifics
- ❌ NO knowledge of database implementation
- ❌ NO knowledge of external service implementations

### Infrastructure Layer

**Purpose**: External concerns and technical implementations

**Contains**:
- DbContext
- Repository Implementations
- Entity Configurations (EF Core)
- External API Clients
- File Storage Services
- Caching Services
- Email/SMS Services
- Background Jobs
- Identity (User/Role entities)

**Rules**:
- ✅ CAN reference Application and Domain layers
- ✅ CAN reference external libraries
- ✅ CAN know about database specifics
- ❌ NO business logic

### Presentation/API Layer

**Purpose**: HTTP/API concerns

**Contains**:
- Controllers
- Middleware
- Filters
- API Models (if different from DTOs)
- Authentication/Authorization configuration
- Swagger configuration
- Dependency Injection configuration

**Rules**:
- ✅ CAN reference Application and Infrastructure (for DI only)
- ✅ CAN handle HTTP specifics
- ❌ NO business logic
- ❌ NO direct database access

## Request Flow

### Typical API Request

```
1. HTTP Request arrives at Controller
                    ↓
2. Controller calls Application Service
                    ↓
3. Service validates input (FluentValidation)
                    ↓
4. Service calls Repository (via interface)
                    ↓
5. Repository executes query (EF Core)
                    ↓
6. Entity returned to Service
                    ↓
7. Service maps Entity to DTO (AutoMapper)
                    ↓
8. DTO returned to Controller
                    ↓
9. Controller returns HTTP Response
```

### With MediatR (CQRS)

```
1. HTTP Request arrives at Controller
                    ↓
2. Controller dispatches Command/Query via MediatR
                    ↓
3. Handler processes request
                    ↓
4. Handler interacts with Repository
                    ↓
5. Result returned through MediatR
                    ↓
6. Controller returns HTTP Response
```

## Patterns Used

### Repository Pattern

Abstracts data access, making the application independent of the data source.

```csharp
// Interface in Application
public interface IProductRepository : IGenericRepository<Product>
{
    Task<Product?> GetBySkuAsync(string sku);
    Task<Product?> GetByExternalCodeAsync(string externalCode);
}

// Implementation in Infrastructure
public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public async Task<Product?> GetBySkuAsync(string sku)
        => await _dbSet.FirstOrDefaultAsync(p => p.Sku == sku);
}
```

### Unit of Work Pattern

Coordinates multiple repository operations within a single transaction.

```csharp
public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    IOrderRepository Orders { get; }
    ICustomerRepository Customers { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

### Result Pattern

Explicit success/failure handling without exceptions for business logic.

```csharp
public class Result<T>
{
    public bool Success { get; }
    public T? Data { get; }
    public string? Error { get; }
    public List<string> Errors { get; }
    
    public static Result<T> Ok(T data) => new(true, data, null);
    public static Result<T> Fail(string error) => new(false, default, error);
}
```

### Factory Methods

Entity creation with validation and domain events.

```csharp
public class Product : BaseEntity
{
    private Product() { } // Private constructor
    
    public static Product Create(string sku, string name, decimal price)
    {
        if (string.IsNullOrEmpty(sku))
            throw new DomainException("SKU is required");
            
        var product = new Product
        {
            Sku = sku,
            Name = name,
            Price = price
        };
        
        product.AddDomainEvent(new ProductCreatedEvent(product.Id));
        return product;
    }
}
```

### CQRS with MediatR

Separates read (Query) and write (Command) operations.

```csharp
// Command
public record UpsertProductCommand(string Sku, string Name, decimal Price) 
    : IRequest<Result<ProductDto>>;

// Handler
public class UpsertProductCommandHandler 
    : IRequestHandler<UpsertProductCommand, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(
        UpsertProductCommand request, 
        CancellationToken cancellationToken)
    {
        // Business logic here
    }
}

// Query
public record GetProductByIdQuery(int Id) : IRequest<ProductDto?>;
```

## Common Violations to Avoid

### ❌ Business Logic in Controller

```csharp
// BAD
[HttpPost]
public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
{
    var order = new Order();
    order.CustomerId = dto.CustomerId;
    
    // Lots of business logic here - WRONG!
    if (dto.Items.Sum(i => i.Quantity) > 100)
        order.ApplyDiscount(0.1m);
    
    await _repository.AddAsync(order);
    return Ok();
}

// GOOD
[HttpPost]
public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
{
    var result = await _orderService.CreateAsync(dto);
    return result.Success ? Ok(result.Data) : BadRequest(result.Error);
}
```

### ❌ Returning Domain Entities from API

```csharp
// BAD - Exposes internal entity structure
[HttpGet("{id}")]
public async Task<Product> GetProduct(int id)
{
    return await _repository.GetByIdAsync(id);
}

// GOOD - Return DTO
[HttpGet("{id}")]
public async Task<ActionResult<ProductDto>> GetProduct(int id)
{
    var dto = await _productService.GetByIdAsync(id);
    return dto != null ? Ok(dto) : NotFound();
}
```

### ❌ Direct Database Access from Controller

```csharp
// BAD
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context; // WRONG!
}

// GOOD
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
}
```

### ❌ Domain Layer Knowing About EF Core

```csharp
// BAD - Domain entity with EF Core attributes
public class Product
{
    [Key] // WRONG!
    public int Id { get; set; }
    
    [Required] // WRONG!
    public string Name { get; set; }
}

// GOOD - Pure entity, configuration in Infrastructure
public class Product : BaseEntity
{
    public string Name { get; private set; }
}

// Configuration in Infrastructure
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired();
    }
}
```

## Decision Checklist

When adding new code, ask yourself:

1. **Where does this code belong?**
   - Business rules → Domain
   - Use case orchestration → Application
   - Database/External services → Infrastructure
   - HTTP handling → API

2. **Am I violating the dependency rule?**
   - Is Domain referencing other layers? ❌
   - Is Application referencing Infrastructure? ❌

3. **Is this testable?**
   - Can I test this without a database?
   - Can I test this without HTTP?

4. **Am I exposing domain entities?**
   - Always use DTOs for API responses

---

**Next**: [03-Domain-Layer-Guide](03-Domain-Layer-Guide.md)
