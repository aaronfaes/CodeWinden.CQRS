namespace CodeWinden.CQRS.Decorators;

/// <summary>
/// Decorator for CQRS query handlers.
/// </summary>
/// <typeparam name="TResult">Type of the result.</typeparam>
public interface IQueryHandlerDecorator<TResult> :
    IQueryHandler<TResult>,
    ICQRSHandlerDecorator<IQueryHandler<TResult>>
{ }

/// <summary>
/// Decorator for CQRS query handlers with result.
/// </summary>
/// <typeparam name="TQuery">Type of the query.</typeparam>
/// <typeparam name="TResult">Type of the result.</typeparam>
public interface IQueryHandlerDecorator<TQuery, TResult> :
    IQueryHandler<TQuery, TResult>,
    ICQRSHandlerDecorator<IQueryHandler<TQuery, TResult>>
    where TQuery : IQuery<TResult>
{ }