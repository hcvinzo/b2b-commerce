using MediatR;

namespace B2BCommerce.Backend.Application.Common.CQRS;

/// <summary>
/// Handler for queries
/// </summary>
/// <typeparam name="TQuery">The type of query</typeparam>
/// <typeparam name="TResult">The type of result</typeparam>
public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
}
