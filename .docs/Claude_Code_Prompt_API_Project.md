# Claude Code Prompt: Generate B2B E-Commerce Backend API Project

## Project Overview

Create a complete ASP.NET Core Web API project for a B2B E-Commerce platform
following Clean Architecture principles. The project should implement a
multi-layered architecture with Domain, Application, Infrastructure, and API
layers.

## Project Structure

Create the following solution structure:

```
B2BCommerce.Backend.Solution/
│
├── src/
│   ├── B2BCommerce.Backend.Domain/           # Domain Layer (no dependencies)
│   ├── B2BCommerce.Backend.Application/      # Application Layer (depends on Domain)
│   ├── B2BCommerce.Backend.Infrastructure/   # Infrastructure Layer (depends on Application, Domain)
│   └── B2BCommerce.Backend.API/              # API Layer (depends on all layers)
│
├── tests/
│   ├── B2BCommerce.Backend.Domain.Tests/
│   ├── B2BCommerce.Backend.Application.Tests/
│   ├── B2BCommerce.Backend.Infrastructure.Tests/
│   └── B2BCommerce.Backend.API.Tests/
│
└── docs/
    └── README.md
```

## Technology Stack

- **.NET 8** (Latest LTS)
- **ASP.NET Core Web API**
- **Entity Framework Core 8**
- **PostgreSQL** (Database)
- **Redis** (Caching)
- **JWT Authentication**
- **Swagger/OpenAPI** (API Documentation)
- **Serilog** (Logging)
- **AutoMapper** (Object Mapping)
- **FluentValidation** (Validation)
- **Hangfire** (Background Jobs)
- **AWS S3** (File Storage)

## Reference Documents

Use the following project knowledge documents as authoritative sources:

1. **docs/B2B_Technical_Architecture_Overview.md** - Overall system architecture
2. **docs/CSharp_Clean_Architecture_Guide.md** - Clean Architecture
   implementation guidelines
3. **docs/Domain_Application_Layer_Specification.md** - Domain and Application
   layer specifications
4. **docs/Infrastructure_Layer_Specification.md** - Infrastructure layer
   specifications
5. **docs/Authentication_With_Identity_Guide.md** - Authentication and
   authorization implementation
6. **docs/Enorm_Technology_B2B_ECommerce_Scope_EN.docx** - Business requirements

## Layer-by-Layer Implementation

### 1. Domain Layer (B2BCommerce.Backend.Domain)

**Requirements:**

- Pure C# classes with no external dependencies
- Implement all domain entities as specified in
  Domain_Application_Layer_Specification.md
- Include value objects, enums, domain events, and domain exceptions
- Follow DDD principles with aggregate roots and entity validation

**Key Entities to Implement:**

- Product (with pricing tiers, images, stock)
- Customer (with credit management, addresses)
- Order (with approval workflow, items, status tracking)
- Category, Brand
- Payment, Shipment
- SystemConfiguration, CurrencyRate

**Domain Services:**

- PricingService (tier pricing, special pricing, currency conversion)
- CreditManagementService (credit validation, usage tracking)
- OrderValidationService (stock checks, credit checks)

**Value Objects:**

- Money (amount + currency)
- Address (street, city, country, postal code)
- Email, PhoneNumber, TaxNumber

**Enums:**

- OrderStatus, PaymentStatus, ShipmentStatus
- PriceTier, CustomerType, OrderApprovalStatus

### 2. Application Layer (B2BCommerce.Backend.Application)

**Requirements:**

- Depend only on Domain layer
- Define all repository and service interfaces
- Implement business logic orchestration
- Include DTOs for data transfer
- Implement validation with FluentValidation
- Configure AutoMapper profiles

**Key Components:**

**Service Interfaces and Implementations:**

- IProductService / ProductService
- IOrderService / OrderService
- ICustomerService / CustomerService
- IAuthService / AuthService
- IPaymentService / PaymentService
- IShipmentService / ShipmentService

**Repository Interfaces (implemented in Infrastructure):**

- IGenericRepository<T>
- IProductRepository
- IOrderRepository
- ICustomerRepository
- ICategoryRepository
- IBrandRepository

**DTOs:**

- Product DTOs (CreateProductDto, UpdateProductDto, ProductDto, ProductListDto)
- Order DTOs (CreateOrderDto, UpdateOrderDto, OrderDto, OrderDetailsDto)
- Customer DTOs (RegisterCustomerDto, CustomerDto, CustomerDetailsDto)
- Auth DTOs (LoginRequestDto, LoginResponseDto, RefreshTokenDto)

**Validators:**

- CreateProductValidator
- CreateOrderValidator
- RegisterCustomerValidator
- LoginRequestValidator

**AutoMapper Profiles:**

- ProductProfile
- OrderProfile
- CustomerProfile
- AuthProfile

**External Service Interfaces:**

- IEmailService
- ISmsService
- IFileStorageService
- IPaymentGatewayService
- ICacheService
- IErpIntegrationService

### 3. Infrastructure Layer (B2BCommerce.Backend.Infrastructure)

**Requirements:**

- Implement all repository interfaces from Application layer
- Configure Entity Framework Core with PostgreSQL
- Implement external service integrations
- Configure caching with Redis
- Implement background jobs with Hangfire
- Follow specifications from Infrastructure_Layer_Specification.md

**Key Components:**

**Data Access:**

- ApplicationDbContext (with DbSets, configurations, audit handling)
- Entity Configurations (Fluent API configurations for all entities)
- UnitOfWork (transaction management)
- Repository implementations

**External Services:**

- EmailService (SMTP email sending with templates)
- SmsService (SMS gateway integration)
- S3FileStorageService (AWS S3 file operations)
- PaynetClient (payment gateway integration)
- RedisCacheService (distributed caching)
- ErpIntegrationService (LOGO ERP integration)

**Background Jobs:**

- OrderSyncJob
- StockUpdateJob
- PriceUpdateJob
- EmailNotificationJob

**Identity:**

- ApplicationUser (extending IdentityUser)
- ApplicationRole (extending IdentityRole)
- JwtTokenService (token generation and validation)

### 4. API Layer (B2BCommerce.Backend.API)

**Requirements:**

- Configure all middleware, authentication, and services
- Implement RESTful controllers
- Configure Swagger/OpenAPI documentation
- Implement global exception handling
- Configure CORS, rate limiting, and security headers
- Follow REST API best practices

**Key Components:**

**Controllers:**

**ProductsController:**

```
GET    /api/products              - Get all products (paginated, filtered)
GET    /api/products/{id}         - Get product by ID
GET    /api/products/sku/{sku}    - Get product by SKU
POST   /api/products              - Create product (Admin only)
PUT    /api/products/{id}         - Update product (Admin only)
DELETE /api/products/{id}         - Delete product (Admin only)
GET    /api/products/search       - Search products
GET    /api/products/category/{categoryId} - Get products by category
```

**OrdersController:**

```
GET    /api/orders                - Get all orders (paginated, filtered)
GET    /api/orders/{id}           - Get order by ID
GET    /api/orders/my-orders      - Get current customer's orders
POST   /api/orders                - Create order
PUT    /api/orders/{id}/approve   - Approve order (Admin only)
PUT    /api/orders/{id}/reject    - Reject order (Admin only)
PUT    /api/orders/{id}/cancel    - Cancel order
GET    /api/orders/{id}/invoice   - Get order invoice
```

**CustomersController:**

```
GET    /api/customers             - Get all customers (Admin only)
GET    /api/customers/{id}        - Get customer by ID
GET    /api/customers/me          - Get current customer profile
POST   /api/customers/register    - Register new customer
PUT    /api/customers/{id}        - Update customer
PUT    /api/customers/{id}/approve - Approve customer (Admin only)
GET    /api/customers/{id}/credit  - Get customer credit info
```

**AuthController:**

```
POST   /api/auth/login            - Login (returns JWT token)
POST   /api/auth/refresh          - Refresh access token
POST   /api/auth/logout           - Logout
POST   /api/auth/change-password  - Change password
POST   /api/auth/forgot-password  - Request password reset
POST   /api/auth/reset-password   - Reset password
```

**CategoriesController, BrandsController, PaymentsController,
ShipmentsController** (similar CRUD operations)

**Middleware:**

- ExceptionHandlingMiddleware (global error handling)
- RequestLoggingMiddleware (log all requests)
- PerformanceMonitoringMiddleware (track response times)

**Filters:**

- ValidationFilter (automatic model validation)
- AuthorizationFilter (role-based access control)

**Configuration Files:**

- Program.cs (dependency injection, middleware pipeline)
- appsettings.json (configuration)
- appsettings.Development.json
- appsettings.Production.json

## Detailed Implementation Instructions

### Program.cs Configuration

Implement the following in Program.cs:

1. **Add Services:**
   - Configure DbContext with PostgreSQL connection string
   - Add Application services (from Application layer)
   - Add Infrastructure services (from Infrastructure layer)
   - Configure Authentication (JWT Bearer)
   - Configure Authorization policies
   - Add AutoMapper
   - Add FluentValidation
   - Configure Swagger/OpenAPI
   - Add CORS policy
   - Add Redis caching
   - Add Hangfire
   - Configure Serilog logging
   - Add API versioning
   - Add health checks

2. **Configure Middleware Pipeline:**
   - UseHttpsRedirection
   - UseCors
   - UseAuthentication
   - UseAuthorization
   - UseSwagger (in development)
   - UseSwaggerUI (in development)
   - Custom exception handling middleware
   - Request logging middleware
   - Performance monitoring middleware

### Authentication & Authorization

Implement JWT-based authentication following
Authentication_With_Identity_Guide.md:

1. **JWT Configuration:**
   - Token generation with claims (UserId, Email, Roles)
   - Access token (15 minutes expiration)
   - Refresh token (7 days expiration)
   - Token validation parameters

2. **Authorization Policies:**
   - Admin policy (requires Admin role)
   - Customer policy (requires Customer role)
   - ApprovedCustomer policy (requires approved customer status)

3. **Password Security:**
   - ASP.NET Core Identity password hashing
   - Minimum password requirements
   - Account lockout after failed attempts

### API Response Format

Standardize all API responses:

**Success Response:**

```json
{
   "success": true,
   "data": {},
   "message": "Operation completed successfully"
}
```

**Error Response:**

```json
{
   "success": false,
   "error": {
      "code": "VALIDATION_ERROR",
      "message": "Validation failed",
      "details": [
         {
            "field": "Email",
            "message": "Email is required"
         }
      ]
   }
}
```

**Pagination Response:**

```json
{
   "success": true,
   "data": {
      "items": [],
      "pageNumber": 1,
      "pageSize": 20,
      "totalPages": 5,
      "totalCount": 100,
      "hasPreviousPage": false,
      "hasNextPage": true
   }
}
```

### Validation Rules

Implement comprehensive validation for all DTOs:

**Product Validation:**

- Name: Required, max 200 characters
- SKU: Required, unique, max 50 characters
- Prices: Required, greater than 0
- Stock quantity: Non-negative integer

**Order Validation:**

- At least one order item required
- Product must be active and in stock
- Quantity must be >= minimum order quantity
- Customer must have sufficient credit
- Shipping address required

**Customer Registration Validation:**

- Company name: Required, max 200 characters
- Tax number: Required, unique, valid format
- Email: Required, unique, valid email format
- Phone: Required, valid phone format
- Password: Min 8 characters, must include uppercase, lowercase, and digit

### Error Handling

Implement global exception handling:

**Exception Types:**

- ValidationException → 400 Bad Request
- NotFoundException → 404 Not Found
- UnauthorizedException → 401 Unauthorized
- ForbiddenException → 403 Forbidden
- BusinessRuleException → 422 Unprocessable Entity
- InsufficientCreditException → 422 Unprocessable Entity
- OutOfStockException → 422 Unprocessable Entity
- All others → 500 Internal Server Error

### Database Configuration

**Connection String:**

```
User Id=postgres.imtvvmzdyujmvijrgbff;Password=[YOUR_PASSWORD];Server=aws-1-eu-central-1.pooler.supabase.com;Port=6543;Database=postgres
```

**EF Core Configuration:**

- Use Npgsql provider for PostgreSQL
- Enable sensitive data logging in development
- Configure connection pooling
- Set command timeout to 30 seconds
- Enable query splitting for complex queries

**Migration Commands:**

```bash
dotnet ef migrations add InitialCreate --project src/B2BCommerce.Backend.Infrastructure --startup-project src/B2BCommerce.Backend.API
dotnet ef database update --project src/B2BCommerce.Backend.Infrastructure --startup-project src/B2BCommerce.Backend.API
```

### Logging Configuration

Configure Serilog with:

- Console sink (for development)
- File sink (rolling daily, 30-day retention)
- PostgreSQL sink (for errors and warnings)
- Enrichers: FromLogContext, MachineName, ThreadId, Environment

**Log Levels:**

- Development: Information
- Production: Warning

### Swagger Configuration

Configure Swagger/OpenAPI with:

- API title: "B2B E-Commerce API"
- Version: "v1"
- Description: Include API overview and authentication instructions
- JWT authentication support (Bearer token)
- XML documentation comments enabled
- Example requests and responses
- Group endpoints by tags (Products, Orders, Customers, etc.)

### Health Checks

Implement health checks for:

- Database connectivity
- Redis connectivity
- External APIs (Paynet, ERP)
- Disk space
- Memory usage

Endpoints:

- /health (overall health)
- /health/ready (readiness probe)
- /health/live (liveness probe)

### CORS Configuration

Configure CORS for frontend:

- Allow origins: localhost:3000, production domain
- Allow methods: GET, POST, PUT, DELETE, PATCH
- Allow headers: Content-Type, Authorization
- Allow credentials: true

### Rate Limiting

Implement rate limiting:

- Per IP: 1000 requests per minute
- Per user: 500 requests per minute
- Per endpoint (special cases):
  - Login: 5 requests per minute
  - Register: 3 requests per minute

### Performance Optimization

1. **Caching Strategy:**
   - Product catalog: 1 hour
   - Categories/Brands: 2 hours
   - Customer data: 30 minutes
   - System configuration: 24 hours

2. **Query Optimization:**
   - Use AsNoTracking() for read-only queries
   - Implement pagination for all list endpoints
   - Use Select() projections to return only needed fields
   - Implement database indexes on frequently queried columns

3. **Response Compression:**
   - Enable Gzip compression for responses > 1KB

### Security Headers

Configure security headers:

- X-Content-Type-Options: nosniff
- X-Frame-Options: DENY
- X-XSS-Protection: 1; mode=block
- Strict-Transport-Security: max-age=31536000
- Content-Security-Policy: default-src 'self'

### Testing Requirements

Create unit tests for:

- All domain entities and their business logic
- All application services
- All repository implementations
- All API controllers

Create integration tests for:

- Complete API workflows (register, login, create order, etc.)
- Database operations
- External service integrations

### NuGet Packages Required

**Domain Layer:**

- No external packages

**Application Layer:**

- AutoMapper.Extensions.Microsoft.DependencyInjection
- FluentValidation.DependencyInjectionExtensions
- Microsoft.Extensions.Logging.Abstractions

**Infrastructure Layer:**

- Microsoft.EntityFrameworkCore
- Npgsql.EntityFrameworkCore.PostgreSQL
- Microsoft.AspNetCore.Identity.EntityFrameworkCore
- StackExchange.Redis
- AWSSDK.S3
- Hangfire.AspNetCore
- Hangfire.PostgreSql
- Serilog.AspNetCore
- Serilog.Sinks.PostgreSQL

**API Layer:**

- Microsoft.AspNetCore.Authentication.JwtBearer
- Swashbuckle.AspNetCore
- Microsoft.AspNetCore.Mvc.Versioning
- AspNetCoreRateLimit

## Implementation Steps

1. **Create Solution and Projects:**
   - Create blank solution
   - Add all four projects (Domain, Application, Infrastructure, API)
   - Set up project references correctly

2. **Implement Domain Layer:**
   - Create all entities with proper validation
   - Implement value objects
   - Define enums
   - Create domain services
   - Define domain exceptions

3. **Implement Application Layer:**
   - Define all repository interfaces
   - Define all service interfaces
   - Create DTOs
   - Implement services with business logic
   - Create validators
   - Configure AutoMapper profiles

4. **Implement Infrastructure Layer:**
   - Configure DbContext
   - Create entity configurations
   - Implement repositories
   - Implement Unit of Work
   - Implement external services
   - Configure background jobs

5. **Implement API Layer:**
   - Configure Program.cs
   - Implement controllers
   - Configure authentication/authorization
   - Set up middleware
   - Configure Swagger
   - Add health checks

6. **Testing:**
   - Write unit tests
   - Write integration tests
   - Perform manual testing with Swagger

7. **Documentation:**
   - Generate Swagger documentation
   - Create README with setup instructions
   - Document API endpoints

## Business Rules to Implement

From the project specifications, ensure these business rules are enforced:

1. **Order Approval Workflow:**
   - Orders start in Pending status
   - Admin can approve or reject orders
   - Approved orders reserve stock and update customer credit
   - Rejected orders release any reserved resources

2. **Credit Management:**
   - Validate customer has sufficient credit before order creation
   - Update used credit on order approval
   - Release credit on order cancellation
   - Alert when customer exceeds 90% of credit limit

3. **Pricing Rules:**
   - Apply tier pricing based on customer's price list
   - Apply special pricing if exists for customer-product combination
   - Convert prices to customer's preferred currency
   - Lock exchange rate at order approval

4. **Stock Management:**
   - Validate stock availability before order creation
   - Reserve stock on order approval
   - Release stock on order cancellation
   - Track serial numbers for serialized products

5. **Tax Calculation:**
   - Calculate tax based on product tax rate
   - Support different tax rates per product category

6. **Discount Rules:**
   - Support percentage and fixed amount discounts
   - Support order-level and item-level discounts
   - Validate discount eligibility

## Sample Data / Seed Data

Create seed data for development:

- 5 product categories
- 10 brands
- 50 products with images and pricing
- 3 admin users
- 10 customer accounts (5 approved, 5 pending)
- Sample orders in different statuses
- System configurations (tax rates, currencies, etc.)

## Environment Variables

Configure these environment variables:

```
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=User Id=postgres.imtvvmzdyujmvijrgbff;Password=[YOUR_PASSWORD];Server=aws-1-eu-central-1.pooler.supabase.com;Port=6543;Database=postgres
ConnectionStrings__Redis=localhost:6379
JWT__Key=your-super-secret-key-min-32-characters
JWT__Issuer=b2b-commerce-api
JWT__Audience=b2b-commerce-client
JWT__ExpirationMinutes=15
Paynet__BaseUrl=https://api.paynet.com
Paynet__MerchantId=***
Paynet__MerchantKey=***
AWS__Region=eu-west-1
AWS__S3__BucketName=b2b-commerce-storage
Email__SmtpHost=smtp.gmail.com
Email__SmtpPort=587
Email__Username=***
Email__Password=***
Email__FromEmail=noreply@b2bcommerce.com
```

## Success Criteria

The implementation is complete when:

1. ✅ All four layers are created with correct dependencies
2. ✅ All entities and value objects are implemented
3. ✅ All repository and service interfaces are defined
4. ✅ All services implement business logic correctly
5. ✅ Database migrations are created and can be applied
6. ✅ All API controllers are implemented with proper endpoints
7. ✅ Authentication and authorization work correctly
8. ✅ Swagger documentation is complete and functional
9. ✅ Validation works on all endpoints
10. ✅ Global exception handling catches all errors
11. ✅ Logging captures all important events
12. ✅ Unit tests pass for all layers
13. ✅ Integration tests pass for critical workflows
14. ✅ Application runs without errors
15. ✅ Can successfully: register customer, login, browse products, create
    order, approve order

## Additional Notes

- Follow C# naming conventions and coding standards
- Use async/await consistently for all I/O operations
- Implement proper disposal of resources
- Use dependency injection throughout
- Keep controllers thin - business logic belongs in services
- Use DTOs for all API inputs and outputs (never expose entities directly)
- Implement proper error messages for all validation failures
- Use strongly-typed configuration objects (IOptions pattern)
- Implement request/response logging for debugging
- Add TODO comments for features to be implemented later

## Reference Project Structure

Refer to the project knowledge documents for detailed implementation of each
component. Follow Clean Architecture principles strictly - never violate
dependency rules.

The Domain layer should have no dependencies. The Application layer should only
depend on Domain. The Infrastructure layer should depend on Application and
Domain. The API layer should depend on all other layers but only for DI
registration.

---

**This prompt should be provided to Claude Code to generate the complete B2B
E-Commerce Backend API project following Clean Architecture principles and all
specified requirements.**
