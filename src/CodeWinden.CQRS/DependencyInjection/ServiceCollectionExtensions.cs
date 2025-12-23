using CodeWinden.CQRS.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CodeWinden.CQRS;

/// <summary>
/// Extension methods for registering CQRS services to an IServiceCollection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers CQRS handlers to the service collection based on the provided options.
    /// </summary>
    /// <param name="services">The service collection to register the handlers to.</param>
    /// <param name="optionsAction">An optional action to configure the CQRS options.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddCQRS(this IServiceCollection services, Action<CQRSOptionsBuilder>? optionsAction = null)
    {
        // Create options builder
        var optionsBuilder = new CQRSOptionsBuilder();

        // Apply user configurations
        optionsAction?.Invoke(optionsBuilder);

        // Build options
        var options = optionsBuilder.Build();

        return AddCQRS(services, options);
    }

    /// <summary>
    /// Registers CQRS handlers to the service collection based on the provided options.
    /// </summary>
    /// <param name="services">The service collection to register the handlers to.</param>
    /// <param name="options">The CQRS options to use for configuring CQRS.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddCQRS(this IServiceCollection services, CQRSOptions options)
    {
        // Register all handlers to the service collection
        DependencyInjectionRegistrar.RegisterHandlers(services, options);

        // Register the CQRS service
        services.TryAddScoped<ICQRSService, CQRSService>();

        return services;
    }
}
