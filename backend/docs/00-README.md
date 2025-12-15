# B2BCommerce Backend Developer Documentation

## Overview

This documentation provides guidelines for developers working on the **B2BCommerce** backend platform - a B2B e-commerce solution built with .NET 8 and Clean Architecture principles.

## Documentation Index

| Document | Description |
|----------|-------------|
| [01-Solution-Structure](01-Solution-Structure.md) | Project organization and folder structure |
| [02-Architecture-Guide](02-Architecture-Guide.md) | Clean Architecture principles and dependency rules |
| [03-Domain-Layer-Guide](03-Domain-Layer-Guide.md) | Domain entities, value objects, and business rules |
| [04-Application-Layer-Guide](04-Application-Layer-Guide.md) | Services, DTOs, validation, and mapping |
| [05-Infrastructure-Layer-Guide](05-Infrastructure-Layer-Guide.md) | Data access, repositories, and external services |
| [06-API-Layer-Guide](06-API-Layer-Guide.md) | Controllers, authentication, and middleware |
| [07-Coding-Standards](07-Coding-Standards.md) | C# conventions and best practices |
| [08-Testing-Guide](08-Testing-Guide.md) | Testing strategies and examples |

## Technology Stack

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 8.0 LTS | Framework |
| ASP.NET Core | 8.0 | Web API |
| Entity Framework Core | 8.0 | ORM |
| PostgreSQL | 15+ | Database |
| Redis | 7.x | Caching |
| MediatR | Latest | CQRS & Domain Events |
| AutoMapper | Latest | Object Mapping |
| FluentValidation | Latest | Input Validation |
| ASP.NET Core Identity | 8.0 | Authentication |

## Quick Reference

### Namespace Convention

```
B2BCommerce.Backend.Domain
B2BCommerce.Backend.Application
B2BCommerce.Backend.Infrastructure
B2BCommerce.Backend.API
B2BCommerce.Backend.IntegrationAPI
```

### Dependency Direction

```
API → Application → Domain
      ↓
Infrastructure → Application → Domain
```

**Remember**: Dependencies always flow inward toward the Domain layer.

### Key Patterns Used

- **Clean Architecture** - Layer separation with dependency inversion
- **Repository Pattern** - Data access abstraction
- **Unit of Work** - Transaction management
- **CQRS** - Command/Query separation via MediatR
- **Result Pattern** - Explicit success/failure handling
- **Factory Methods** - Entity creation with validation

### Getting Started

1. Clone the repository
2. Ensure PostgreSQL and Redis are running
3. Update connection strings in `appsettings.Development.json`
4. Run migrations: `dotnet ef database update`
5. Start the API: `dotnet run --project src/B2BCommerce.Backend.API`

### Before You Code

1. Read the Architecture Guide to understand layer responsibilities
2. Review the specific layer guide for where your code belongs
3. Follow Coding Standards for consistency
4. Write tests as per Testing Guide

---

**Version**: 1.0  
**Last Updated**: December 2025
