using B2BCommerce.Backend.Application;
using B2BCommerce.Backend.Infrastructure;
using B2BCommerce.Backend.IntegrationAPI.Authentication;
using B2BCommerce.Backend.IntegrationAPI.Middleware;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .CreateLogger();

try
{
    Log.Information("Starting B2B Commerce Integration API");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddControllers();

    // Add Application services (AutoMapper, FluentValidation)
    builder.Services.AddApplication();

    // Add Infrastructure services (DbContext, Repositories, Identity, Services)
    builder.Services.AddInfrastructure(builder.Configuration);

    // Override Identity cookie behavior to return 401 instead of redirecting to login page
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

    // Configure API Key settings
    builder.Services.Configure<ApiKeySettings>(
        builder.Configuration.GetSection("ApiKeySettings"));

    // Add API Key Authentication - must set all default schemes to override Identity's defaults
    builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = ApiKeyAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = ApiKeyAuthenticationDefaults.AuthenticationScheme;
            options.DefaultScheme = ApiKeyAuthenticationDefaults.AuthenticationScheme;
        })
        .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
            ApiKeyAuthenticationDefaults.AuthenticationScheme,
            options => { });

    // Add Authorization with scope-based policies
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("categories:read", policy =>
            policy.RequireClaim("scope", "categories:read", "categories:*", "*"));
        options.AddPolicy("categories:write", policy =>
            policy.RequireClaim("scope", "categories:write", "categories:*", "*"));
        options.AddPolicy("products:read", policy =>
            policy.RequireClaim("scope", "products:read", "products:*", "*"));
        options.AddPolicy("products:write", policy =>
            policy.RequireClaim("scope", "products:write", "products:*", "*"));
        options.AddPolicy("orders:read", policy =>
            policy.RequireClaim("scope", "orders:read", "orders:*", "*"));
        options.AddPolicy("orders:write", policy =>
            policy.RequireClaim("scope", "orders:write", "orders:*", "*"));
        options.AddPolicy("customers:read", policy =>
            policy.RequireClaim("scope", "customers:read", "customers:*", "*"));
        options.AddPolicy("customers:write", policy =>
            policy.RequireClaim("scope", "customers:write", "customers:*", "*"));
        options.AddPolicy("attributes:read", policy =>
            policy.RequireClaim("scope", "attributes:read", "attributes:*", "*"));
        options.AddPolicy("attributes:write", policy =>
            policy.RequireClaim("scope", "attributes:write", "attributes:*", "*"));
    });

    // Add API Explorer and Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
        {
            Title = "B2B E-Commerce Integration API",
            Version = "v1",
            Description = @"External Integration API for B2B E-Commerce Platform.

## Authentication
All endpoints require API Key authentication via the `X-API-Key` header.

## Scopes
API keys are assigned scopes that control access:
- `categories:read` - Read category data
- `categories:write` - Create, update, delete categories
- `products:read` - Read product data
- `products:write` - Create, update, delete products
- `orders:read` - Read order data
- `orders:write` - Create, update orders
- `customers:read` - Read customer data
- `customers:write` - Create, update customers
- `attributes:read` - Read attribute definitions
- `attributes:write` - Create, update, delete attributes
- `*` - Full access to all endpoints

## Rate Limiting
Each API key has a configured rate limit (requests per minute).

## Response Format
All responses follow a standard format:
```json
{
  ""success"": true,
  ""data"": { },
  ""message"": ""optional message""
}
```",
            Contact = new Microsoft.OpenApi.OpenApiContact
            {
                Name = "B2B Commerce Team"
            },
            License = new Microsoft.OpenApi.OpenApiLicense
            {
                Name = "Proprietary"
            }
        });

        // Add API Key Authentication to Swagger
        options.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.OpenApiSecurityScheme
        {
            Description = "API Key authentication. Enter your API key in the text box below.",
            Name = "X-API-Key",
            In = Microsoft.OpenApi.ParameterLocation.Header,
            Type = Microsoft.OpenApi.SecuritySchemeType.ApiKey,
            Scheme = "ApiKey"
        });

        options.AddSecurityRequirement(doc =>
        {
            var requirement = new Microsoft.OpenApi.OpenApiSecurityRequirement();
            var schemeReference = new Microsoft.OpenApi.OpenApiSecuritySchemeReference("ApiKey", doc);
            requirement[schemeReference] = new List<string>();
            return requirement;
        });

        // Include XML documentation
        var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }

        // Custom schema IDs to avoid conflicts
        options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
    });

    // Add Health Checks
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<B2BCommerce.Backend.Infrastructure.Data.ApplicationDbContext>();

    var app = builder.Build();

    // Configure the HTTP request pipeline

    // Global exception handling - must be first in pipeline
    app.UseExceptionHandling();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "B2B E-Commerce Integration API v1");
            options.RoutePrefix = "swagger";
        });
    }

    // Use Serilog request logging
    app.UseSerilogRequestLogging();

    // Add API usage logging middleware
    app.UseMiddleware<ApiUsageLoggingMiddleware>();

    app.UseHttpsRedirection();

    // Use Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Map Controllers
    app.MapControllers();

    // Map Health Checks
    app.MapHealthChecks("/health");

    Log.Information("B2B Commerce Integration API started successfully");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
