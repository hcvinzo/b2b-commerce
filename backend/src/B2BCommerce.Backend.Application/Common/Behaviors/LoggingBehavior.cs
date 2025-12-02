using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace B2BCommerce.Backend.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior for request/response logging
/// </summary>
/// <typeparam name="TRequest">The type of request</typeparam>
/// <typeparam name="TResponse">The type of response</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestId = Guid.NewGuid().ToString();

        _logger.LogInformation(
            "Handling {RequestName} [{RequestId}]",
            requestName,
            requestId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();

            _logger.LogInformation(
                "Handled {RequestName} [{RequestId}] in {ElapsedMilliseconds}ms",
                requestName,
                requestId,
                stopwatch.ElapsedMilliseconds);

            // Log warning for slow requests (> 500ms)
            if (stopwatch.ElapsedMilliseconds > 500)
            {
                _logger.LogWarning(
                    "Long running request: {RequestName} [{RequestId}] took {ElapsedMilliseconds}ms",
                    requestName,
                    requestId,
                    stopwatch.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Error handling {RequestName} [{RequestId}] after {ElapsedMilliseconds}ms: {ErrorMessage}",
                requestName,
                requestId,
                stopwatch.ElapsedMilliseconds,
                ex.Message);

            throw;
        }
    }
}
