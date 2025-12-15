using System;
using CodeWinden.CQRS.Decorators;

namespace CodeWinden.CQRS.Proxies;

/// <summary>
/// Proxy for CQRS query handlers.
/// </summary>
/// <typeparam name="THandler">Type of the query handler.</typeparam>
/// <typeparam name="TResult">Type of the result.</typeparam>
public class QueryHandlerProxy<THandler, TResult> :
    ProxyHandlerBase<IQueryHandler<TResult>, THandler, IQueryHandlerDecorator<TResult>>,
    IQueryHandler<TResult>
    where THandler : IQueryHandler<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryHandlerProxy{THandler, TResult}"/> class.
    /// </summary>
    /// <param name="handler">The handler.</param>
    /// <param name="decorators">The decorators.</param>
    public QueryHandlerProxy(THandler handler, IEnumerable<IQueryHandlerDecorator<TResult>> decorators) : base(handler, decorators) { }

    /// <summary>
    /// Handles the query.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the result.</returns>
    public Task<TResult> Handle(CancellationToken cancellationToken)
    {
        // Get the decorated handler
        var handler = GetDecoratedHandler();

        // Execute the command using the handler
        return handler.Handle(cancellationToken);
    }
}

/// <summary>
/// Proxy for CQRS query handlers with parameters.
/// </summary>
/// <typeparam name="THandler">Type of the query handler.</typeparam>
/// <typeparam name="TQuery">Type of the query.</typeparam>
/// <typeparam name="TResult">Type of the result.</typeparam>
public class QueryHandlerProxy<THandler, TQuery, TResult> :
    ProxyHandlerBase<IQueryHandler<TQuery, TResult>, THandler, IQueryHandlerDecorator<TQuery, TResult>>,
    IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
    where THandler : IQueryHandler<TQuery, TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryHandlerProxy{THandler, TQuery, TResult}"/> class.
    /// </summary>
    /// <param name="handler">The handler.</param>
    /// <param name="decorators">The decorators.</param>
    public QueryHandlerProxy(THandler handler, IEnumerable<IQueryHandlerDecorator<TQuery, TResult>> decorators) : base(handler, decorators) { }

    /// <summary>
    /// Handles the query.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the result.</returns>
    public Task<TResult> Handle(TQuery query, CancellationToken cancellationToken)
    {
        // Get the decorated handler
        var handler = GetDecoratedHandler();

        // Execute the command using the handler
        return handler.Handle(query, cancellationToken);
    }
}
