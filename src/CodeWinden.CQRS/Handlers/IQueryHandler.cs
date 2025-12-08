namespace CodeWinden.CQRS;

/// <summary>
/// Marker interface for queries to strongly type the expected result.
/// </summary>
/// <typeparam name="TResult">Type of result expected when executing a query.</typeparam>
public interface IQuery<TResult> { }

/// <summary>
/// Handler for queries without parameters.
/// </summary>
/// <typeparam name="TResult">Type of result expected when executing a query.</typeparam>
public interface IQueryHandler<TResult> : ICQRSHandler
{
    /// <summary>
    /// Handles the query and returns the result.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the query.</returns>
    Task<TResult> Handle(CancellationToken cancellationToken);
}

/// <summary>
/// Handler for queries with parameters.
/// </summary>
/// <typeparam name="TQuery">Type of query to handle.</typeparam>
/// <typeparam name="TResult">Type of result expected when executing a query.</typeparam>
public interface IQueryHandler<TQuery, TResult> : ICQRSHandler
    where TQuery : IQuery<TResult>
{
    /// <summary>
    /// Handles the query and returns the result.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the query.</returns>
    Task<TResult> Handle(TQuery query, CancellationToken cancellationToken);
}