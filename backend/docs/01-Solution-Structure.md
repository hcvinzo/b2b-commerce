# Solution Structure

## Overview

The B2BCommerce solution follows **Clean Architecture** principles with clear separation between layers. Each project has a specific responsibility and defined dependencies.

## Solution Layout

```
B2BCommerce.Backend/
│
├── src/
│   │
│   ├── B2BCommerce.Backend.Domain/
│   │   ├── Common/
│   │   │   ├── BaseEntity.cs
│   │   │   ├── ExternalEntity.cs
│   │   │   ├── IAggregateRoot.cs
│   │   │   └── IExternalEntity.cs
│   │   ├── Entities/
│   │   │   ├── Product.cs
│   │   │   ├── Category.cs
│   │   │   ├── Brand.cs
│   │   │   ├── Customer.cs
│   │   │   ├── Order.cs
│   │   │   ├── OrderItem.cs
│   │   │   ├── Payment.cs
│   │   │   └── ...
│   │   ├── ValueObjects/
│   │   │   ├── Money.cs
│   │   │   ├── Email.cs
│   │   │   ├── PhoneNumber.cs
│   │   │   ├── Address.cs
│   │   │   └── TaxNumber.cs
│   │   ├── Events/
│   │   │   ├── IDomainEvent.cs
│   │   │   ├── ProductCreatedEvent.cs
│   │   │   ├── OrderSubmittedEvent.cs
│   │   │   └── ...
│   │   ├── Services/
│   │   │   ├── IPricingService.cs
│   │   │   ├── ICreditService.cs
│   │   │   └── ICurrencyService.cs
│   │   ├── Exceptions/
│   │   │   ├── DomainException.cs
│   │   │   ├── InsufficientStockException.cs
│   │   │   └── InsufficientCreditException.cs
│   │   └── Enums/
│   │       ├── OrderStatus.cs
│   │       ├── PaymentStatus.cs
│   │       ├── UserRole.cs
│   │       └── ...
│   │
│   ├── B2BCommerce.Backend.Application/
│   │   ├── Common/
│   │   │   ├── Result.cs
│   │   │   ├── PaginatedList.cs
│   │   │   └── ICurrentUserService.cs
│   │   ├── Interfaces/
│   │   │   ├── Repositories/
│   │   │   │   ├── IGenericRepository.cs
│   │   │   │   ├── IProductRepository.cs
│   │   │   │   ├── IOrderRepository.cs
│   │   │   │   └── ...
│   │   │   ├── Services/
│   │   │   │   ├── IAuthService.cs
│   │   │   │   ├── ITokenService.cs
│   │   │   │   ├── IEmailService.cs
│   │   │   │   └── ...
│   │   │   └── IUnitOfWork.cs
│   │   ├── Services/
│   │   │   ├── ProductService.cs
│   │   │   ├── OrderService.cs
│   │   │   ├── CustomerService.cs
│   │   │   └── ...
│   │   ├── DTOs/
│   │   │   ├── Products/
│   │   │   │   ├── ProductDto.cs
│   │   │   │   ├── CreateProductDto.cs
│   │   │   │   └── UpdateProductDto.cs
│   │   │   ├── Orders/
│   │   │   ├── Customers/
│   │   │   └── Auth/
│   │   ├── Mapping/
│   │   │   ├── ProductMappingProfile.cs
│   │   │   ├── OrderMappingProfile.cs
│   │   │   └── ...
│   │   ├── Validators/
│   │   │   ├── Products/
│   │   │   │   ├── CreateProductValidator.cs
│   │   │   │   └── UpdateProductValidator.cs
│   │   │   └── ...
│   │   ├── EventHandlers/
│   │   │   ├── OrderSubmittedEventHandler.cs
│   │   │   └── ...
│   │   ├── Commands/
│   │   │   ├── Products/
│   │   │   │   ├── UpsertProductCommand.cs
│   │   │   │   └── UpsertProductCommandHandler.cs
│   │   │   └── ...
│   │   ├── Queries/
│   │   │   ├── Products/
│   │   │   │   ├── GetProductByIdQuery.cs
│   │   │   │   └── GetProductsQuery.cs
│   │   │   └── ...
│   │   └── Exceptions/
│   │       ├── NotFoundException.cs
│   │       ├── ValidationException.cs
│   │       └── UnauthorizedException.cs
│   │
│   ├── B2BCommerce.Backend.Infrastructure/
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── UnitOfWork.cs
│   │   │   ├── Configurations/
│   │   │   │   ├── ProductConfiguration.cs
│   │   │   │   ├── OrderConfiguration.cs
│   │   │   │   └── ...
│   │   │   ├── Migrations/
│   │   │   └── Interceptors/
│   │   │       ├── AuditInterceptor.cs
│   │   │       └── SoftDeleteInterceptor.cs
│   │   ├── Repositories/
│   │   │   ├── GenericRepository.cs
│   │   │   ├── ProductRepository.cs
│   │   │   ├── OrderRepository.cs
│   │   │   └── ...
│   │   ├── Identity/
│   │   │   ├── ApplicationUser.cs
│   │   │   ├── ApplicationAdminUser.cs
│   │   │   ├── ApplicationRole.cs
│   │   │   └── RefreshToken.cs
│   │   ├── Services/
│   │   │   ├── TokenService.cs
│   │   │   ├── EmailService.cs
│   │   │   ├── CacheService.cs
│   │   │   └── FileStorageService.cs
│   │   ├── ExternalApis/
│   │   │   ├── Payment/
│   │   │   │   └── PaynetClient.cs
│   │   │   ├── Shipping/
│   │   │   │   └── CargoApiClient.cs
│   │   │   └── Erp/
│   │   │       └── LogoErpClient.cs
│   │   ├── BackgroundJobs/
│   │   │   ├── OrderSyncJob.cs
│   │   │   └── StockUpdateJob.cs
│   │   └── DependencyInjection.cs
│   │
│   ├── B2BCommerce.Backend.API/
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   ├── ProductsController.cs
│   │   │   ├── OrdersController.cs
│   │   │   └── ...
│   │   ├── Middleware/
│   │   │   ├── ExceptionHandlingMiddleware.cs
│   │   │   └── RequestLoggingMiddleware.cs
│   │   ├── Filters/
│   │   │   └── ValidationFilter.cs
│   │   ├── Extensions/
│   │   │   └── ServiceCollectionExtensions.cs
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   └── B2BCommerce.Backend.IntegrationAPI/
│       ├── Controllers/
│       │   ├── IntegrationProductsController.cs
│       │   ├── IntegrationCustomersController.cs
│       │   └── ...
│       ├── Authentication/
│       │   └── ApiKeyAuthenticationHandler.cs
│       ├── Program.cs
│       └── appsettings.json
│
├── tests/
│   ├── B2BCommerce.Backend.Domain.Tests/
│   ├── B2BCommerce.Backend.Application.Tests/
│   ├── B2BCommerce.Backend.Infrastructure.Tests/
│   └── B2BCommerce.Backend.API.Tests/
│
└── B2BCommerce.Backend.sln
```

## Project Dependencies

### Dependency Graph

```
┌─────────────────────────────────────────────────────────────┐
│                        API Layer                             │
│  ┌─────────────────────┐    ┌────────────────────────────┐  │
│  │   B2BCommerce.      │    │   B2BCommerce.             │  │
│  │   Backend.API       │    │   Backend.IntegrationAPI   │  │
│  └──────────┬──────────┘    └─────────────┬──────────────┘  │
└─────────────┼─────────────────────────────┼─────────────────┘
              │                             │
              ▼                             ▼
┌─────────────────────────────────────────────────────────────┐
│               B2BCommerce.Backend.Infrastructure            │
│                                                             │
│  References: Application, Domain                            │
│  Contains: DbContext, Repositories, External Services       │
└──────────────────────────────┬──────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────┐
│                B2BCommerce.Backend.Application              │
│                                                             │
│  References: Domain only                                    │
│  Contains: Services, DTOs, Interfaces, Validators           │
└──────────────────────────────┬──────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────┐
│                  B2BCommerce.Backend.Domain                 │
│                                                             │
│  References: NONE (pure C#)                                 │
│  Contains: Entities, Value Objects, Domain Events           │
└─────────────────────────────────────────────────────────────┘
```

### Project References

| Project | References |
|---------|------------|
| Domain | None |
| Application | Domain |
| Infrastructure | Application, Domain |
| API | Application, Infrastructure |
| IntegrationAPI | Application, Infrastructure |

## NuGet Packages by Project

### Domain
```xml
<!-- No external packages - pure C# only -->
```

### Application
```xml
<PackageReference Include="AutoMapper" Version="12.0.1" />
<PackageReference Include="FluentValidation" Version="11.8.0" />
<PackageReference Include="MediatR" Version="12.2.0" />
```

### Infrastructure
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="8.0.0" />
<PackageReference Include="Hangfire" Version="1.8.6" />
<PackageReference Include="Hangfire.PostgreSql" Version="1.20.4" />
<PackageReference Include="Serilog" Version="3.1.1" />
<PackageReference Include="AWSSDK.S3" Version="3.7.x" />
<PackageReference Include="Polly" Version="8.2.0" />
```

### API / IntegrationAPI
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
```

## Creating New Projects

When creating a new project, ensure:

1. **Namespace follows convention**: `B2BCommerce.Backend.{LayerName}`
2. **Project references are correct**: Only reference allowed dependencies
3. **Folder structure matches**: Follow the established patterns
4. **Tests project exists**: Create corresponding test project

---

**Next**: [02-Architecture-Guide](02-Architecture-Guide.md)
