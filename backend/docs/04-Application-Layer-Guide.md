# Application Layer Guide

## Overview

The Application layer orchestrates business logic by coordinating between domain entities, repositories, and external services. It defines service interfaces, DTOs, validation rules, and handles use case implementation.

## Namespace

```csharp
namespace B2BCommerce.Backend.Application.Services
namespace B2BCommerce.Backend.Application.DTOs
namespace B2BCommerce.Backend.Application.Interfaces
namespace B2BCommerce.Backend.Application.Mapping
namespace B2BCommerce.Backend.Application.Validators
namespace B2BCommerce.Backend.Application.Commands
namespace B2BCommerce.Backend.Application.Queries
namespace B2BCommerce.Backend.Application.EventHandlers
namespace B2BCommerce.Backend.Application.Exceptions
namespace B2BCommerce.Backend.Application.Common
```

## Common Interfaces

### IUnitOfWork

```csharp
// File: Application/Interfaces/IUnitOfWork.cs
namespace B2BCommerce.Backend.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Products { get; }
        ICategoryRepository Categories { get; }
        IBrandRepository Brands { get; }
        ICustomerRepository Customers { get; }
        IOrderRepository Orders { get; }
        IPaymentRepository Payments { get; }
        
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
```

### IGenericRepository

```csharp
// File: Application/Interfaces/Repositories/IGenericRepository.cs
namespace B2BCommerce.Backend.Application.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<bool> ExistsAsync(int id);
    }
}
```

### Specific Repository Interface

```csharp
// File: Application/Interfaces/Repositories/IProductRepository.cs
namespace B2BCommerce.Backend.Application.Interfaces.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<Product?> GetBySkuAsync(string sku);
        Task<Product?> GetByExternalCodeAsync(string externalCode);
        Task<Product?> GetByIdWithDetailsAsync(int id);
        Task<PaginatedList<Product>> GetPagedAsync(ProductQueryDto query);
        Task<bool> SkuExistsAsync(string sku, int? excludeId = null);
    }
}
```

## DTOs (Data Transfer Objects)

### Naming Conventions

| Type | Suffix | Example |
|------|--------|---------|
| Read/Response | `Dto` | `ProductDto` |
| Create | `Create[Entity]Dto` | `CreateProductDto` |
| Update | `Update[Entity]Dto` | `UpdateProductDto` |
| Query | `[Entity]QueryDto` | `ProductQueryDto` |
| List Item | `[Entity]ListItemDto` | `ProductListItemDto` |

### DTO Examples

```csharp
// File: Application/DTOs/Products/ProductDto.cs
namespace B2BCommerce.Backend.Application.DTOs.Products
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Sku { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal ListPrice { get; set; }
        public decimal DealerPrice { get; set; }
        public string Currency { get; set; } = null!;
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        
        public string CategoryName { get; set; } = null!;
        public string BrandName { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

// File: Application/DTOs/Products/CreateProductDto.cs
namespace B2BCommerce.Backend.Application.DTOs.Products
{
    public class CreateProductDto
    {
        public string Sku { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public decimal ListPrice { get; set; }
        public decimal DealerPrice { get; set; }
        public string Currency { get; set; } = "TRY";
        public bool TrackStock { get; set; } = true;
        public bool RequiresSerialNumber { get; set; }
        public int? WarrantyMonths { get; set; }
    }
}

// File: Application/DTOs/Products/UpdateProductDto.cs
namespace B2BCommerce.Backend.Application.DTOs.Products
{
    public class UpdateProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public decimal ListPrice { get; set; }
        public decimal DealerPrice { get; set; }
        public bool IsActive { get; set; }
    }
}

// File: Application/DTOs/Products/ProductQueryDto.cs
namespace B2BCommerce.Backend.Application.DTOs.Products
{
    public class ProductQueryDto
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public bool? IsActive { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
```

## Result Pattern

Use the Result pattern for explicit success/failure handling.

```csharp
// File: Application/Common/Result.cs
namespace B2BCommerce.Backend.Application.Common
{
    public class Result
    {
        public bool Success { get; protected set; }
        public string? Error { get; protected set; }
        public List<string> Errors { get; protected set; } = new();
        
        public static Result Ok() => new() { Success = true };
        public static Result Fail(string error) => new() { Success = false, Error = error };
        public static Result Fail(List<string> errors) => new() { Success = false, Errors = errors };
    }
    
    public class Result<T> : Result
    {
        public T? Data { get; private set; }
        
        public static Result<T> Ok(T data) => new() { Success = true, Data = data };
        public static new Result<T> Fail(string error) => new() { Success = false, Error = error };
        public static new Result<T> Fail(List<string> errors) => new() { Success = false, Errors = errors };
    }
}
```

## Validation with FluentValidation

### Validator Examples

```csharp
// File: Application/Validators/Products/CreateProductValidator.cs
namespace B2BCommerce.Backend.Application.Validators.Products
{
    public class CreateProductValidator : AbstractValidator<CreateProductDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        
        public CreateProductValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            
            RuleFor(x => x.Sku)
                .NotEmpty().WithMessage("SKU is required")
                .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters")
                .MustAsync(BeUniqueSku).WithMessage("SKU already exists");
            
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");
            
            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Category is required")
                .MustAsync(CategoryExists).WithMessage("Category not found");
            
            RuleFor(x => x.BrandId)
                .GreaterThan(0).WithMessage("Brand is required")
                .MustAsync(BrandExists).WithMessage("Brand not found");
            
            RuleFor(x => x.ListPrice)
                .GreaterThanOrEqualTo(0).WithMessage("List price cannot be negative");
            
            RuleFor(x => x.DealerPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Dealer price cannot be negative")
                .LessThanOrEqualTo(x => x.ListPrice)
                    .WithMessage("Dealer price cannot exceed list price");
            
            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required")
                .Length(3).WithMessage("Currency must be 3 characters")
                .Matches("^[A-Z]{3}$").WithMessage("Invalid currency code");
            
            RuleFor(x => x.WarrantyMonths)
                .GreaterThan(0).When(x => x.RequiresSerialNumber)
                .WithMessage("Warranty period required for serial tracking");
        }
        
        private async Task<bool> BeUniqueSku(string sku, CancellationToken ct)
        {
            return !await _unitOfWork.Products.SkuExistsAsync(sku);
        }
        
        private async Task<bool> CategoryExists(int categoryId, CancellationToken ct)
        {
            return await _unitOfWork.Categories.ExistsAsync(categoryId);
        }
        
        private async Task<bool> BrandExists(int brandId, CancellationToken ct)
        {
            return await _unitOfWork.Brands.ExistsAsync(brandId);
        }
    }
}
```

## AutoMapper Profiles

```csharp
// File: Application/Mapping/ProductMappingProfile.cs
namespace B2BCommerce.Backend.Application.Mapping
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            // Entity to DTO
            CreateMap<Product, ProductDto>()
                .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category.Name))
                .ForMember(d => d.BrandName, opt => opt.MapFrom(s => s.Brand.Name));
            
            CreateMap<Product, ProductListItemDto>();
            
            // DTO to Entity is handled by factory methods, not AutoMapper
        }
    }
}
```

## Service Implementation

### Service Interface

```csharp
// File: Application/Interfaces/Services/IProductService.cs
namespace B2BCommerce.Backend.Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<ProductDto?> GetByIdAsync(int id);
        Task<ProductDto?> GetBySkuAsync(string sku);
        Task<PaginatedList<ProductListItemDto>> GetPagedAsync(ProductQueryDto query);
        Task<Result<ProductDto>> CreateAsync(CreateProductDto dto);
        Task<Result<ProductDto>> UpdateAsync(UpdateProductDto dto);
        Task<Result> DeleteAsync(int id);
    }
}
```

### Service Implementation

```csharp
// File: Application/Services/ProductService.cs
namespace B2BCommerce.Backend.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateProductDto> _createValidator;
        private readonly IValidator<UpdateProductDto> _updateValidator;
        private readonly ILogger<ProductService> _logger;
        
        public ProductService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateProductDto> createValidator,
            IValidator<UpdateProductDto> updateValidator,
            ILogger<ProductService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }
        
        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdWithDetailsAsync(id);
            return product != null ? _mapper.Map<ProductDto>(product) : null;
        }
        
        public async Task<ProductDto?> GetBySkuAsync(string sku)
        {
            var product = await _unitOfWork.Products.GetBySkuAsync(sku);
            return product != null ? _mapper.Map<ProductDto>(product) : null;
        }
        
        public async Task<PaginatedList<ProductListItemDto>> GetPagedAsync(ProductQueryDto query)
        {
            var products = await _unitOfWork.Products.GetPagedAsync(query);
            return _mapper.Map<PaginatedList<ProductListItemDto>>(products);
        }
        
        public async Task<Result<ProductDto>> CreateAsync(CreateProductDto dto)
        {
            // Validate
            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Result<ProductDto>.Fail(
                    validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }
            
            try
            {
                // Create entity using factory method
                var product = Product.Create(
                    dto.Name,
                    dto.Sku,
                    dto.CategoryId,
                    dto.BrandId,
                    dto.ListPrice,
                    dto.DealerPrice,
                    dto.Currency);
                
                await _unitOfWork.Products.AddAsync(product);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Product created: {ProductId} - {Sku}", 
                    product.Id, product.Sku);
                
                // Reload with navigation properties
                var created = await _unitOfWork.Products.GetByIdWithDetailsAsync(product.Id);
                return Result<ProductDto>.Ok(_mapper.Map<ProductDto>(created!));
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Domain validation failed for product creation");
                return Result<ProductDto>.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                throw;
            }
        }
        
        public async Task<Result<ProductDto>> UpdateAsync(UpdateProductDto dto)
        {
            // Validate
            var validationResult = await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Result<ProductDto>.Fail(
                    validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }
            
            var product = await _unitOfWork.Products.GetByIdAsync(dto.Id);
            if (product == null)
            {
                return Result<ProductDto>.Fail("Product not found");
            }
            
            try
            {
                // Use entity methods for updates
                product.UpdatePricing(dto.ListPrice, dto.DealerPrice);
                
                // Other updates...
                
                await _unitOfWork.SaveChangesAsync();
                
                var updated = await _unitOfWork.Products.GetByIdWithDetailsAsync(product.Id);
                return Result<ProductDto>.Ok(_mapper.Map<ProductDto>(updated!));
            }
            catch (DomainException ex)
            {
                return Result<ProductDto>.Fail(ex.Message);
            }
        }
        
        public async Task<Result> DeleteAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                return Result.Fail("Product not found");
            }
            
            product.Delete();
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Product deleted: {ProductId}", id);
            
            return Result.Ok();
        }
    }
}
```

## CQRS with MediatR

### Command Example

```csharp
// File: Application/Commands/Products/UpsertProductCommand.cs
namespace B2BCommerce.Backend.Application.Commands.Products
{
    public record UpsertProductCommand(
        string ExternalCode,
        string Name,
        string Sku,
        int CategoryId,
        int BrandId,
        decimal ListPrice,
        decimal DealerPrice,
        string Currency = "TRY",
        string? ExternalId = null,
        string? Description = null
    ) : IRequest<Result<ProductDto>>;
}

// File: Application/Commands/Products/UpsertProductCommandHandler.cs
namespace B2BCommerce.Backend.Application.Commands.Products
{
    public class UpsertProductCommandHandler 
        : IRequestHandler<UpsertProductCommand, Result<ProductDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpsertProductCommandHandler> _logger;
        
        public UpsertProductCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<UpsertProductCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }
        
        public async Task<Result<ProductDto>> Handle(
            UpsertProductCommand request, 
            CancellationToken cancellationToken)
        {
            try
            {
                var existingProduct = await _unitOfWork.Products
                    .GetByExternalCodeAsync(request.ExternalCode);
                
                Product product;
                
                if (existingProduct != null)
                {
                    // Update existing
                    existingProduct.UpdateFromExternal(
                        request.Name,
                        request.CategoryId,
                        request.BrandId,
                        request.ListPrice,
                        request.DealerPrice,
                        request.Currency,
                        request.Description);
                    
                    product = existingProduct;
                    _logger.LogInformation("Product updated via sync: {ExternalCode}", 
                        request.ExternalCode);
                }
                else
                {
                    // Create new
                    product = Product.CreateFromExternal(
                        request.ExternalCode,
                        request.Name,
                        request.Sku,
                        request.CategoryId,
                        request.BrandId,
                        request.ListPrice,
                        request.DealerPrice,
                        request.Currency,
                        request.ExternalId);
                    
                    await _unitOfWork.Products.AddAsync(product);
                    _logger.LogInformation("Product created via sync: {ExternalCode}", 
                        request.ExternalCode);
                }
                
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                var dto = _mapper.Map<ProductDto>(product);
                return Result<ProductDto>.Ok(dto);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Domain validation failed for product upsert");
                return Result<ProductDto>.Fail(ex.Message);
            }
        }
    }
}
```

### Query Example

```csharp
// File: Application/Queries/Products/GetProductByIdQuery.cs
namespace B2BCommerce.Backend.Application.Queries.Products
{
    public record GetProductByIdQuery(int Id) : IRequest<ProductDto?>;
    
    public class GetProductByIdQueryHandler 
        : IRequestHandler<GetProductByIdQuery, ProductDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        
        public GetProductByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        
        public async Task<ProductDto?> Handle(
            GetProductByIdQuery request, 
            CancellationToken cancellationToken)
        {
            var product = await _unitOfWork.Products.GetByIdWithDetailsAsync(request.Id);
            return product != null ? _mapper.Map<ProductDto>(product) : null;
        }
    }
}
```

## Domain Event Handlers

```csharp
// File: Application/EventHandlers/OrderSubmittedEventHandler.cs
namespace B2BCommerce.Backend.Application.EventHandlers
{
    public class OrderSubmittedEventHandler : INotificationHandler<OrderSubmittedEvent>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ILogger<OrderSubmittedEventHandler> _logger;
        
        public OrderSubmittedEventHandler(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            ILogger<OrderSubmittedEventHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _logger = logger;
        }
        
        public async Task Handle(
            OrderSubmittedEvent notification, 
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Order submitted: {OrderId} by Customer {CustomerId}", 
                notification.OrderId, notification.CustomerId);
            
            var order = await _unitOfWork.Orders.GetByIdWithDetailsAsync(notification.OrderId);
            if (order == null) return;
            
            // Send notification email
            await _emailService.SendOrderConfirmationAsync(order);
            
            // Additional processing...
        }
    }
}
```

## Application Exceptions

```csharp
// File: Application/Exceptions/NotFoundException.cs
namespace B2BCommerce.Backend.Application.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string entityName, object key)
            : base($"Entity '{entityName}' with key '{key}' was not found.")
        {
        }
    }
}

// File: Application/Exceptions/ValidationException.cs
namespace B2BCommerce.Backend.Application.Exceptions
{
    public class ValidationException : Exception
    {
        public IDictionary<string, string[]> Errors { get; }
        
        public ValidationException(IDictionary<string, string[]> errors)
            : base("One or more validation errors occurred.")
        {
            Errors = errors;
        }
        
        public ValidationException(IEnumerable<ValidationFailure> failures)
            : this(failures
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(g => g.Key, g => g.ToArray()))
        {
        }
    }
}
```

## Paginated List

```csharp
// File: Application/Common/PaginatedList.cs
namespace B2BCommerce.Backend.Application.Common
{
    public class PaginatedList<T>
    {
        public List<T> Items { get; }
        public int PageNumber { get; }
        public int TotalPages { get; }
        public int TotalCount { get; }
        
        public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = count;
            PageNumber = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        }
        
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
        
        public static async Task<PaginatedList<T>> CreateAsync(
            IQueryable<T> source, 
            int pageNumber, 
            int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return new PaginatedList<T>(items, count, pageNumber, pageSize);
        }
    }
}
```

## Rules Summary

| Rule | Description |
|------|-------------|
| Only reference Domain | No Infrastructure or API references |
| Use DTOs | Never expose domain entities |
| Validate input | Use FluentValidation |
| Use Result pattern | Explicit success/failure handling |
| Use factory methods | Don't create entities with `new` |
| Log appropriately | Information for actions, Warning for issues |
| Handle exceptions | Catch domain exceptions |

---

**Next**: [05-Infrastructure-Layer-Guide](05-Infrastructure-Layer-Guide.md)
