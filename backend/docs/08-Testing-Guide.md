# Testing Guide

## Overview

This guide covers testing strategies, patterns, and examples for the B2BCommerce backend. Testing is organized by layer, with each layer having specific testing approaches.

## Testing Stack

| Tool | Purpose |
|------|---------|
| xUnit | Test framework |
| Moq | Mocking library |
| FluentAssertions | Assertion library |
| Bogus | Fake data generation |
| Microsoft.EntityFrameworkCore.InMemory | In-memory database for tests |
| WebApplicationFactory | Integration testing |

## Test Project Structure

```
tests/
├── B2BCommerce.Backend.Domain.Tests/
│   ├── Entities/
│   │   ├── ProductTests.cs
│   │   └── OrderTests.cs
│   └── ValueObjects/
│       ├── MoneyTests.cs
│       └── EmailTests.cs
│
├── B2BCommerce.Backend.Application.Tests/
│   ├── Services/
│   │   ├── ProductServiceTests.cs
│   │   └── OrderServiceTests.cs
│   ├── Validators/
│   │   └── CreateProductValidatorTests.cs
│   └── Commands/
│       └── UpsertProductCommandHandlerTests.cs
│
├── B2BCommerce.Backend.Infrastructure.Tests/
│   ├── Repositories/
│   │   ├── ProductRepositoryTests.cs
│   │   └── OrderRepositoryTests.cs
│   └── Services/
│       └── TokenServiceTests.cs
│
└── B2BCommerce.Backend.API.Tests/
    ├── Controllers/
    │   └── ProductsControllerTests.cs
    └── Integration/
        └── ProductsEndpointTests.cs
```

## Naming Conventions

### Test Class Names

`{ClassUnderTest}Tests`

```csharp
public class ProductTests { }
public class ProductServiceTests { }
public class ProductRepositoryTests { }
```

### Test Method Names

`{MethodName}_{Scenario}_{ExpectedResult}`

```csharp
public void Create_WithValidData_CreatesProduct()
public void Create_WithNegativePrice_ThrowsDomainException()
public async Task GetByIdAsync_WithExistingId_ReturnsProduct()
public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
```

## Domain Layer Tests

### Entity Tests

Test factory methods, business rules, and behavior methods.

```csharp
// File: Domain.Tests/Entities/ProductTests.cs
namespace B2BCommerce.Backend.Domain.Tests.Entities
{
    public class ProductTests
    {
        [Fact]
        public void Create_WithValidData_CreatesProduct()
        {
            // Arrange
            var name = "Test Product";
            var sku = "TEST-001";
            var categoryId = 1;
            var brandId = 1;
            var listPrice = 100m;
            var dealerPrice = 80m;
            
            // Act
            var product = Product.Create(
                name, sku, categoryId, brandId, listPrice, dealerPrice);
            
            // Assert
            product.Should().NotBeNull();
            product.Name.Should().Be(name);
            product.Sku.Should().Be(sku);
            product.ListPrice.Should().Be(listPrice);
            product.DealerPrice.Should().Be(dealerPrice);
            product.IsActive.Should().BeTrue();
            product.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<ProductCreatedEvent>();
        }
        
        [Fact]
        public void Create_WithEmptySku_ThrowsDomainException()
        {
            // Arrange & Act
            var act = () => Product.Create(
                "Test", "", 1, 1, 100, 80);
            
            // Assert
            act.Should().Throw<DomainException>()
                .WithMessage("*SKU*required*");
        }
        
        [Fact]
        public void Create_WithDealerPriceExceedingListPrice_ThrowsDomainException()
        {
            // Act
            var act = () => Product.Create(
                "Test", "SKU-001", 1, 1, 
                listPrice: 80, 
                dealerPrice: 100);
            
            // Assert
            act.Should().Throw<DomainException>()
                .WithMessage("*Dealer price*exceed*list price*");
        }
        
        [Fact]
        public void ReserveStock_WithSufficientStock_DecreasesQuantity()
        {
            // Arrange
            var product = CreateTestProduct(stockQuantity: 10);
            
            // Act
            product.ReserveStock(3);
            
            // Assert
            product.StockQuantity.Should().Be(7);
            product.DomainEvents.Should().Contain(e => e is StockReservedEvent);
        }
        
        [Fact]
        public void ReserveStock_WithInsufficientStock_ThrowsException()
        {
            // Arrange
            var product = CreateTestProduct(stockQuantity: 5);
            
            // Act
            var act = () => product.ReserveStock(10);
            
            // Assert
            act.Should().Throw<InsufficientStockException>();
        }
        
        [Theory]
        [InlineData(true, 10, 5, true)]    // Active, has stock
        [InlineData(false, 10, 5, false)]   // Inactive
        [InlineData(true, 5, 10, false)]    // Insufficient stock
        [InlineData(true, 0, 0, true)]      // No stock tracking
        public void CanBeOrdered_ReturnsExpectedResult(
            bool isActive, 
            int stockQuantity, 
            int requestedQuantity,
            bool expected)
        {
            // Arrange
            var product = CreateTestProduct(isActive, stockQuantity);
            
            // Act
            var result = product.CanBeOrdered(requestedQuantity);
            
            // Assert
            result.Should().Be(expected);
        }
        
        private static Product CreateTestProduct(
            bool isActive = true,
            int stockQuantity = 10)
        {
            // Use reflection or internal factory for testing
            // This is a simplified example
            var product = Product.Create("Test", "SKU-001", 1, 1, 100, 80);
            // Set additional properties as needed
            return product;
        }
    }
}
```

### Value Object Tests

```csharp
// File: Domain.Tests/ValueObjects/MoneyTests.cs
namespace B2BCommerce.Backend.Domain.Tests.ValueObjects
{
    public class MoneyTests
    {
        [Fact]
        public void Create_WithValidData_CreatesMoney()
        {
            // Act
            var money = Money.Create(100.50m, "TRY");
            
            // Assert
            money.Amount.Should().Be(100.50m);
            money.Currency.Should().Be("TRY");
        }
        
        [Fact]
        public void Create_WithNegativeAmount_ThrowsDomainException()
        {
            // Act
            var act = () => Money.Create(-100, "TRY");
            
            // Assert
            act.Should().Throw<DomainException>()
                .WithMessage("*negative*");
        }
        
        [Fact]
        public void Add_WithSameCurrency_ReturnsSum()
        {
            // Arrange
            var money1 = Money.Create(100, "TRY");
            var money2 = Money.Create(50, "TRY");
            
            // Act
            var result = money1.Add(money2);
            
            // Assert
            result.Amount.Should().Be(150);
            result.Currency.Should().Be("TRY");
        }
        
        [Fact]
        public void Add_WithDifferentCurrency_ThrowsDomainException()
        {
            // Arrange
            var money1 = Money.Create(100, "TRY");
            var money2 = Money.Create(50, "USD");
            
            // Act
            var act = () => money1.Add(money2);
            
            // Assert
            act.Should().Throw<DomainException>()
                .WithMessage("*different currencies*");
        }
        
        [Fact]
        public void Equals_WithSameValues_ReturnsTrue()
        {
            // Arrange
            var money1 = Money.Create(100, "TRY");
            var money2 = Money.Create(100, "TRY");
            
            // Assert
            money1.Should().Be(money2);
            (money1 == money2).Should().BeTrue();
        }
    }
}
```

## Application Layer Tests

### Service Tests

Mock dependencies and test business logic.

```csharp
// File: Application.Tests/Services/ProductServiceTests.cs
namespace B2BCommerce.Backend.Application.Tests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IValidator<CreateProductDto>> _mockValidator;
        private readonly Mock<ILogger<ProductService>> _mockLogger;
        private readonly ProductService _service;
        
        public ProductServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockValidator = new Mock<IValidator<CreateProductDto>>();
            _mockLogger = new Mock<ILogger<ProductService>>();
            
            _service = new ProductService(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockValidator.Object,
                Mock.Of<IValidator<UpdateProductDto>>(),
                _mockLogger.Object);
        }
        
        [Fact]
        public async Task GetByIdAsync_WithExistingProduct_ReturnsProductDto()
        {
            // Arrange
            var productId = 1;
            var product = new Product { Id = productId, Name = "Test Product" };
            var productDto = new ProductDto { Id = productId, Name = "Test Product" };
            
            _mockUnitOfWork.Setup(u => u.Products.GetByIdWithDetailsAsync(productId))
                .ReturnsAsync(product);
            
            _mockMapper.Setup(m => m.Map<ProductDto>(product))
                .Returns(productDto);
            
            // Act
            var result = await _service.GetByIdAsync(productId);
            
            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(productId);
            result.Name.Should().Be("Test Product");
        }
        
        [Fact]
        public async Task GetByIdAsync_WithNonExistingProduct_ReturnsNull()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.Products.GetByIdWithDetailsAsync(999))
                .ReturnsAsync((Product?)null);
            
            // Act
            var result = await _service.GetByIdAsync(999);
            
            // Assert
            result.Should().BeNull();
        }
        
        [Fact]
        public async Task CreateAsync_WithValidData_ReturnsSuccessResult()
        {
            // Arrange
            var dto = new CreateProductDto
            {
                Name = "New Product",
                Sku = "NEW-001",
                CategoryId = 1,
                BrandId = 1,
                ListPrice = 100,
                DealerPrice = 80
            };
            
            _mockValidator.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult());
            
            _mockUnitOfWork.Setup(u => u.Products.AddAsync(It.IsAny<Product>()))
                .ReturnsAsync((Product p) => p);
            
            _mockUnitOfWork.Setup(u => u.Products.GetByIdWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(new Product { Id = 1, Name = dto.Name });
            
            _mockMapper.Setup(m => m.Map<ProductDto>(It.IsAny<Product>()))
                .Returns(new ProductDto { Id = 1, Name = dto.Name });
            
            // Act
            var result = await _service.CreateAsync(dto);
            
            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
        }
        
        [Fact]
        public async Task CreateAsync_WithInvalidData_ReturnsFailureResult()
        {
            // Arrange
            var dto = new CreateProductDto { Name = "" };
            
            var validationFailures = new List<ValidationFailure>
            {
                new("Name", "Name is required"),
                new("Sku", "SKU is required")
            };
            
            _mockValidator.Setup(v => v.ValidateAsync(dto, default))
                .ReturnsAsync(new ValidationResult(validationFailures));
            
            // Act
            var result = await _service.CreateAsync(dto);
            
            // Assert
            result.Success.Should().BeFalse();
            result.Errors.Should().HaveCount(2);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Never);
        }
    }
}
```

### Validator Tests

```csharp
// File: Application.Tests/Validators/CreateProductValidatorTests.cs
namespace B2BCommerce.Backend.Application.Tests.Validators
{
    public class CreateProductValidatorTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly CreateProductValidator _validator;
        
        public CreateProductValidatorTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _validator = new CreateProductValidator(_mockUnitOfWork.Object);
            
            // Setup default mocks
            _mockUnitOfWork.Setup(u => u.Products.SkuExistsAsync(It.IsAny<string>(), null))
                .ReturnsAsync(false);
            _mockUnitOfWork.Setup(u => u.Categories.ExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.Brands.ExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(true);
        }
        
        [Fact]
        public async Task Validate_WithValidDto_ReturnsValid()
        {
            // Arrange
            var dto = new CreateProductDto
            {
                Sku = "SKU-001",
                Name = "Test Product",
                CategoryId = 1,
                BrandId = 1,
                ListPrice = 100,
                DealerPrice = 80,
                Currency = "TRY"
            };
            
            // Act
            var result = await _validator.ValidateAsync(dto);
            
            // Assert
            result.IsValid.Should().BeTrue();
        }
        
        [Fact]
        public async Task Validate_WithEmptySku_ReturnsInvalid()
        {
            // Arrange
            var dto = new CreateProductDto
            {
                Sku = "",
                Name = "Test Product",
                CategoryId = 1,
                BrandId = 1,
                ListPrice = 100,
                DealerPrice = 80,
                Currency = "TRY"
            };
            
            // Act
            var result = await _validator.ValidateAsync(dto);
            
            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Sku");
        }
        
        [Fact]
        public async Task Validate_WithDuplicateSku_ReturnsInvalid()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.Products.SkuExistsAsync("EXISTING-SKU", null))
                .ReturnsAsync(true);
            
            var dto = new CreateProductDto
            {
                Sku = "EXISTING-SKU",
                Name = "Test Product",
                CategoryId = 1,
                BrandId = 1,
                ListPrice = 100,
                DealerPrice = 80,
                Currency = "TRY"
            };
            
            // Act
            var result = await _validator.ValidateAsync(dto);
            
            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => 
                e.PropertyName == "Sku" && 
                e.ErrorMessage.Contains("already exists"));
        }
        
        [Fact]
        public async Task Validate_WithDealerPriceExceedingListPrice_ReturnsInvalid()
        {
            // Arrange
            var dto = new CreateProductDto
            {
                Sku = "SKU-001",
                Name = "Test Product",
                CategoryId = 1,
                BrandId = 1,
                ListPrice = 80,
                DealerPrice = 100,
                Currency = "TRY"
            };
            
            // Act
            var result = await _validator.ValidateAsync(dto);
            
            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "DealerPrice");
        }
    }
}
```

## Infrastructure Layer Tests

### Repository Tests

Use in-memory database for repository tests.

```csharp
// File: Infrastructure.Tests/Repositories/ProductRepositoryTests.cs
namespace B2BCommerce.Backend.Infrastructure.Tests.Repositories
{
    public class ProductRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ProductRepository _repository;
        
        public ProductRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new ApplicationDbContext(options);
            _repository = new ProductRepository(_context);
            
            SeedTestData();
        }
        
        private void SeedTestData()
        {
            var category = new Category { Id = 1, Name = "Electronics" };
            var brand = new Brand { Id = 1, Name = "TestBrand" };
            
            _context.Categories.Add(category);
            _context.Brands.Add(brand);
            
            _context.Products.AddRange(
                CreateProduct(1, "SKU-001", "Product 1", true),
                CreateProduct(2, "SKU-002", "Product 2", true),
                CreateProduct(3, "SKU-003", "Inactive Product", false)
            );
            
            _context.SaveChanges();
        }
        
        private Product CreateProduct(int id, string sku, string name, bool isActive)
        {
            // Use reflection or test helpers to create products
            // This is simplified
            return new Product 
            { 
                Id = id, 
                Sku = sku, 
                Name = name, 
                IsActive = isActive,
                CategoryId = 1,
                BrandId = 1
            };
        }
        
        [Fact]
        public async Task GetBySkuAsync_WithExistingSku_ReturnsProduct()
        {
            // Act
            var result = await _repository.GetBySkuAsync("SKU-001");
            
            // Assert
            result.Should().NotBeNull();
            result!.Sku.Should().Be("SKU-001");
        }
        
        [Fact]
        public async Task GetBySkuAsync_WithNonExistingSku_ReturnsNull()
        {
            // Act
            var result = await _repository.GetBySkuAsync("NON-EXISTING");
            
            // Assert
            result.Should().BeNull();
        }
        
        [Fact]
        public async Task GetPagedAsync_WithActiveFilter_ReturnsOnlyActiveProducts()
        {
            // Arrange
            var query = new ProductQueryDto
            {
                IsActive = true,
                PageNumber = 1,
                PageSize = 10
            };
            
            // Act
            var result = await _repository.GetPagedAsync(query);
            
            // Assert
            result.Items.Should().HaveCount(2);
            result.Items.Should().OnlyContain(p => p.IsActive);
        }
        
        [Fact]
        public async Task SkuExistsAsync_WithExistingSku_ReturnsTrue()
        {
            // Act
            var result = await _repository.SkuExistsAsync("SKU-001");
            
            // Assert
            result.Should().BeTrue();
        }
        
        [Fact]
        public async Task SkuExistsAsync_WithExcludedId_ExcludesProduct()
        {
            // Act
            var result = await _repository.SkuExistsAsync("SKU-001", excludeId: 1);
            
            // Assert
            result.Should().BeFalse();
        }
        
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
```

## API Integration Tests

### Integration Test Setup

```csharp
// File: API.Tests/Integration/CustomWebApplicationFactory.cs
namespace B2BCommerce.Backend.API.Tests.Integration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext configuration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                
                if (descriptor != null)
                    services.Remove(descriptor);
                
                // Add in-memory database
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
                
                // Seed test data
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.EnsureCreated();
                SeedTestData(context);
            });
        }
        
        private static void SeedTestData(ApplicationDbContext context)
        {
            // Add test data
        }
    }
}
```

### Integration Tests

```csharp
// File: API.Tests/Integration/ProductsEndpointTests.cs
namespace B2BCommerce.Backend.API.Tests.Integration
{
    public class ProductsEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        
        public ProductsEndpointTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            
            // Add authentication header
            var token = GenerateTestToken();
            _client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }
        
        [Fact]
        public async Task GetProducts_ReturnsSuccessAndPaginatedList()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/products");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadFromJsonAsync<PaginatedList<ProductDto>>();
            content.Should().NotBeNull();
            content!.Items.Should().NotBeEmpty();
        }
        
        [Fact]
        public async Task GetProduct_WithExistingId_ReturnsProduct()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/products/1");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var product = await response.Content.ReadFromJsonAsync<ProductDto>();
            product.Should().NotBeNull();
            product!.Id.Should().Be(1);
        }
        
        [Fact]
        public async Task GetProduct_WithNonExistingId_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/products/999");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        
        [Fact]
        public async Task CreateProduct_WithValidData_ReturnsCreated()
        {
            // Arrange
            var dto = new CreateProductDto
            {
                Sku = "NEW-SKU",
                Name = "New Product",
                CategoryId = 1,
                BrandId = 1,
                ListPrice = 100,
                DealerPrice = 80,
                Currency = "TRY"
            };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/products", dto);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
            
            var product = await response.Content.ReadFromJsonAsync<ProductDto>();
            product.Should().NotBeNull();
            product!.Sku.Should().Be("NEW-SKU");
        }
        
        [Fact]
        public async Task CreateProduct_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var dto = new CreateProductDto
            {
                Sku = "", // Invalid
                Name = "",
                CategoryId = 0
            };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/products", dto);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        
        private static string GenerateTestToken()
        {
            // Generate a test JWT token
            // This is a simplified example
            return "test-token";
        }
    }
}
```

## Test Helpers

### Fake Data Generator

```csharp
// File: Tests/Common/TestDataGenerator.cs
namespace B2BCommerce.Backend.Tests.Common
{
    public static class TestDataGenerator
    {
        private static readonly Faker _faker = new("en");
        
        public static CreateProductDto GenerateCreateProductDto() => new()
        {
            Sku = _faker.Commerce.Ean13(),
            Name = _faker.Commerce.ProductName(),
            Description = _faker.Commerce.ProductDescription(),
            CategoryId = _faker.Random.Int(1, 10),
            BrandId = _faker.Random.Int(1, 10),
            ListPrice = _faker.Random.Decimal(10, 1000),
            DealerPrice = _faker.Random.Decimal(5, 900),
            Currency = "TRY"
        };
        
        public static List<CreateProductDto> GenerateCreateProductDtos(int count) =>
            Enumerable.Range(0, count)
                .Select(_ => GenerateCreateProductDto())
                .ToList();
    }
}
```

## Test Coverage Requirements

| Layer | Minimum Coverage |
|-------|------------------|
| Domain | 90% |
| Application | 80% |
| Infrastructure | 70% |
| API | 60% |

## Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific project
dotnet test tests/B2BCommerce.Backend.Domain.Tests

# Run with filter
dotnet test --filter "FullyQualifiedName~ProductServiceTests"
```

## Rules Summary

| Rule | Description |
|------|-------------|
| Arrange-Act-Assert | Structure all tests with AAA pattern |
| One assertion per test | Keep tests focused |
| Mock dependencies | Use Moq for external dependencies |
| Use in-memory DB | For repository tests |
| Test edge cases | Include boundary conditions |
| Meaningful names | Clear test method names |

---

**End of Documentation**
