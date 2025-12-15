using System.Diagnostics;
using B2BCommerce.Backend.Application.DTOs.Integration;
using B2BCommerce.Backend.Application.Interfaces.Repositories;
using B2BCommerce.Backend.Domain.Entities.Integration;

namespace B2BCommerce.Backend.IntegrationAPI.Middleware;

/// <summary>
/// Middleware for logging API usage per request
/// </summary>
public class ApiUsageLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiUsageLoggingMiddleware> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public ApiUsageLoggingMiddleware(
        RequestDelegate next,
        ILogger<ApiUsageLoggingMiddleware> logger,
        IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        string? errorMessage = null;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            // Only log if we have a valid API key context
            if (context.Items.TryGetValue("ApiKeyValidationResult", out var validationResultObj)
                && validationResultObj is ApiKeyValidationResult validationResult
                && validationResult.IsValid
                && validationResult.ApiKeyId.HasValue)
            {
                // Use a separate scope to avoid DbContext concurrency issues
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var usageLogRepository = scope.ServiceProvider.GetRequiredService<IApiKeyUsageLogRepository>();
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        var usageLog = ApiKeyUsageLog.Create(
                            apiKeyId: validationResult.ApiKeyId.Value,
                            endpoint: context.Request.Path.Value ?? "/",
                            httpMethod: context.Request.Method,
                            ipAddress: GetClientIpAddress(context),
                            userAgent: context.Request.Headers.UserAgent.FirstOrDefault(),
                            responseStatusCode: context.Response.StatusCode,
                            responseTimeMs: (int)stopwatch.ElapsedMilliseconds,
                            errorMessage: errorMessage);

                        await usageLogRepository.AddAsync(usageLog);
                        await unitOfWork.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        // Don't fail the request if logging fails
                        _logger.LogError(ex, "Failed to log API usage for key {ApiKeyId}", validationResult.ApiKeyId);
                    }
                });
            }
        }
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
