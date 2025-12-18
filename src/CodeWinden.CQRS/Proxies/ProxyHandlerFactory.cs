using Microsoft.Extensions.DependencyInjection;

namespace CodeWinden.CQRS.Proxies;

/// <summary>
/// Factory for creating proxy service descriptors for CQRS handlers with decorators.
/// </summary>
public static class ProxyHandlerFactory
{
    private static readonly Type _commandHandlerProxyTypeDefinition = typeof(CommandHandlerProxy<,>);
    private static readonly Type _commandHandlerWithResultProxyTypeDefinition = typeof(CommandHandlerProxy<,,>);
    private static readonly Type _queryHandlerProxyTypeDefinition = typeof(QueryHandlerProxy<,>);
    private static readonly Type _queryHandlerWithResultProxyTypeDefinition = typeof(QueryHandlerProxy<,,>);

    /// <summary>
    /// Creates a proxy service descriptor for the given handler and its applicable decorators.
    /// </summary>
    /// <param name="handler">The original service descriptor for the handler.</param>
    /// <returns>A new service descriptor for the proxy.</returns>
    /// <exception cref="NotSupportedException">Thrown when the handler type is not supported.</exception>
    public static ServiceDescriptor CreateProxyServiceDescriptor(ServiceDescriptor handler)
    {
        switch (handler.ServiceType)
        {
            case Type serviceType
                when serviceType.IsGenericType &&
                     serviceType.GetGenericTypeDefinition() == typeof(ICommandHandler<>):
                return CreateCommandHandlerProxyDescriptor(handler);

            case Type serviceType when
                serviceType.IsGenericType &&
                serviceType.GetGenericTypeDefinition() == typeof(ICommandHandler<,>):
                return CreateCommandHandlerWithResultProxyDescriptor(handler);

            case Type serviceType when
                serviceType.IsGenericType &&
                serviceType.GetGenericTypeDefinition() == typeof(IQueryHandler<>):
                return CreateQueryHandlerProxyDescriptor(handler);

            case Type serviceType when
                serviceType.IsGenericType &&
                serviceType.GetGenericTypeDefinition() == typeof(IQueryHandler<,>):
                return CreateQueryHandlerWithQueryProxyDescriptor(handler);

            default:
                throw new NotSupportedException($"Unsupported handler type: {handler.ServiceType.FullName}");
        }
    }

    private static ServiceDescriptor CreateCommandHandlerProxyDescriptor(ServiceDescriptor handler)
    {
        var genericType = handler.ServiceType.GetGenericArguments();
        var proxyType = _commandHandlerProxyTypeDefinition.MakeGenericType(handler.ServiceType, genericType[0]);
        return ServiceDescriptor.Describe(proxyType, proxyType, handler.Lifetime);
    }

    private static ServiceDescriptor CreateCommandHandlerWithResultProxyDescriptor(ServiceDescriptor handler)
    {
        var genericType = handler.ServiceType.GetGenericArguments();
        var proxyType = _commandHandlerWithResultProxyTypeDefinition.MakeGenericType(handler.ServiceType, genericType[0], genericType[1]);
        return ServiceDescriptor.Describe(proxyType, proxyType, handler.Lifetime);
    }

    private static ServiceDescriptor CreateQueryHandlerProxyDescriptor(ServiceDescriptor handler)
    {
        var genericType = handler.ServiceType.GetGenericArguments();
        var proxyType = _queryHandlerProxyTypeDefinition.MakeGenericType(handler.ServiceType, genericType[0]);
        return ServiceDescriptor.Describe(proxyType, proxyType, handler.Lifetime);
    }

    private static ServiceDescriptor CreateQueryHandlerWithQueryProxyDescriptor(ServiceDescriptor handler)
    {
        var genericType = handler.ServiceType.GetGenericArguments();
        var proxyType = _queryHandlerWithResultProxyTypeDefinition.MakeGenericType(handler.ServiceType, genericType[0], genericType[1]);
        return ServiceDescriptor.Describe(proxyType, proxyType, handler.Lifetime);
    }
}