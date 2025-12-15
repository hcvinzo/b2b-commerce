# API Layer Guide

## Overview

The API layer handles HTTP concerns: controllers, middleware, authentication, and request/response handling. It's the entry point for external consumers.

## Namespaces

```csharp
// B2B Portal API
namespace B2BCommerce.Backend.API.Controllers
namespace B2BCommerce.Backend.API.Middleware
namespace B2BCommerce.Backend.API.Filters
namespace B2BCommerce.Backend.API.Extensions

// Integration API (ERP Sync)
namespace B2BCommerce.Backend.IntegrationAPI.Controllers
namespace B2BCommerce.Backend.IntegrationAPI.Authentication
```

## Two API Projects

| Project | Purpose | Authentication | Consumers |
|---------|---------|----------------|-----------|
| `B2BCommerce.Backend.API` | Main B2B portal | JWT Bearer | Dealer portal, Admin panel |
| `B2BCommerce.Backend.IntegrationAPI` | ERP integration | API Key | LOGO ERP, External systems |

## Controller Guidelines

### Base Controller

```csharp
// File: API/Controllers/BaseApiController.cs
namespace B2BCommerce.Backend.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public abstract class BaseApiController : ControllerBase
    {
        protected int CurrentUserId => 
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        
        protected int CurrentCustomerId =>
            int.Parse(User.FindFirstValue("CustomerId") ?? "0");
        
        protected ActionResult HandleResult<T>(Result<T> result)
        {
            if (result.Success)
                return Ok(result.Data);
            
            if (result.Errors.Any())
                return BadRequest(new { Errors = result.Errors });
            
            return BadRequest(new { Error = result.Error });
        }
        
        protected ActionResult HandleResult(Result result)
        {
            return result.Success ? Ok() : BadRequest(new { Error = result.Error });
        }
    }
}
```

### Controller Implementation

```csharp
// File: API/Controllers/ProductsController.cs
namespace B2BCommerce.Backend.API.Controllers
{
    [Authorize]
    public class ProductsController : BaseApiController
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;
        
        public ProductsController(
            IProductService productService,
            ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }
        
        /// <summary>
        /// Get paginated list of products
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<ProductListItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedList<ProductListItemDto>>> GetProducts(
            [FromQuery] ProductQueryDto query)
        {
            var result = await _productService.GetPagedAsync(query);
            return Ok(result);
        }
        
        /// <summary>
        /// Get product by ID
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            
            if (product == null)
                return NotFound();
            
            return Ok(product);
        }
        
        /// <summary>
        /// Create a new product
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductDto>> CreateProduct(
            [FromBody] CreateProductDto dto)
        {
            var result = await _productService.CreateAsync(dto);
            
            if (!result.Success)
                return HandleResult(result);
            
            return CreatedAtAction(
                nameof(GetProduct), 
                new { id = result.Data!.Id }, 
                result.Data);
        }
        
        /// <summary>
        /// Update an existing product
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductDto>> UpdateProduct(
            int id, 
            [FromBody] UpdateProductDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new { Error = "ID mismatch" });
            
            var result = await _productService.UpdateAsync(dto);
            return HandleResult(result);
        }
        
        /// <summary>
        /// Delete a product (soft delete)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _productService.DeleteAsync(id);
            
            if (!result.Success)
                return NotFound();
            
            return NoContent();
        }
    }
}
```

### Using MediatR in Controllers

```csharp
// File: API/Controllers/ProductsController.cs (MediatR version)
namespace B2BCommerce.Backend.API.Controllers
{
    [Authorize]
    public class ProductsController : BaseApiController
    {
        private readonly IMediator _mediator;
        
        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var result = await _mediator.Send(new GetProductByIdQuery(id));
            return result != null ? Ok(result) : NotFound();
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductDto>> CreateProduct(
            [FromBody] CreateProductCommand command)
        {
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }
    }
}
```

## Integration API Controllers

```csharp
// File: IntegrationAPI/Controllers/IntegrationProductsController.cs
namespace B2BCommerce.Backend.IntegrationAPI.Controllers
{
    [ApiController]
    [Route("api/integration/v1/products")]
    [ApiKeyAuthorize]  // Custom API Key authentication
    public class IntegrationProductsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<IntegrationProductsController> _logger;
        
        public IntegrationProductsController(
            IMediator mediator,
            ILogger<IntegrationProductsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }
        
        /// <summary>
        /// Sync product from ERP (upsert by external code)
        /// </summary>
        [HttpPost("sync")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductDto>> SyncProduct(
            [FromBody] SyncProductDto dto)
        {
            _logger.LogInformation(
                "Product sync request: {ExternalCode}", dto.ExternalCode);
            
            var command = new UpsertProductCommand(
                dto.ExternalCode,
                dto.Name,
                dto.Sku,
                dto.CategoryId,
                dto.BrandId,
                dto.ListPrice,
                dto.DealerPrice,
                dto.Currency,
                dto.ExternalId,
                dto.Description);
            
            var result = await _mediator.Send(command);
            
            if (!result.Success)
                return BadRequest(new { Error = result.Error });
            
            return Ok(result.Data);
        }
        
        /// <summary>
        /// Bulk sync products from ERP
        /// </summary>
        [HttpPost("sync/bulk")]
        [ProducesResponseType(typeof(BulkSyncResultDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<BulkSyncResultDto>> BulkSyncProducts(
            [FromBody] List<SyncProductDto> products)
        {
            var results = new BulkSyncResultDto();
            
            foreach (var dto in products)
            {
                try
                {
                    var command = new UpsertProductCommand(
                        dto.ExternalCode,
                        dto.Name,
                        dto.Sku,
                        dto.CategoryId,
                        dto.BrandId,
                        dto.ListPrice,
                        dto.DealerPrice);
                    
                    var result = await _mediator.Send(command);
                    
                    if (result.Success)
                        results.Succeeded.Add(dto.ExternalCode);
                    else
                        results.Failed.Add(new FailedSyncItem(dto.ExternalCode, result.Error!));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to sync product {ExternalCode}", dto.ExternalCode);
                    results.Failed.Add(new FailedSyncItem(dto.ExternalCode, ex.Message));
                }
            }
            
            return Ok(results);
        }
    }
}
```

## Authentication Configuration

### JWT Authentication (B2B API)

```csharp
// File: API/Extensions/AuthenticationExtensions.cs
namespace B2BCommerce.Backend.API.Extensions
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
            
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            context.Response.Headers.Append(
                                "Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });
            
            return services;
        }
    }
}
```

### API Key Authentication (Integration API)

```csharp
// File: IntegrationAPI/Authentication/ApiKeyAuthenticationHandler.cs
namespace B2BCommerce.Backend.IntegrationAPI.Authentication
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private const string ApiKeyHeaderName = "X-API-Key";
        private readonly IConfiguration _configuration;
        
        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IConfiguration configuration)
            : base(options, logger, encoder)
        {
            _configuration = configuration;
        }
        
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeader))
            {
                return Task.FromResult(
                    AuthenticateResult.Fail("API Key header not found"));
            }
            
            var providedApiKey = apiKeyHeader.ToString();
            var validApiKey = _configuration["IntegrationApi:ApiKey"];
            
            if (string.IsNullOrEmpty(validApiKey) || providedApiKey != validApiKey)
            {
                return Task.FromResult(
                    AuthenticateResult.Fail("Invalid API Key"));
            }
            
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "IntegrationClient"),
                new Claim(ClaimTypes.Role, "Integration")
            };
            
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
    
    // Attribute for controllers
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAuthorizeAttribute : AuthorizeAttribute
    {
        public ApiKeyAuthorizeAttribute()
        {
            AuthenticationSchemes = "ApiKey";
        }
    }
}
```

## Middleware

### Exception Handling Middleware

```csharp
// File: API/Middleware/ExceptionHandlingMiddleware.cs
namespace B2BCommerce.Backend.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        
        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }
        
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, message) = exception switch
            {
                ValidationException validationEx => 
                    (StatusCodes.Status400BadRequest, 
                     new { Errors = validationEx.Errors }),
                     
                NotFoundException notFoundEx => 
                    (StatusCodes.Status404NotFound, 
                     new { Error = notFoundEx.Message }),
                     
                UnauthorizedAccessException => 
                    (StatusCodes.Status401Unauthorized, 
                     new { Error = "Unauthorized" }),
                     
                DomainException domainEx => 
                    (StatusCodes.Status400BadRequest, 
                     new { Error = domainEx.Message }),
                     
                _ => (StatusCodes.Status500InternalServerError, 
                      new { Error = "An unexpected error occurred" } as object)
            };
            
            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, "Unhandled exception occurred");
            }
            else
            {
                _logger.LogWarning(exception, "Request failed: {Message}", exception.Message);
            }
            
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            
            await context.Response.WriteAsJsonAsync(message);
        }
    }
}
```

### Request Logging Middleware

```csharp
// File: API/Middleware/RequestLoggingMiddleware.cs
namespace B2BCommerce.Backend.API.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        
        public RequestLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        
        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                
                _logger.LogInformation(
                    "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
```

## Program.cs Configuration

### B2B API Program.cs

```csharp
// File: API/Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "B2BCommerce API",
        Version = "v1",
        Description = "B2B E-Commerce Platform API"
    });
    
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

// Build app
var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

## HTTP Status Code Guidelines

| Status Code | Use Case |
|-------------|----------|
| 200 OK | Successful GET, PUT |
| 201 Created | Successful POST (with Location header) |
| 204 No Content | Successful DELETE |
| 400 Bad Request | Validation errors, business rule violations |
| 401 Unauthorized | Missing or invalid authentication |
| 403 Forbidden | Authenticated but not authorized |
| 404 Not Found | Resource doesn't exist |
| 409 Conflict | Duplicate resource (e.g., SKU already exists) |
| 500 Internal Server Error | Unexpected errors |

## API Versioning

```csharp
// URL versioning: /api/v1/products
[Route("api/v1/[controller]")]

// Later: /api/v2/products with breaking changes
[Route("api/v2/[controller]")]
```

## Rules Summary

| Rule | Description |
|------|-------------|
| Thin controllers | Only HTTP concerns, delegate to services |
| Return DTOs | Never return domain entities |
| Use proper status codes | Follow REST conventions |
| Document endpoints | Use XML comments and Swagger attributes |
| Handle errors globally | Use exception middleware |
| Log requests | Track performance and issues |
| Validate early | Use FluentValidation with filters |

---

**Next**: [07-Coding-Standards](07-Coding-Standards.md)
