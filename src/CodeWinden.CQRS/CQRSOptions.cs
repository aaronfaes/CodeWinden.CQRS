using System.Collections.ObjectModel;
using System.Reflection;
using CodeWinden.CQRS.Decorators;
using CodeWinden.CQRS.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace CodeWinden.CQRS;

/// <summary>
/// Options for configuring CQRS.
/// </summary>
public record CQRSOptions
{
    /// <summary>
    /// Assembly to scan for handlers. (Note: the lifetime will be set to the <see cref="HandlerFromAssemblyLifetime"/>.)
    /// </summary>
    public Assembly? AssemblyWithHandlers { get; set; }
    /// <summary>
    /// Collection of specific handler types to register.
    /// </summary>
    public ICollection<DependencyInjectionConfiguration> Handlers { get; } = new Collection<DependencyInjectionConfiguration>();
    /// <summary>
    /// Collection of decorators to apply to handlers.
    /// </summary>
    public ICollection<DependencyInjectionConfiguration> Decorators { get; } = new Collection<DependencyInjectionConfiguration>();
    /// <summary>
    /// Additional registrations to perform on the service collection.
    /// </summary>
    public ICollection<Action<IServiceCollection>> AdditionalRegistrations { get; } = new Collection<Action<IServiceCollection>>();
    /// <summary>
    /// Default lifetime for handlers if not specified otherwise.
    /// </summary>
    public ServiceLifetime HandlerFromAssemblyLifetime { get; set; } = ServiceLifetime.Scoped;
}

/// <summary>
/// Builder for configuring CQRS options.
/// </summary>
public class CQRSOptionsBuilder
{
    /// <summary>
    /// Instance of CQRSOptions being built.
    /// </summary>
    private readonly CQRSOptions _options = new CQRSOptions();

    /// <summary>
    /// Adds a handler type to be registered.
    /// </summary>
    /// <typeparam name="THandler">Type of the handler to add.</typeparam>
    /// <param name="lifetime">Service lifetime for the handler.</param>
    /// <returns>The current CQRSOptionsBuilder instance.</returns>
    public CQRSOptionsBuilder AddHandler<THandler>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        return AddHandler(typeof(THandler), lifetime);
    }

    /// <summary>
    /// Adds a handler type to be registered.
    /// </summary>
    /// <param name="handlerType">Type of the handler to add.</param>
    /// <param name="lifetime">Service lifetime for the handler.</param>
    /// <returns>The current CQRSOptionsBuilder instance.</returns>
    public CQRSOptionsBuilder AddHandler(Type handlerType, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        _options.Handlers.Add(new DependencyInjectionConfiguration { Type = handlerType, Lifetime = lifetime });

        return this;
    }

    /// <summary>
    /// Set the assembly to scan for handlers by specifying a type contained in that assembly.
    /// </summary>
    /// <typeparam name="TAssembly">Type contained in the assembly to scan for handlers.</typeparam>
    /// <param name="lifetime">Service lifetime for the handlers.</param>
    /// <returns>The current CQRSOptionsBuilder instance.</returns>
    public CQRSOptionsBuilder AddHandlersFromAssemblyContaining<TAssembly>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        return AddHandlersFromAssemblyContaining(typeof(TAssembly).Assembly, lifetime);
    }

    /// <summary>
    /// Set the assembly to scan for handlers in that assembly.
    /// </summary>
    /// <param name="assembly">Assembly to scan for handlers.</param>
    /// <param name="lifetime">Service lifetime for the handlers.</param>
    /// <returns>The current CQRSOptionsBuilder instance.</returns>
    public CQRSOptionsBuilder AddHandlersFromAssemblyContaining(Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        _options.AssemblyWithHandlers = assembly;
        _options.HandlerFromAssemblyLifetime = lifetime;

        return this;
    }

    /// <summary>
    /// Adds a decorator type to be applied to handlers.
    /// </summary>
    /// <typeparam name="TDecorator">Type of the decorator to add.</typeparam>
    /// <param name="lifetime">Service lifetime for the decorator.</param>
    /// <returns>The current CQRSOptionsBuilder instance.</returns>
    public CQRSOptionsBuilder AddDecorator<TDecorator>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TDecorator : ICQRSHandlerDecorator
    {
        return AddDecorator(typeof(TDecorator), lifetime);
    }

    /// <summary>
    /// Adds a decorator type to be applied to handlers.
    /// </summary>
    /// <param name="type">Type of the decorator to add.</param>
    /// <param name="lifetime">Service lifetime for the decorator.</param>
    /// <returns>The current CQRSOptionsBuilder instance.</returns>
    public CQRSOptionsBuilder AddDecorator(Type type, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        // Ensure the type is a concrete class
        if (type.IsAbstract || type.IsInterface)
        {
            throw new ArgumentException("Decorator type must be a concrete class.", nameof(type));
        }

        // Ensure the type implements ICQRSHandlerDecorator
        if (!typeof(ICQRSHandlerDecorator).IsAssignableFrom(type))
        {
            throw new ArgumentException("Decorator type must implement ICQRSHandlerDecorator. Use ICQRSCommandHandlerDecorator or ICQRSQueryHandlerDecorator instead.", nameof(type));
        }

        // Add the decorator configuration
        _options.Decorators.Add(new DependencyInjectionConfiguration { Type = type, Lifetime = lifetime });

        return this;
    }

    /// <summary>
    /// Adds an additional registration action to be performed on the service collection.
    /// </summary>
    /// <param name="registrationAction">The action to perform additional registrations on the service collection.</param>
    /// <returns>The current CQRSOptionsBuilder instance.</returns>
    public CQRSOptionsBuilder AddAdditionalRegistration(Action<IServiceCollection> registrationAction)
    {
        _options.AdditionalRegistrations.Add(registrationAction);
        return this;
    }

    /// <summary>
    /// Builds the CQRSOptions instance.
    /// </summary>
    /// <returns>Configured CQRSOptions instance.</returns>
    public CQRSOptions Build()
    {
        return _options;
    }
}