using CodeWinden.CQRS.Tests.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace CodeWinden.CQRS.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCQRS_WithDefaultOptions_RegistersCQRSService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddCQRS();

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ICQRSService));
        Assert.NotNull(descriptor);
        Assert.Equal(typeof(CQRSService), descriptor.ImplementationType);

        Assert.IsType<ServiceCollection>(result);
        Assert.Same(services, result);
    }

    [Fact]
    public void AddCQRS_WithDefaultOptions_RegistersCQRSServiceAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCQRS();

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ICQRSService));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddCQRS_WithAssemblyScanning_RegistersAllHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddCQRS(options => options.AddHandlersFromAssemblyContaining<TestCommandHandler>());

        // Assert
        Assert.Contains(services, d => d.ServiceType == typeof(ICommandHandler<TestCommand>));
        Assert.Contains(services, d => d.ServiceType == typeof(ICommandHandler<TestCommandWithResult, int>));
        Assert.Contains(services, d => d.ServiceType == typeof(IQueryHandler<TestQuery, string>));
        Assert.Contains(services, d => d.ServiceType == typeof(IQueryHandler<AnotherTestQuery, int>));

        Assert.IsType<ServiceCollection>(result);
        Assert.Same(services, result);
    }

    [Fact]
    public void AddCQRS_WithAssemblyScanning_RegistersHandlersWithCorrectImplementation()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCQRS(options => options.AddHandlersFromAssemblyContaining<TestCommandHandler>());

        // Assert
        var commandDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ICommandHandler<TestCommand>));
        Assert.NotNull(commandDescriptor);
        Assert.Equal(typeof(TestCommandHandler), commandDescriptor.ImplementationType);

        var queryDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IQueryHandler<TestQuery, string>));
        Assert.NotNull(queryDescriptor);
        Assert.Equal(typeof(TestQueryHandler), queryDescriptor.ImplementationType);
    }

    [Fact]
    public void AddCQRS_WithExplicitHandler_RegistersSpecifiedHandler()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCQRS(options => options.AddHandler<TestCommandHandler>());

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ICommandHandler<TestCommand>));
        Assert.NotNull(descriptor);
        Assert.Equal(typeof(TestCommandHandler), descriptor.ImplementationType);
    }

    [Fact]
    public void AddCQRS_WithMultipleExplicitHandlers_RegistersAllHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCQRS(options => options
            .AddHandler<TestCommandHandler>()
            .AddHandler<TestQueryHandler>());

        // Assert
        Assert.Contains(services, d => d.ServiceType == typeof(ICommandHandler<TestCommand>));
        Assert.Contains(services, d => d.ServiceType == typeof(IQueryHandler<TestQuery, string>));
    }

    [Fact]
    public void AddCQRS_WithCustomLifetime_RegistersHandlersWithSpecifiedLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCQRS(options => options
            .AddHandler<TestCommandHandler>()
            .WithHandlerLifetime(ServiceLifetime.Singleton));

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ICommandHandler<TestCommand>));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddCQRS_WithTransientLifetime_RegistersHandlersAsTransient()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCQRS(options => options
            .AddHandler<TestCommandHandler>()
            .WithHandlerLifetime(ServiceLifetime.Transient));

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ICommandHandler<TestCommand>));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
    }

    [Fact]
    public void AddCQRS_WithScopedLifetime_RegistersHandlersAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCQRS(options => options
            .AddHandler<TestCommandHandler>()
            .WithHandlerLifetime(ServiceLifetime.Scoped));

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ICommandHandler<TestCommand>));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddCQRS_WithDefaultLifetime_RegistersHandlersAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCQRS(options => options.AddHandler<TestCommandHandler>());

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ICommandHandler<TestCommand>));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
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
    }

    [Fact]
    public void AddCQRS_WithCQRSOptionsDirectly_RegistersHandlersAndService()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new CQRSOptionsBuilder()
            .AddHandler<TestCommandHandler>()
            .Build();

        // Act
        services.AddCQRS(options);

        // Assert
        Assert.Contains(services, d => d.ServiceType == typeof(ICommandHandler<TestCommand>));
        Assert.Contains(services, d => d.ServiceType == typeof(ICQRSService));
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
    public void AddCQRS_CalledMultipleTimes_DoesNotDuplicateHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCQRS(options => options.AddHandler<TestCommandHandler>());
        services.AddCQRS(options => options.AddHandler<TestCommandHandler>());

        // Assert
        var handlerDescriptors = services.Where(d => d.ServiceType == typeof(ICommandHandler<TestCommand>)).ToList();
        Assert.Single(handlerDescriptors);
    }

    [Fact]
    public void AddCQRS_WithMultiInterfaceHandler_RegistersAllInterfaces()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCQRS(options => options.AddHandler<MultiInterfaceHandler>());

        // Assert
        var descriptor1 = services.FirstOrDefault(d => d.ServiceType == typeof(ICommandHandler<MultiTestCommand>));
        Assert.NotNull(descriptor1);
        Assert.Equal(typeof(MultiInterfaceHandler), descriptor1.ImplementationType);

        var descriptor2 = services.FirstOrDefault(d => d.ServiceType == typeof(ICommandHandler<MultiAnotherTestCommand>));
        Assert.NotNull(descriptor2);
        Assert.Equal(typeof(MultiInterfaceHandler), descriptor2.ImplementationType);
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

        // Verify no handlers were registered (only CQRS service)
        Assert.Single(services);
    }

    [Fact]
    public void AddCQRS_WithHandlerTypeByType_RegistersHandler()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCQRS(options => options.AddHandler(typeof(TestCommandHandler)));

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ICommandHandler<TestCommand>));
        Assert.NotNull(descriptor);
        Assert.Equal(typeof(TestCommandHandler), descriptor.ImplementationType);
    }

    [Fact]
    public void AddCQRS_WithMixedAssemblyAndExplicitHandlers_RegistersAllHandlersOnce()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCQRS(options => options
            .AddHandlersFromAssemblyContaining<TestCommandHandler>()
            .AddHandler<TestCommandHandler>()); // Explicit addition should not duplicate

        // Assert
        var handlerDescriptors = services.Where(d => d.ServiceType == typeof(ICommandHandler<TestCommand>)).ToList();
        Assert.Single(handlerDescriptors);
    }

    [Fact]
    public void AddCQRS_WithHandlerType_RegistersHandlerAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCQRS(options => options.AddHandler(typeof(AnotherTestCommandHandler)));

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ICommandHandler<AnotherTestCommand>));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddCQRS_WithMultipleHandlersAndCustomLifetime_RegistersAllWithSameLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCQRS(options => options
            .AddHandler<TestCommandHandler>()
            .AddHandler<AnotherTestCommandHandler>()
            .WithHandlerLifetime(ServiceLifetime.Transient));

        // Assert
        var descriptor1 = services.FirstOrDefault(d => d.ServiceType == typeof(ICommandHandler<TestCommand>));
        Assert.NotNull(descriptor1);
        Assert.Equal(ServiceLifetime.Transient, descriptor1.Lifetime);

        var descriptor2 = services.FirstOrDefault(d => d.ServiceType == typeof(ICommandHandler<AnotherTestCommand>));
        Assert.NotNull(descriptor2);
        Assert.Equal(ServiceLifetime.Transient, descriptor2.Lifetime);
    }

    [Fact]
    public void AddCQRS_WithParameterlessQueryHandler_RegistersHandler()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCQRS(options => options.AddHandler<ParameterlessQueryHandler>());

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IQueryHandler<bool>));
        Assert.NotNull(descriptor);
        Assert.Equal(typeof(ParameterlessQueryHandler), descriptor.ImplementationType);
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

        Assert.True(handlerIndex >= 0);
        Assert.True(serviceIndex >= 0);
        Assert.True(handlerIndex < serviceIndex);
    }

    [Fact]
    public void AddCQRS_WithAssemblyContaining_RegistersHandlersFromCorrectAssembly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCQRS(options => options.AddHandlersFromAssemblyContaining<TestCommandHandler>());

        // Assert
        var handlers = services.Where(d =>
            typeof(ICQRSHandler).IsAssignableFrom(d.ServiceType) &&
            d.ServiceType != typeof(ICQRSHandler)).ToList();

        Assert.NotEmpty(handlers);
        Assert.All(handlers, descriptor =>
        {
            Assert.NotNull(descriptor.ImplementationType);
            Assert.Equal(typeof(TestCommandHandler).Assembly, descriptor.ImplementationType!.Assembly);
        });
    }

    [Fact]
    public void AddCQRS_WithCommandHandler_RegistersCorrectServiceType()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCQRS(options => options.AddHandler<TestCommandWithResultHandler>());

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ICommandHandler<TestCommandWithResult, int>));
        Assert.NotNull(descriptor);
        Assert.Equal(typeof(TestCommandWithResultHandler), descriptor.ImplementationType);
    }

    [Fact]
    public void AddCQRS_MultipleCalls_AddsHandlersFromAllCalls()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCQRS(options => options.AddHandler<TestCommandHandler>());
        services.AddCQRS(options => options.AddHandler<AnotherTestCommandHandler>());

        // Assert
        Assert.Contains(services, d => d.ServiceType == typeof(ICommandHandler<TestCommand>));
        Assert.Contains(services, d => d.ServiceType == typeof(ICommandHandler<AnotherTestCommand>));
    }
}
