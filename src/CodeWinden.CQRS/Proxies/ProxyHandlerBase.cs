using CodeWinden.CQRS.Decorators;
using Microsoft.Extensions.DependencyInjection;

namespace CodeWinden.CQRS.Proxies;

/// <summary>
/// Base class for CQRS handler proxies.
/// </summary>
/// <typeparam name="THandlerInterface">Type of the handler interface.</typeparam>
/// <typeparam name="THandler">Type of the handler.</typeparam>
/// <typeparam name="THandlerDecoratorInterface">Type of the handler decorator interface.</typeparam>
public abstract class ProxyHandlerBase<THandlerInterface, THandler, THandlerDecoratorInterface>
    where THandlerInterface : ICQRSHandler
    where THandler : THandlerInterface
    where THandlerDecoratorInterface : THandlerInterface, ICQRSHandlerDecorator<THandlerInterface>
{
    /// <summary>
    /// The handler.
    /// </summary>
    protected readonly THandler _handler;
    /// <summary>
    /// The decorators.
    /// </summary>
    protected readonly IEnumerable<THandlerDecoratorInterface> _decorators;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProxyHandlerBase{THandlerInterface, THandler, THandlerDecoratorInterface}"/> class.
    /// </summary>
    /// <param name="handler">The handler.</param>
    /// <param name="decorators">The decorators.</param>
    protected ProxyHandlerBase(THandler handler, IEnumerable<THandlerDecoratorInterface> decorators)
    {
        _handler = handler;
        _decorators = decorators;
    }

    /// <summary>
    /// Gets the decorated handler.
    /// </summary>
    /// <returns>The decorated handler.</returns>
    protected THandlerInterface GetDecoratedHandler()
    {
        // Set the handler
        THandlerInterface handler = _handler;

        // Wrap the handler with decorators
        foreach (var decorator in _decorators)
        {
            decorator.SetHandler(handler);
            handler = decorator;
        }

        // Return the fully decorated handler
        return handler;
    }
}
