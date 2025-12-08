namespace CodeWinden.CQRS;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Extension methods for registering CQRS services to an IServiceCollection.
/// </summary>
public static class ServiceCollectionExtensions
{
    // <summary>
    /// Registers CQRS handlers to the service collection based on the provided options.
    /// </summary>
    /// <param name="services">The service collection to register the handlers to.</param>
    /// <param name="optionsAction">An optional action to configure the CQRS options.</param>
    public static void AddCQRS(this IServiceCollection services, Action<CQRSOptionsBuilder>? optionsAction = null)
    {
        // Create options builder
        var optionsBuilder = new CQRSOptionsBuilder();

        // Apply user configurations
        optionsAction?.Invoke(optionsBuilder);

        // Build options
        var options = optionsBuilder.Build();

        AddCQRS(services, options);
    }

    /// <summary>
    /// Registers CQRS handlers to the service collection based on the provided options.
    /// </summary>
    /// <param name="services">The service collection to register the handlers to.</param>
    /// <param name="options">The CQRS options to use for configuring CQRS.</param>
    public static void AddCQRS(this IServiceCollection services, CQRSOptions options)
    {
        // Locate all handlers
        var handlers = HandlerLocator.LocateHandlers(options);

        // Register all handlers to the service collection
        foreach (var handler in handlers)
        {
            services.TryAdd(handler);
        }

        // Register the CQRS service
        services.TryAddSingleton<ICQRSService, CQRSService>();
    }
}
