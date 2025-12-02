# B2B E-Commerce Backend API

A complete ASP.NET Core Web API implementation for a B2B E-Commerce platform following Clean Architecture principles.

## Project Structure

```
backend/
├── src/
│   ├── B2BCommerce.Backend.Domain/           # Domain Layer (✅ Complete)
│   ├── B2BCommerce.Backend.Application/      # Application Layer (✅ Interfaces & DTOs Complete)
│   ├── B2BCommerce.Backend.Infrastructure/   # Infrastructure Layer (✅ Complete)
│   └── B2BCommerce.Backend.API/              # API Layer (✅ Configuration Complete)
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

#### 3. Infrastructure Layer (B2BCommerce.Backend.Infrastructure) - Complete & Building ✅

**Data Access:**
- ✅ ApplicationDbContext with all DbSets and audit handling
- ✅ 11 Entity Configurations using Fluent API
- ✅ GenericRepository<T> base implementation
- ✅ 8 Specific Repository implementations
- ✅ UnitOfWork with transaction management

**Entity Configurations:**
- ✅ Value objects as owned types (Money, Address)
- ✅ Value object conversions (Email, PhoneNumber, TaxNumber)
- ✅ JSON columns for collections (ImageUrls, Specifications, SerialNumbers)
- ✅ Proper relationships with cascade behaviors
- ✅ Indexes for performance (unique and composite)
- ✅ Soft delete filtering

**Repositories:**
- ✅ ProductRepository (SKU search, category filter, advanced search)
- ✅ OrderRepository (order number lookup, customer orders, pending orders)
- ✅ CustomerRepository (email/tax number lookup, unapproved customers)
- ✅ CategoryRepository, BrandRepository, PaymentRepository, ShipmentRepository
- ✅ CurrencyRateRepository (with historical date filtering)

**Identity:**
- ✅ ApplicationUser (custom properties: FirstName, LastName, CustomerId, RefreshToken)
- ✅ ApplicationRole (with Description)
- ✅ Identity configuration (password policies, lockout settings)

**Service Registration:**
- ✅ DependencyInjection class
- ✅ PostgreSQL with retry logic
- ✅ All repositories registered as scoped services

#### 4. API Layer (B2BCommerce.Backend.API) - Configuration Complete ✅

**Configuration:**
- ✅ Program.cs with complete setup (DI, middleware, auth, Swagger)
- ✅ appsettings.json with all configurations
- ✅ JWT Authentication with Bearer token support
- ✅ Serilog structured logging (Console + File)
- ✅ Swagger/OpenAPI with JWT authentication UI
- ✅ CORS policy for frontend
- ✅ Health checks with EF Core
- ✅ ASP.NET Core Identity integration

**NuGet Packages:**
- ✅ Microsoft.AspNetCore.Authentication.JwtBearer 8.0.11
- ✅ Serilog.AspNetCore 8.0.3
- ✅ AutoMapper.Extensions.Microsoft.DependencyInjection 12.0.1
- ✅ FluentValidation.AspNetCore 11.3.0
- ✅ Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore 8.0.11
- ✅ Swashbuckle.AspNetCore 6.6.2

### ⏳ Pending

#### API Layer - Controllers & Business Logic
- ⏳ Controllers (Products, Orders, Customers, Auth, etc.)
- ⏳ Application Services (ProductService, OrderService, etc.)
- ⏳ Global exception handling middleware

#### Cross-Cutting Concerns
- ⏳ FluentValidation validators for DTOs
- ⏳ AutoMapper profiles for entity-DTO mappings
- ⏳ JWT Token Service implementation (generation, validation, refresh)

#### Database & Data
- ✅ EF Core migrations (InitialCreate with PostgreSQL types)
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

The EF Core migration creates the following 18 tables:

**Business Entities:**
- **Products**: 50+ fields including pricing, stock, images, specifications
- **Categories**: Hierarchical categories with parent-child relationships
- **Brands**: Brand information with logos and website URLs
- **Customers**: Credit management, addresses, approval status
- **CustomerAddresses**: Multiple addresses per customer with default flag
- **Orders**: Financial tracking, approval workflow, shipping details
- **OrderItems**: Line items with pricing, tax, discounts, serial numbers
- **Payments**: Multiple payment methods, gateway integration
- **Shipments**: Carrier tracking, delivery status
- **CurrencyRates**: Historical currency exchange rates
- **SystemConfigurations**: System-wide configuration settings

**ASP.NET Core Identity Tables:**
- **Users**: Application users with custom properties (FirstName, LastName, CustomerId, RefreshToken)
- **Roles**: Application roles with descriptions
- **UserRoles**: User-role assignments
- **UserClaims**: User claims
- **RoleClaims**: Role claims
- **UserLogins**: External login providers
- **UserTokens**: Authentication tokens

All tables include:
- Audit fields (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
- Soft delete support (IsDeleted, DeletedAt, DeletedBy)
- Optimized indexes for queries
- Proper foreign key relationships with cascade behaviors

## Next Steps

To continue development:

1. **Configure Database Connection:**
   - Update the password in [appsettings.json](src/B2BCommerce.Backend.API/appsettings.json)
   - Replace `YOUR_PASSWORD_HERE` with your actual Supabase PostgreSQL password
   - Also update the JWT Key with a secure 32+ character secret

2. **Apply Database Migrations:**
   ```bash
   dotnet ef database update --project src/B2BCommerce.Backend.Infrastructure --startup-project src/B2BCommerce.Backend.API
   ```

3. **Implement Application Services:**
   - ProductService with business logic
   - OrderService with approval workflow
   - CustomerService with credit management
   - AuthService with JWT token generation

3. **Add Validators & Mappers:**
   - FluentValidation validators for all DTOs
   - AutoMapper profiles for entity-DTO mappings

4. **Implement API Controllers:**
   - ProductsController (CRUD + search)
   - OrdersController (CRUD + approval)
   - CustomersController (CRUD + credit management)
   - AuthController (login, register, refresh token)
   - CategoriesController, BrandsController, etc.

5. **Add Middleware:**
   - Global exception handling middleware
   - Request/Response logging enhancements

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
