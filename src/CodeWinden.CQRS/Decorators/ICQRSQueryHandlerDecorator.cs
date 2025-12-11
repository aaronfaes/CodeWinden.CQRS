namespace CodeWinden.CQRS.Decorators;

/// <summary>
/// Decorator for CQRS query handlers.
/// </summary>
/// <typeparam name="TResult">Type of the result.</typeparam>
public interface ICQRSQueryHandlerDecorator<TResult> :
    IQueryHandler<TResult>,
    ICQRSHandlerDecorator
{ }

/// <summary>
/// Decorator for CQRS query handlers with result.
/// </summary>
/// <typeparam name="TQuery">Type of the query.</typeparam>
/// <typeparam name="TResult">Type of the result.</typeparam>
public interface ICQRSQueryHandlerDecorator<TQuery, TResult> :
    IQueryHandler<TQuery, TResult>,
    ICQRSHandlerDecorator
    where TQuery : IQuery<TResult>
{ }