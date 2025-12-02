using MediatR;

namespace B2BCommerce.Backend.Application.Common.CQRS;

/// <summary>
/// Marker interface for commands that return a result
/// </summary>
/// <typeparam name="TResult">The type of result returned by the command</typeparam>
public interface ICommand<out TResult> : IRequest<TResult>
{
}

/// <summary>
/// Marker interface for commands that don't return a result
/// </summary>
public interface ICommand : IRequest<Unit>
{
}
