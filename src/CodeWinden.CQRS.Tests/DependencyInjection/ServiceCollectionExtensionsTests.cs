using CodeWinden.CQRS.Decorators;
using CodeWinden.CQRS.Tests.Decorators;
using CodeWinden.CQRS.Tests.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace CodeWinden.CQRS.Tests;

/// <summary>
/// Unit tests for the ServiceCollectionExtensions class.
/// Tests focus on the extension method behavior, fluent API, and CQRS service registration.
/// Handler registration details are tested separately in DependencyInjectionRegistrarTests.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCQRS_WithDefaultOptions_RegistersCQRSServiceAndReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddCQRS();

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ICQRSService));
        Assert.NotNull(descriptor);
        Assert.Equal(typeof(CQRSService), descriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.Same(services, result);
    }

    [Fact]
    public void AddCQRS_WithOptionsAction_RegistersCQRSServiceAndReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddCQRS(options => options.AddHandler<TestCommandHandler>());

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ICQRSService));
        Assert.NotNull(descriptor);
        Assert.Equal(typeof(CQRSService), descriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.Same(services, result);
    }

    [Fact]
    public void AddCQRS_WithNullOptionsAction_RegistersCQRSService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Action<CQRSOptionsBuilder>? nullAction = null;
        services.AddCQRS(nullAction);

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ICQRSService));
        Assert.NotNull(descriptor);
        Assert.Equal(typeof(CQRSService), descriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddCQRS_WithCQRSOptionsDirectly_RegistersCQRSServiceAndHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new CQRSOptionsBuilder()
            .AddHandler<TestCommandHandler>()
            .Build();

        // Act
        var result = services.AddCQRS(options);

        // Assert
        Assert.Contains(services, d => d.ServiceType == typeof(ICommandHandler<TestCommand>));
        Assert.Contains(services, d => d.ServiceType == typeof(ICQRSService));
        Assert.Same(services, result);
    }

    [Fact]
    public void AddCQRS_CalledMultipleTimes_DoesNotDuplicateCQRSService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCQRS(options => options.AddHandler<TestCommandHandler>());
        services.AddCQRS(options => options.AddHandler<AnotherTestCommandHandler>());

        // Assert
        var cqrsServiceDescriptors = services.Where(d => d.ServiceType == typeof(ICQRSService)).ToList();
        Assert.Single(cqrsServiceDescriptors);
    }

    [Fact]
    public void AddCQRS_WithEmptyOptions_OnlyRegistersCQRSService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCQRS(options => { });

        // Assert
        var cqrsServiceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ICQRSService));
        Assert.NotNull(cqrsServiceDescriptor);
        Assert.Single(services);
    }

    [Fact]
    public void AddCQRS_RegistersHandlersBeforeCQRSService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCQRS(options => options.AddHandler<TestCommandHandler>());

        // Assert
        var handlerIndex = services.ToList().FindIndex(d => d.ServiceType == typeof(ICommandHandler<TestCommand>));
        var serviceIndex = services.ToList().FindIndex(d => d.ServiceType == typeof(ICQRSService));

        Assert.NotEqual(-1, handlerIndex);
        Assert.NotEqual(-1, serviceIndex);
        Assert.True(handlerIndex < serviceIndex, "Handler should be registered before CQRSService");
    }

    [Fact]
    public void AddCQRS_WithDecorators_RegistersHandlersAndDecorators()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCQRS(options =>
            options
                .AddHandler<TestCommandHandler>()
                .AddHandler<TestQueryHandler>()
                .AddDecorator<TestCommandHandlerDecorator>()
                .AddDecorator<TestQueryHandlerDecorator>()
        );

        // Assert
        Assert.Contains(services, d => d.ServiceType == typeof(ICommandHandler<TestCommand>));
        Assert.Contains(services, d => d.ServiceType == typeof(ICommandHandlerDecorator<TestCommand>));
        Assert.Contains(services, d => d.ServiceType == typeof(IQueryHandler<TestQuery, string>));
        Assert.Contains(services, d => d.ServiceType == typeof(IQueryHandlerDecorator<TestQuery, string>));
        Assert.Contains(services, d => d.ServiceType == typeof(ICQRSService));
    }
}
