using CodeWinden.CQRS.Locators;
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
        // Locate all handlers
        var handlers = HandlerLocator.LocateHandlers(options);

        // Locate all decorators
        var decorators = DecoratorLocator.LocateDecorators(options);

        // Register handlers with the configured decorators
        foreach (var handler in handlers)
        {
            services.TryAdd(handler);
        }
    }
}