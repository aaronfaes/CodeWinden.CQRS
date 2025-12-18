using CodeWinden.CQRS.Locators;
using CodeWinden.CQRS.Proxies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CodeWinden.CQRS.DependencyInjection;

/// <summary>
/// Registers CQRS handlers and decorators to the DI container.
/// </summary>
public static class DependencyInjectionRegistrar
{
    /// <summary>
    /// Registers CQRS handlers and decorators to the service collection based on the provided options.
    /// </summary>
    /// <param name="services">The service collection to register the handlers to.</param>
    /// <param name="options">The CQRS options to use for configuring CQRS.</param>
    public static void RegisterHandlers(IServiceCollection services, CQRSOptions options)
    {
        // Locate all decorators
        var decorators = DecoratorLocator.LocateDecorators(options);

        // Loop over all decorators and register them
        foreach (var decorator in decorators)
        {
            services.TryAdd(decorator);
        }

        // Locate all handlers
        var handlers = HandlerLocator.LocateHandlers(options);

        // Loop over all handlers and register them
        foreach (var handler in handlers)
        {
            // Register the handler to the service collection
            services.TryAdd(ServiceDescriptor.Describe(handler.ServiceType, handler.ImplementationType!, handler.Lifetime));

            // Create proxy handler with decorators
            var proxyHandlerServiceDescriptor = ProxyHandlerFactory.CreateProxyServiceDescriptor(handler);

            // Register the proxy handler to the service collection
            services.TryAdd(proxyHandlerServiceDescriptor);
        }
    }
}
