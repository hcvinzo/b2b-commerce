using MediatR;

namespace B2BCommerce.Backend.Application.Common.CQRS;

/// <summary>
/// Handler for commands that return a result
/// </summary>
/// <typeparam name="TCommand">The type of command</typeparam>
/// <typeparam name="TResult">The type of result</typeparam>
public interface ICommandHandler<in TCommand, TResult> : IRequestHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
}

/// <summary>
/// Handler for commands that don't return a result
/// </summary>
/// <typeparam name="TCommand">The type of command</typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Unit>
    where TCommand : ICommand
{
}
