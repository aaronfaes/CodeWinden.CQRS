using System.Collections.ObjectModel;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace CodeWinden.CQRS;

/// <summary>
/// Options for configuring CQRS.
/// </summary>
public record CQRSOptions
{
    /// <summary>
    /// Assembly to scan for handlers.
    /// </summary>
    public Assembly? AssemblyWithHandlers { get; set; }
    /// <summary>
    /// Collection of specific handler types to register.
    /// </summary>
    public ICollection<Type> HandlerTypes { get; } = new Collection<Type>();
    /// <summary>
    /// Lifetime of the registered handlers.
    /// </summary>
    public ServiceLifetime HandlerLifetime { get; set; } = ServiceLifetime.Scoped;
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
    /// <returns>The current CQRSOptionsBuilder instance.</returns>
    public CQRSOptionsBuilder AddHandler<THandler>()
    {
        _options.HandlerTypes.Add(typeof(THandler));
        return this;
    }

    /// <summary>
    /// Adds a handler type to be registered.
    /// </summary>
    /// <param name="handlerType">Type of the handler to add.</param>
    /// <returns>The current CQRSOptionsBuilder instance.</returns>
    public CQRSOptionsBuilder AddHandler(Type handlerType)
    {
        _options.HandlerTypes.Add(handlerType);

        return this;
    }

    /// <summary>
    /// Adds multiple handler types to be registered.
    /// </summary>
    /// <param name="handlerTypes">Span of handler types to add.</param>
    /// <returns>The current CQRSOptionsBuilder instance.</returns>
    public CQRSOptionsBuilder AddHandler(ReadOnlySpan<Type> handlerTypes)
    {
        foreach (var handlerType in handlerTypes)
        {
            _options.HandlerTypes.Add(handlerType);
        }

        return this;
    }

    /// <summary>
    /// Set the assembly to scan for handlers by specifying a type contained in that assembly.
    /// </summary>
    /// <typeparam name="TAssembly">Type contained in the assembly to scan for handlers.</typeparam>
    /// <returns>The current CQRSOptionsBuilder instance.</returns>
    public CQRSOptionsBuilder AddHandlersFromAssemblyContaining<TAssembly>()
    {
        return AddHandlersFromAssemblyContaining(typeof(TAssembly).Assembly);
    }

    // <summary>
    /// Set the assembly to scan for handlers in that assembly.
    /// </summary>
    /// <param name="assembly">Assembly to scan for handlers.</param>
    /// <returns>The current CQRSOptionsBuilder instance.</returns>
    public CQRSOptionsBuilder AddHandlersFromAssemblyContaining(Assembly assembly)
    {
        _options.AssemblyWithHandlers = assembly;

        return this;
    }

    /// <summary>
    /// Sets the lifetime for registered handlers.
    /// </summary>
    /// <param name="lifetime">The desired service lifetime.</param>
    /// <returns>The current CQRSOptionsBuilder instance.</returns>
    public CQRSOptionsBuilder WithHandlerLifetime(ServiceLifetime lifetime)
    {
        _options.HandlerLifetime = lifetime;
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