using CodeWinden.CQRS.Decorators;
using Microsoft.Extensions.DependencyInjection;

namespace CodeWinden.CQRS.Locators;

/// <summary>
/// Locates CQRS decorator types for registration in the DI container.
/// </summary>
public static class DecoratorLocator
{
    /// <summary>
    /// Locates all CQRS decorator types and returns their service descriptors for DI registration.
    /// </summary>
    /// <param name="options">Instance of CQRSOptions that is used to load decorators.</param>
    /// <returns>Service descriptors to be added to Service Collection</returns>
    public static IEnumerable<ServiceDescriptor> LocateDecorators(CQRSOptions options)
    {
        // For each type, find the interfaces it implements that derive from ICQRSHandler
        foreach (var diConfiguration in options.Decorators)
        {
            // Get all interfaces implemented by the type that derive from ICQRSHandler
            var interfaces = diConfiguration.Type.GetInterfaces()
                .Where(i => typeof(ICQRSHandlerDecorator).IsAssignableFrom(i) && i != typeof(ICQRSHandlerDecorator));

            // Register each interface with the concrete type and specified lifetime
            foreach (var decoratorInterface in interfaces)
            {
                yield return ServiceDescriptor.Describe(decoratorInterface, diConfiguration.Type, diConfiguration.Lifetime);
            }
        }
    }
}