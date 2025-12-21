namespace CodeWinden.CQRS.Decorators;

/// <summary>
/// Base class for CQRS handler decorators.
/// </summary>
/// <typeparam name="THandler">The type of the handler to be decorated.</typeparam>
public abstract class BaseDecorator<THandler> : ICQRSHandlerDecorator<THandler>
    where THandler : ICQRSHandler
{
    /// <summary>
    /// The handler.
    /// </summary>
    protected THandler _handler = default!;

    /// <summary>
    /// Sets the handler to be decorated.
    /// </summary>
    /// <param name="handler">The handler instance to be decorated.</param>
    public void SetHandler(THandler handler)
    {
        _handler = handler;
    }
}