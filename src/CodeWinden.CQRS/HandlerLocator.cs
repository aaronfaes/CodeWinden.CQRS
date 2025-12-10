using Microsoft.Extensions.DependencyInjection;

namespace CodeWinden.CQRS;

/// <summary>
/// Locates CQRS handler types for registration in the DI container.
/// </summary>
public static class HandlerLocator
{
    /// <summary>
    /// Locates all CQRS handler types and returns their service descriptors for DI registration.
    /// </summary>
    /// <param name="options">Instance of CQRSOptions that is used to load handlers.</param>
    /// <returns>Service descriptors to be added to Service Collection</returns>
    public static IEnumerable<ServiceDescriptor> LocateHandlers(CQRSOptions options)
    {
        // Get all types that implement handler interfaces
        var types = GetAllHandlersTypes(options);

        // For each type, find the interfaces it implements that derive from ICQRSHandler
        foreach (var type in types)
        {
            // Get all interfaces implemented by the type that derive from ICQRSHandler
            var interfaces = type.GetInterfaces()
                .Where(i => typeof(ICQRSHandler).IsAssignableFrom(i) && i != typeof(ICQRSHandler));

            // Register each interface with the concrete type and specified lifetime
            foreach (var handlerInterface in interfaces)
            {
                yield return ServiceDescriptor.Describe(handlerInterface, type, options.HandlerLifetime);
            }
        }
    }

    /// <summary>
    /// Gets all handler types from the specified assembly and explicitly added types.
    /// </summary>
    private static IEnumerable<Type> GetAllHandlersTypes(CQRSOptions options)
    {
        // If no assembly is specified, return only explicitly added types
        if (options.AssemblyWithHandlers == null)
        {
            return options.HandlerTypes;
        }

        // Scan the specified assembly for types implementing ICQRSHandler
        var assemblyTypes = options.AssemblyWithHandlers.GetTypes()
            .Where(t =>
                // we only want concrete classes
                t.IsClass &&
                !t.IsAbstract &&
                // that implements the ICQRSHandler marker interface
                typeof(ICQRSHandler).IsAssignableFrom(t)
            );

        // Combine types from assembly and explicitly added types
        return options.HandlerTypes.Concat(assemblyTypes);
    }
}