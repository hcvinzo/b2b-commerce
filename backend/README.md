# B2B E-Commerce Backend API

A complete ASP.NET Core Web API implementation for a B2B E-Commerce platform following Clean Architecture principles.

## Project Structure

```
backend/
├── src/
│   ├── B2BCommerce.Backend.Domain/           # Domain Layer (✅ Complete)
│   ├── B2BCommerce.Backend.Application/      # Application Layer (✅ Interfaces & DTOs Complete)
│   ├── B2BCommerce.Backend.Infrastructure/   # Infrastructure Layer (⚙️ In Progress)
│   └── B2BCommerce.Backend.API/              # API Layer (⏳ Pending)
│
├── tests/
│   ├── B2BCommerce.Backend.Domain.Tests/
│   ├── B2BCommerce.Backend.Application.Tests/
│   ├── B2BCommerce.Backend.Infrastructure.Tests/
│   └── B2BCommerce.Backend.API.Tests/
│
└── README.md
```

## Technology Stack

- **.NET 8** (LTS)
- **ASP.NET Core Web API**
- **Entity Framework Core 8**
- **PostgreSQL** (Supabase)
- **ASP.NET Core Identity** (Authentication)
- **AutoMapper** (Object Mapping)
- **FluentValidation** (Validation)

## Implementation Status

### ✅ Completed

#### 1. Domain Layer (B2BCommerce.Backend.Domain)

**Entities:**
- ✅ Product (with pricing tiers, stock management, images)
- ✅ Category (hierarchical categories)
- ✅ Brand
- ✅ Customer (with credit management, addresses, approval workflow)
- ✅ CustomerAddress
- ✅ Order (with approval workflow, financial tracking)
- ✅ OrderItem (with pricing, tax, discounts, serial numbers)
- ✅ Payment (with multiple payment methods, status tracking)
- ✅ Shipment (with tracking, carrier information, status)
- ✅ SystemConfiguration
- ✅ CurrencyRate (for currency conversion)

**Value Objects:**
- ✅ Money (amount + currency with operations)
- ✅ Address (street, city, state, country, postal code)
- ✅ Email (validated email address)
- ✅ PhoneNumber (validated phone)
- ✅ TaxNumber (validated tax ID)

**Enums:**
- ✅ OrderStatus, PaymentStatus, ShipmentStatus
- ✅ PriceTier, CustomerType, OrderApprovalStatus
- ✅ PaymentMethod

**Domain Services:**
- ✅ IPricingService / PricingService (tier pricing, currency conversion)
- ✅ ICreditManagementService / CreditManagementService (credit validation, tracking)
- ✅ IOrderValidationService / OrderValidationService (stock checks, credit checks)

**Domain Exceptions:**
- ✅ DomainException (base exception)
- ✅ InsufficientCreditException
- ✅ OutOfStockException
- ✅ InvalidOperationDomainException

#### 2. Application Layer (B2BCommerce.Backend.Application)

**Repository Interfaces:**
- ✅ IGenericRepository<T> (base repository with common operations)
- ✅ IProductRepository (with SKU, category search)
- ✅ IOrderRepository (with order number, customer orders)
- ✅ ICustomerRepository (with email, tax number lookup)
- ✅ ICategoryRepository (with parent/child navigation)
- ✅ IBrandRepository
- ✅ IPaymentRepository
- ✅ IShipmentRepository
- ✅ ICurrencyRateRepository
- ✅ IUnitOfWork (transaction management)

**DTOs:**
- ✅ Product DTOs (ProductDto, CreateProductDto, UpdateProductDto, ProductListDto)
- ✅ Order DTOs (OrderDto, CreateOrderDto, OrderItemDto, CreateOrderItemDto)
- ✅ Customer DTOs (CustomerDto, RegisterCustomerDto, UpdateCustomerDto)
- ✅ Auth DTOs (LoginRequestDto, LoginResponseDto, RefreshTokenDto, ChangePasswordDto)

**Common:**
- ✅ Result<T> pattern for operation responses
- ✅ PagedResult<T> for pagination

**Service Interfaces:**
- ✅ IProductService (basic interface created)

### ⚙️ In Progress

#### Infrastructure Layer (B2BCommerce.Backend.Infrastructure)
- ✅ NuGet packages installed (EF Core 8, PostgreSQL, Identity)
- ⏳ ApplicationDbContext (needs implementation)
- ⏳ Entity Configurations (Fluent API)
- ⏳ Repository Implementations
- ⏳ Unit of Work Implementation
- ⏳ Identity (ApplicationUser, ApplicationRole, JwtTokenService)

### ⏳ Pending

#### API Layer (B2BCommerce.Backend.API)
- ⏳ Controllers (Products, Orders, Customers, Auth, etc.)
- ⏳ Program.cs configuration
- ⏳ Middleware (Exception handling, Logging, etc.)
- ⏳ Swagger/OpenAPI configuration
- ⏳ appsettings.json configuration

#### Cross-Cutting Concerns
- ⏳ FluentValidation validators
- ⏳ AutoMapper profiles
- ⏳ JWT Authentication configuration
- ⏳ Global exception handling
- ⏳ Logging (Serilog)

#### Database & Data
- ⏳ EF Core migrations
- ⏳ Seed data

## Key Business Features Implemented

### Order Management
- Multi-tier pricing (List, Tier1-5, Special)
- Order approval workflow (Pending → Approved/Rejected)
- Order status tracking (Pending, Approved, Processing, Shipped, Delivered, Cancelled)
- Currency conversion with locked exchange rates

### Customer Credit Management
- Credit limit tracking
- Used credit calculation
- Available credit validation
- Credit near-limit alerts (90% threshold)
- Credit reservation on order approval
- Credit release on order cancellation

### Product Management
- SKU-based product tracking
- Multi-tier pricing
- Stock quantity management
- Stock reservation/release
- Minimum order quantity
- Serial number tracking
- Product images and specifications
- Active/Inactive status

### Payment & Shipment
- Multiple payment methods (Credit Card, Bank Transfer, Open Account, COD, Check)
- Payment status tracking (Pending, Authorized, Captured, Failed, Refunded)
- Shipment tracking with carrier information
- Shipment status workflow

## Database Schema

The domain model includes:
- **Products**: 50+ fields including pricing, stock, images, specifications
- **Customers**: Credit management, addresses, approval status
- **Orders**: Financial tracking, approval workflow, shipping details
- **Payments**: Multiple payment methods, gateway integration
- **Shipments**: Carrier tracking, delivery status

## Next Steps

To continue development:

1. **Complete Infrastructure Layer:**
   - Implement ApplicationDbContext with all DbSets
   - Create Entity Configurations for all entities
   - Implement Repository classes
   - Implement UnitOfWork
   - Configure ASP.NET Core Identity

2. **Create Database Migrations:**
   ```bash
   dotnet ef migrations add InitialCreate --project src/B2BCommerce.Backend.Infrastructure --startup-project src/B2BCommerce.Backend.API
   ```

3. **Implement API Layer:**
   - Configure Program.cs (DI, middleware, auth, Swagger)
   - Implement controllers
   - Add global exception handling middleware

4. **Add Validators & Mappers:**
   - FluentValidation validators for all DTOs
   - AutoMapper profiles for entity-DTO mappings

5. **Configure appsettings.json:**
   - Connection strings (PostgreSQL)
   - JWT settings
   - External services (AWS S3, Email, etc.)

## Database Connection

**PostgreSQL (Supabase):**
```
User Id=postgres.imtvvmzdyujmvijrgbff;
Password=[YOUR_PASSWORD];
Server=aws-1-eu-central-1.pooler.supabase.com;
Port=6543;
Database=postgres
```

## Building the Project

```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build src/B2BCommerce.Backend.Domain/B2BCommerce.Backend.Domain.csproj

# Run tests
dotnet test

# Run API
dotnet run --project src/B2BCommerce.Backend.API/B2BCommerce.Backend.API.csproj
```

## Architecture Principles

This project follows **Clean Architecture**:

1. **Domain Layer**: No dependencies, pure C# business logic
2. **Application Layer**: Depends only on Domain
3. **Infrastructure Layer**: Depends on Application and Domain
4. **API Layer**: Depends on all layers (for DI only)

## License

[Your License]
