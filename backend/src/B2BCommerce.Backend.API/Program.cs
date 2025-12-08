using System.Text;
using B2BCommerce.Backend.Application;
using B2BCommerce.Backend.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .CreateLogger();

try
{
    Log.Information("Starting B2B Commerce Backend API");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddControllers();

    // Add Application services (AutoMapper, FluentValidation)
    builder.Services.AddApplication();

    // Add Infrastructure services (DbContext, Repositories, Identity, Services)
    builder.Services.AddInfrastructure(builder.Configuration);

    // Add Authentication
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not configured"));

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
    });

    // Add Authorization
    builder.Services.AddAuthorization();

    // Add CORS
    var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" };
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins(corsOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });

    // Add API Explorer and Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
        {
            Title = "B2B E-Commerce API",
            Version = "v1",
            Description = "RESTful API for B2B E-Commerce Platform with multi-tier pricing, credit management, and order approval workflow",
            Contact = new Microsoft.OpenApi.OpenApiContact
            {
                Name = "B2B Commerce Team"
            }
        });

        // Add JWT Authentication to Swagger
        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
            Name = "Authorization",
            In = Microsoft.OpenApi.ParameterLocation.Header,
            Type = Microsoft.OpenApi.SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(doc =>
        {
            var requirement = new Microsoft.OpenApi.OpenApiSecurityRequirement();
            var schemeReference = new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", doc);
            requirement[schemeReference] = new List<string>();
            return requirement;
        });
    });

    // Add Health Checks
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<B2BCommerce.Backend.Infrastructure.Data.ApplicationDbContext>();

    // Register DatabaseSeeder
    builder.Services.AddScoped<B2BCommerce.Backend.Infrastructure.Data.DatabaseSeeder>();

    var app = builder.Build();

    // Seed database in development
    if (app.Environment.IsDevelopment())
    {
        using (var scope = app.Services.CreateScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<B2BCommerce.Backend.Infrastructure.Data.DatabaseSeeder>();
            await seeder.SeedAsync();
        }
    }

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "B2B E-Commerce API v1");
            options.RoutePrefix = string.Empty; // Swagger at root
        });
    }

    // Use Serilog request logging
    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();

    // Use CORS
    app.UseCors("AllowFrontend");

    // Use Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Map Controllers
    app.MapControllers();

    // Map Health Checks
    app.MapHealthChecks("/health");

    Log.Information("B2B Commerce Backend API started successfully");

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
