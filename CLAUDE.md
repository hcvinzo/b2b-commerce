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
| **Domain** | `B2BCommerce.Backend.Domain` | Entities, Value Objects, Domain Services, Exceptions. Pure C#, NO external dependencies |
| **Application** | `B2BCommerce.Backend.Application` | DTOs, Service Interfaces, Repository Interfaces, CQRS Handlers, Validators. Depends only on Domain |
| **Infrastructure** | `B2BCommerce.Backend.Infrastructure` | EF Core DbContext, Repository implementations, Service implementations, Identity. Implements Application interfaces |
| **API** | `B2BCommerce.Backend.API` | Controllers, Middleware, Auth config. Thin layer, delegates via MediatR |
| **IntegrationAPI** | `B2BCommerce.Backend.IntegrationAPI` | External API for ERP integrations. API Key auth, delegates via MediatR |

### Key Architectural Rules

1. **Domain layer has NO dependencies** - pure business logic
2. **Application defines interfaces** (e.g., `IProductRepository`, `ICategoryService`) that Infrastructure implements
3. **Never expose Domain entities to API** - always use DTOs
4. **Repositories use soft delete** - filter by `IsDeleted` automatically
5. **All entities include audit fields**: `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`
6. **Controllers delegate to Application layer** - use MediatR for CQRS, never use EF Core directly in controllers
7. **Use pattern matching for null checks** - prefer `is null` / `is not null` over `== null` / `!= null`
   - **Exception**: Expression trees (AutoMapper, EF Core LINQ) must use `!= null` due to CS8122
8. **Use domain exceptions** - throw `InvalidOperationDomainException` instead of `System.InvalidOperationException`

## Domain Entities & Value Objects

**Core Entities**: Product, Category, Brand, Customer, CustomerAddress, Order, OrderItem, Payment, Shipment, CurrencyRate, SystemConfiguration

**Value Objects** (immutable, stored as owned types):
- `Money` - amount + currency with arithmetic operators
- `Address` - street, city, state, country, postalCode
- `Email`, `PhoneNumber`, `TaxNumber` - validated wrapper types with conversions

**Enums**: OrderStatus, PaymentStatus, ShipmentStatus, PriceTier, CustomerType, OrderApprovalStatus, PaymentMethod

## Code Patterns

### Creating Entities
Use factory methods on entities, not direct constructors:
```csharp
// Correct - use factory method
var product = Product.Create(sku, name, categoryId, brandId, listPrice);
var category = Category.Create(name, description, parentCategoryId, displayOrder);
var customer = Customer.Create(companyName, taxNumber, email, phone, ...);

// Wrong - direct constructor (marked as [Obsolete])
var product = new Product(...); // Will generate warning CS0618
```

All entities have factory methods: `Product`, `Category`, `Brand`, `Customer`, `Order`, `OrderItem`, `Payment`, `Shipment`, `ApiClient`, `ApiKey`

### Repository Pattern
```csharp
// Application layer defines interface
public interface IProductRepository : IGenericRepository<Product>
{
    Task<Product?> GetBySKUAsync(string sku);
}

// Infrastructure implements it
public class ProductRepository : GenericRepository<Product>, IProductRepository
```

### Unit of Work
```csharp
await _unitOfWork.BeginTransactionAsync();
try {
    // operations
    await _unitOfWork.SaveChangesAsync();
    await _unitOfWork.CommitAsync();
} catch {
    await _unitOfWork.RollbackAsync();
    throw;
}
```

### Value Object Configurations (EF Core)
```csharp
// Owned types for Money
builder.OwnsOne(e => e.ListPrice, money => {
    money.Property(m => m.Amount).HasColumnName("ListPriceAmount");
    money.Property(m => m.Currency).HasColumnName("ListPriceCurrency");
});

// Conversions for Email/Phone/TaxNumber
builder.Property(c => c.Email)
    .HasConversion(email => email.Value, value => new Email(value));
```

### CQRS Pattern (Commands & Queries)
Controllers use MediatR to dispatch commands/queries to handlers:
```csharp
// Controller (thin layer)
[HttpGet("{id:guid}")]
public async Task<IActionResult> GetCategory(Guid id)
{
    var query = new GetCategoryByIdQuery(id);
    var result = await _mediator.Send(query);

    if (!result.IsSuccess)
        return NotFoundResponse(result.ErrorMessage);

    return OkResponse(result.Data);
}

// Query in Application layer
public record GetCategoryByIdQuery(Guid Id) : IQuery<Result<CategoryDto>>;

// Handler delegates to service
public class GetCategoryByIdQueryHandler : IQueryHandler<GetCategoryByIdQuery, Result<CategoryDto>>
{
    private readonly ICategoryService _categoryService;

    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken ct)
    {
        return await _categoryService.GetByIdAsync(request.Id, ct);
    }
}
```

### Service Layer Pattern
Services in Infrastructure implement Application interfaces and contain business logic:
```csharp
// Application layer defines interface
public interface ICategoryService
{
    Task<Result<CategoryDto>> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Result<CategoryDto>> CreateAsync(CreateCategoryDto dto, string? createdBy, CancellationToken ct);
}

// Infrastructure implements with EF Core
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    // ... implementation using EF Core
}
```

## Technology Stack

### Backend
- **.NET 10** (STS - Nov 2025)
- **Entity Framework Core 10** with PostgreSQL (Npgsql)
- **ASP.NET Core Identity** for authentication
- **JWT Bearer** tokens for API auth
- **MediatR** for CQRS pattern (Commands/Queries)
- **AutoMapper** for entity-DTO mapping
- **FluentValidation** for DTO validation
- **Serilog** for structured logging
- **xUnit** + FluentAssertions + Moq for testing

### Frontend
- **Next.js 16** with App Router (static export)
- **React 19** with TypeScript
- **Tailwind CSS 4** for styling
- **react-hook-form** + **Zod** for form validation
- **Zustand** for state management
- **Axios** for API calls

## Business Domain

**B2B dealer portal** with:
- Multi-tier pricing (List, Tier1-5, Special per customer)
- Customer credit management with limits and reservations
- Order approval workflow (Pending → Approved/Rejected)
- Serial number tracking per product
- Currency conversion with locked exchange rates

## Deployment

### Frontend (AWS EC2 with Nginx)
- Static export to `frontend/out/`
- Nginx config provided in `frontend/nginx.conf`
- Coming-soon redirect handled by Nginx `try_files` directive

```bash
# Deploy to EC2
scp -r frontend/out/* ec2-user@your-ec2:/var/www/frontend/out/
sudo cp frontend/nginx.conf /etc/nginx/conf.d/frontend.conf
sudo nginx -t && sudo systemctl restart nginx
```

### Backend
- Deployed as .NET application
- Environment variables via `.env` or AWS Parameter Store

## Project Structure

```
backend/src/
├── B2BCommerce.Backend.Domain/
│   ├── Common/              # BaseEntity, audit fields
│   ├── Entities/            # Product, Category, Order, etc.
│   ├── ValueObjects/        # Money, Address, Email, etc.
│   ├── Enums/               # OrderStatus, PaymentStatus, etc.
│   ├── DomainServices/      # CreditManagementService, OrderValidationService
│   └── Exceptions/          # InvalidOperationDomainException, etc.
│
├── B2BCommerce.Backend.Application/
│   ├── Common/              # Result<T>, PagedResult<T>, CQRS interfaces
│   ├── DTOs/                # CategoryDto, ProductDto, etc.
│   ├── Features/            # CQRS Commands & Queries by feature
│   │   └── Categories/
│   │       ├── Commands/    # CreateCategory, UpdateCategory, etc.
│   │       └── Queries/     # GetCategories, GetCategoryById, etc.
│   ├── Interfaces/
│   │   ├── Repositories/    # IProductRepository, ICategoryRepository
│   │   └── Services/        # ICategoryService, IProductService
│   ├── Mappings/            # AutoMapper profiles
│   └── Validators/          # FluentValidation validators
│
├── B2BCommerce.Backend.Infrastructure/
│   ├── Data/                # ApplicationDbContext, Migrations
│   │   ├── Configurations/  # EF Core entity configurations
│   │   └── Repositories/    # Repository implementations
│   ├── Services/            # Service implementations (CategoryService, etc.)
│   └── Identity/            # ASP.NET Core Identity
│
├── B2BCommerce.Backend.API/
│   └── Controllers/         # MediatR-based controllers (JWT auth)
│
└── B2BCommerce.Backend.IntegrationAPI/
    ├── Controllers/         # MediatR-based controllers (API Key auth)
    └── DTOs/                # API-specific DTOs (mapped from Application DTOs)
```

## Documentation

Detailed specifications are in `docs/`:
- `00-08` - Architecture specification documents
- `B2B_Technical_Architecture_Overview.md` - Full system architecture
- `CSharp_Clean_Architecture_Guide.md` - Implementation patterns
- `Domain_Application_Layer_Specification.md` - Entity specifications
- `Infrastructure_Layer_Specification.md` - Data access details
