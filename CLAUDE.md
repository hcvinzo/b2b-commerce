# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

B2B E-Commerce Platform following Clean Architecture principles. A .NET 10 backend API with PostgreSQL (Supabase) for dealer portal, admin panel, and ERP integrations.

## Build & Run Commands

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

## Architecture

**Clean Architecture with 4 layers** - dependencies flow inward toward Domain:

```
API → Infrastructure → Application → Domain
```

| Layer | Project | Responsibility |
|-------|---------|----------------|
| **Domain** | `B2BCommerce.Backend.Domain` | Entities, Value Objects, Domain Services, Exceptions. Pure C#, NO external dependencies |
| **Application** | `B2BCommerce.Backend.Application` | DTOs, Service Interfaces, Repository Interfaces, Validators. Depends only on Domain |
| **Infrastructure** | `B2BCommerce.Backend.Infrastructure` | EF Core DbContext, Repository implementations, Identity, External API clients. Implements Application interfaces |
| **API** | `B2BCommerce.Backend.API` | Controllers, Middleware, Auth config. Thin layer, delegates to Application services |

### Key Architectural Rules

1. **Domain layer has NO dependencies** - pure business logic
2. **Application defines interfaces** (e.g., `IProductRepository`) that Infrastructure implements
3. **Never expose Domain entities to API** - always use DTOs
4. **Repositories use soft delete** - filter by `IsDeleted` automatically
5. **All entities include audit fields**: `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`

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
var product = Product.Create(sku, name, categoryId, brandId, listPrice);
```

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

## Technology Stack

- **.NET 10** (STS - Nov 2025)
- **Entity Framework Core 10** with PostgreSQL (Npgsql)
- **ASP.NET Core Identity** for authentication
- **JWT Bearer** tokens for API auth
- **AutoMapper** for entity-DTO mapping
- **FluentValidation** for DTO validation
- **Serilog** for structured logging
- **xUnit** + FluentAssertions + Moq for testing

## Business Domain

**B2B dealer portal** with:
- Multi-tier pricing (List, Tier1-5, Special per customer)
- Customer credit management with limits and reservations
- Order approval workflow (Pending → Approved/Rejected)
- Serial number tracking per product
- Currency conversion with locked exchange rates

## Documentation

Detailed specifications are in `.docs/`:
- `B2B_Technical_Architecture_Overview.md` - Full system architecture
- `CSharp_Clean_Architecture_Guide.md` - Implementation patterns
- `Domain_Application_Layer_Specification.md` - Entity specifications
- `Infrastructure_Layer_Specification.md` - Data access details
