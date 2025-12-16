namespace CodeWinden.CQRS.Decorators;

/// <summary>
/// Marker interface for CQRS handler decorators.
/// </summary>
public interface ICQRSHandlerDecorator { }

/// <summary>
/// Marker interface for CQRS handler decorators.
/// </summary>
public interface ICQRSHandlerDecorator<THandler> : ICQRSHandlerDecorator
    where THandler : ICQRSHandler
{
    /// <summary>
    /// Sets the handler to be decorated.
    /// </summary>
    /// <param name="handler">The handler instance to be decorated.</param>
    void SetHandler(THandler handler);
}