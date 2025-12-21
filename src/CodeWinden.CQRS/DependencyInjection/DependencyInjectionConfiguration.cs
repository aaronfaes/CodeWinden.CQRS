using Microsoft.Extensions.DependencyInjection;

namespace CodeWinden.CQRS.DependencyInjection;

/// <summary>
/// Configuration for a service to be registered in the DI container.
/// </summary>
public record DependencyInjectionConfiguration
{
    /// <summary>
    /// Type of the service to register.
    /// </summary>
    public required Type Type { get; init; }
    /// <summary>
    /// Lifetime of the service in the DI container.
    /// </summary>
    public required ServiceLifetime Lifetime { get; init; }
}