using FluentValidation;
using MediatR;

namespace B2BCommerce.Backend.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior for automatic FluentValidation
/// </summary>
/// <typeparam name="TRequest">The type of request</typeparam>
/// <typeparam name="TResponse">The type of response</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count != 0)
        {
            // Group failures by property name
            var errorDictionary = failures
                .GroupBy(f => f.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(f => f.ErrorMessage).ToArray());

            // If response type is a Result, return a validation failure result
            var responseType = typeof(TResponse);

            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var resultType = responseType.GetGenericArguments()[0];
                var failureMethod = responseType.GetMethod("ValidationFailure",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                if (failureMethod is not null)
                {
                    return (TResponse)failureMethod.Invoke(null, new object[] { errorDictionary })!;
                }
            }
            else if (responseType == typeof(Result))
            {
                return (TResponse)(object)Result.ValidationFailure(errorDictionary);
            }

            throw new ValidationException(failures);
        }

        return await next();
    }
}
