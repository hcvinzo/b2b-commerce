using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Application.Interfaces.Services;
using B2BCommerce.Backend.Infrastructure.Data;
using B2BCommerce.Backend.Infrastructure.Data.Repositories;
using B2BCommerce.Backend.Infrastructure.Data.Repositories.Integration;
using B2BCommerce.Backend.Infrastructure.Identity;
using B2BCommerce.Backend.Infrastructure.Services;
using B2BCommerce.Backend.Infrastructure.Services.Integration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace B2BCommerce.Backend.Infrastructure;

/// <summary>
/// Infrastructure layer dependency injection configuration
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                }));

        // Identity
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IShipmentRepository, ShipmentRepository>();
        services.AddScoped<ICurrencyRateRepository, CurrencyRateRepository>();

        // Integration Repositories
        services.AddScoped<IApiClientRepository, ApiClientRepository>();
        services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
        services.AddScoped<IApiKeyUsageLogRepository, ApiKeyUsageLogRepository>();

        // Newsletter Repository
        services.AddScoped<INewsletterSubscriptionRepository, NewsletterSubscriptionRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Memory Cache (for ParameterService)
        services.AddMemoryCache();

        // Application Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ICustomerAttributeService, CustomerAttributeService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IRoleManagementService, RoleManagementService>();
        services.AddScoped<ICustomerUserService, CustomerUserService>();
        services.AddScoped<IGeoLocationTypeService, GeoLocationTypeService>();
        services.AddScoped<IGeoLocationService, GeoLocationService>();
        services.AddScoped<IParameterService, ParameterService>();

        // Integration Services
        services.AddScoped<IApiKeyGenerator, ApiKeyGenerator>();
        services.AddScoped<IApiClientService, ApiClientService>();
        services.AddScoped<IApiKeyService, ApiKeyService>();

        // Storage Service (AWS S3 with CloudFront)
        services.AddSingleton<IStorageService, S3StorageService>();

        return services;
    }
}
