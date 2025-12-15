# Coding Standards

## Overview

This document defines the coding standards and conventions for the B2BCommerce backend. Following these standards ensures consistency and maintainability across the codebase.

## Naming Conventions

### General Rules

| Element | Convention | Example |
|---------|------------|---------|
| Namespaces | PascalCase | `B2BCommerce.Backend.Domain.Entities` |
| Classes | PascalCase | `ProductService`, `OrderRepository` |
| Interfaces | IPascalCase | `IProductService`, `IOrderRepository` |
| Methods | PascalCase | `GetByIdAsync`, `CreateOrder` |
| Properties | PascalCase | `FirstName`, `OrderNumber` |
| Public fields | PascalCase | `MaxRetryCount` |
| Private fields | _camelCase | `_productRepository` |
| Parameters | camelCase | `productId`, `customerName` |
| Local variables | camelCase | `orderTotal`, `isValid` |
| Constants | PascalCase | `DefaultPageSize`, `MaxFileSize` |

### Async Methods

Always suffix async methods with `Async`.

```csharp
// ✅ GOOD
public async Task<Product> GetByIdAsync(int id)
public async Task CreateAsync(Product product)

// ❌ BAD
public async Task<Product> GetById(int id)
public async Task Create(Product product)
```

### Boolean Properties/Methods

Use `Is`, `Has`, `Can`, `Should` prefixes for booleans.

```csharp
// ✅ GOOD
public bool IsActive { get; set; }
public bool HasDiscount { get; set; }
public bool CanBeOrdered()
public bool ShouldSendNotification()

// ❌ BAD
public bool Active { get; set; }
public bool Discount { get; set; }
```

### Collection Properties

Use plural names for collections.

```csharp
// ✅ GOOD
public ICollection<OrderItem> Items { get; set; }
public List<Product> Products { get; set; }

// ❌ BAD
public ICollection<OrderItem> ItemCollection { get; set; }
public List<Product> ProductList { get; set; }
```

## Code Organization

### File Structure

One class per file, filename matches class name.

```
ProductService.cs → contains ProductService class
IProductService.cs → contains IProductService interface
```

### Using Statements

Order using statements:
1. System namespaces
2. Microsoft namespaces
3. Third-party namespaces
4. Project namespaces

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using AutoMapper;
using FluentValidation;

using B2BCommerce.Backend.Domain.Entities;
using B2BCommerce.Backend.Application.DTOs;
```

### Class Organization

Order class members:
1. Constants
2. Private fields
3. Constructors
4. Public properties
5. Public methods
6. Private methods

```csharp
public class ProductService : IProductService
{
    // 1. Constants
    private const int DefaultPageSize = 20;
    
    // 2. Private fields
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;
    
    // 3. Constructors
    public ProductService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }
    
    // 4. Public properties (if any)
    
    // 5. Public methods
    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        return MapToDto(product);
    }
    
    // 6. Private methods
    private ProductDto? MapToDto(Product? product)
    {
        return product != null ? _mapper.Map<ProductDto>(product) : null;
    }
}
```

## Formatting

### Braces

Always use braces, even for single-line statements.

```csharp
// ✅ GOOD
if (product == null)
{
    return NotFound();
}

// ❌ BAD
if (product == null)
    return NotFound();
```

### Line Length

Keep lines under 120 characters. Break long method calls and parameters.

```csharp
// ✅ GOOD
var product = Product.CreateFromExternal(
    externalCode: dto.ExternalCode,
    name: dto.Name,
    sku: dto.Sku,
    categoryId: dto.CategoryId,
    brandId: dto.BrandId,
    listPrice: dto.ListPrice,
    dealerPrice: dto.DealerPrice);

// ❌ BAD
var product = Product.CreateFromExternal(dto.ExternalCode, dto.Name, dto.Sku, dto.CategoryId, dto.BrandId, dto.ListPrice, dto.DealerPrice);
```

### Spacing

Use single blank lines to separate logical groups.

```csharp
public async Task<Result<ProductDto>> CreateAsync(CreateProductDto dto)
{
    // Validation
    var validationResult = await _validator.ValidateAsync(dto);
    if (!validationResult.IsValid)
    {
        return Result<ProductDto>.Fail(validationResult.Errors);
    }
    
    // Create entity
    var product = Product.Create(dto.Name, dto.Sku, dto.CategoryId);
    
    // Save
    await _unitOfWork.Products.AddAsync(product);
    await _unitOfWork.SaveChangesAsync();
    
    // Return result
    return Result<ProductDto>.Ok(_mapper.Map<ProductDto>(product));
}
```

## Null Handling

### Nullable Reference Types

Enable nullable reference types and handle nulls explicitly.

```csharp
// Nullable types
public string? Description { get; set; }
public Product? GetByIdAsync(int id)

// Non-nullable types
public string Name { get; set; } = null!;
public Product GetRequiredByIdAsync(int id)
```

### Null Checking

Use pattern matching for null checks.

```csharp
// ✅ GOOD
if (product is null)
    return NotFound();

if (product is not null)
    DoSomething(product);

// ❌ BAD
if (product == null)
    return NotFound();
```

### Null Coalescing

Use null coalescing operators where appropriate.

```csharp
// Null coalescing
var name = dto.Name ?? "Default";

// Null conditional
var categoryName = product?.Category?.Name;

// Null coalescing assignment
_products ??= new List<Product>();
```

## Async/Await

### Always Await

Never use `.Result` or `.Wait()` - always use `async/await`.

```csharp
// ✅ GOOD
public async Task<Product> GetByIdAsync(int id)
{
    return await _repository.GetByIdAsync(id);
}

// ❌ BAD - Deadlock risk!
public Product GetById(int id)
{
    return _repository.GetByIdAsync(id).Result;
}
```

### ConfigureAwait

In library code (Domain, Application), use `ConfigureAwait(false)`.

```csharp
// In Application layer
public async Task<ProductDto> GetByIdAsync(int id)
{
    var product = await _repository.GetByIdAsync(id).ConfigureAwait(false);
    return _mapper.Map<ProductDto>(product);
}
```

### Async All the Way

Don't mix sync and async code.

```csharp
// ✅ GOOD
public async Task<List<Product>> GetAllAsync()
{
    return await _context.Products.ToListAsync();
}

// ❌ BAD
public List<Product> GetAll()
{
    return _context.Products.ToListAsync().Result;
}
```

## LINQ

### Prefer Method Syntax

Use method syntax for consistency.

```csharp
// ✅ GOOD
var activeProducts = products
    .Where(p => p.IsActive)
    .OrderBy(p => p.Name)
    .ToList();

// ❌ Avoid query syntax
var activeProducts = (from p in products
                      where p.IsActive
                      orderby p.Name
                      select p).ToList();
```

### Use Projection

Select only needed properties.

```csharp
// ✅ GOOD
var productNames = await _context.Products
    .Where(p => p.IsActive)
    .Select(p => new { p.Id, p.Name })
    .ToListAsync();

// ❌ BAD - loads entire entity
var productNames = (await _context.Products
    .Where(p => p.IsActive)
    .ToListAsync())
    .Select(p => new { p.Id, p.Name });
```

## Exception Handling

### Specific Exceptions

Catch specific exceptions, not general `Exception`.

```csharp
// ✅ GOOD
try
{
    await _unitOfWork.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException ex)
{
    _logger.LogWarning(ex, "Concurrency conflict");
    throw new ConflictException("The record was modified by another user");
}
catch (DbUpdateException ex)
{
    _logger.LogError(ex, "Database update failed");
    throw;
}

// ❌ BAD
try
{
    await _unitOfWork.SaveChangesAsync();
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error");
    throw;
}
```

### Don't Swallow Exceptions

Always log or rethrow exceptions.

```csharp
// ✅ GOOD
catch (Exception ex)
{
    _logger.LogError(ex, "Operation failed");
    throw;
}

// ❌ BAD - swallows exception
catch (Exception)
{
    // Silent failure
}
```

## Logging

### Log Levels

| Level | Use Case |
|-------|----------|
| Trace | Detailed debugging information |
| Debug | Development debugging |
| Information | Normal application flow |
| Warning | Unexpected events that don't stop execution |
| Error | Errors that prevent operation completion |
| Critical | System failures |

### Structured Logging

Use structured logging with message templates.

```csharp
// ✅ GOOD - Structured logging
_logger.LogInformation(
    "Product created: {ProductId} - {Sku}", 
    product.Id, 
    product.Sku);

_logger.LogError(
    ex, 
    "Failed to create product {Sku} for customer {CustomerId}", 
    dto.Sku, 
    customerId);

// ❌ BAD - String interpolation
_logger.LogInformation($"Product created: {product.Id} - {product.Sku}");
```

### Sensitive Data

Never log sensitive data (passwords, tokens, PII).

```csharp
// ✅ GOOD
_logger.LogInformation("User {UserId} logged in", user.Id);

// ❌ BAD
_logger.LogInformation("User {Email} logged in with password {Password}", email, password);
```

## Comments and Documentation

### XML Documentation

Document all public APIs.

```csharp
/// <summary>
/// Retrieves a product by its unique identifier.
/// </summary>
/// <param name="id">The product ID.</param>
/// <returns>The product DTO if found; otherwise, null.</returns>
/// <exception cref="ArgumentException">Thrown when id is less than 1.</exception>
public async Task<ProductDto?> GetByIdAsync(int id)
{
    if (id < 1)
        throw new ArgumentException("ID must be positive", nameof(id));
    
    var product = await _repository.GetByIdAsync(id);
    return _mapper.Map<ProductDto>(product);
}
```

### Inline Comments

Use comments sparingly for non-obvious logic.

```csharp
// ✅ GOOD - Explains why
// Skip stock validation for non-trackable products
if (!product.TrackStock)
    return true;

// ❌ BAD - States the obvious
// Get the product
var product = await _repository.GetByIdAsync(id);
```

## Dependency Injection

### Constructor Injection

Use constructor injection for required dependencies.

```csharp
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;
    
    public ProductService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
```

### Interface Dependencies

Depend on interfaces, not implementations.

```csharp
// ✅ GOOD
public ProductService(IUnitOfWork unitOfWork)

// ❌ BAD
public ProductService(UnitOfWork unitOfWork)
```

## Performance

### Use IEnumerable vs IList

Return `IEnumerable<T>` when iteration is all that's needed.

```csharp
// For single iteration
public IEnumerable<Product> GetActiveProducts()

// When count/index access is needed
public IList<Product> GetActiveProducts()
```

### Avoid Multiple Enumeration

Materialize queries before multiple access.

```csharp
// ✅ GOOD
var products = await query.ToListAsync();
if (products.Any())
{
    foreach (var product in products) { }
}

// ❌ BAD - Enumerates twice
if (query.Any())  // First enumeration
{
    foreach (var product in query) { } // Second enumeration
}
```

## Records and DTOs

### Use Records for DTOs

Prefer records for immutable DTOs.

```csharp
// ✅ GOOD
public record ProductDto(int Id, string Name, string Sku, decimal Price);

public record CreateProductCommand(
    string Name,
    string Sku,
    int CategoryId,
    decimal Price) : IRequest<Result<ProductDto>>;

// Also acceptable: class with init-only properties
public class ProductDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
}
```

## Quick Reference

| Do | Don't |
|----|-------|
| Use `async/await` | Use `.Result` or `.Wait()` |
| Use specific exceptions | Catch general `Exception` |
| Use structured logging | Use string interpolation in logs |
| Return DTOs from API | Return domain entities |
| Use constructor injection | Use service locator |
| Use nullable reference types | Ignore null warnings |
| Document public APIs | Over-comment obvious code |
| Use braces for all blocks | Skip braces for single lines |

---

**Next**: [08-Testing-Guide](08-Testing-Guide.md)
