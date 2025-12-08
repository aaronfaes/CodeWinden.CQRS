using CodeWinden.CQRS.Tests.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace CodeWinden.CQRS.Tests;

public class HandlerLocatorTests
{
    [Fact]
    public void LocateHandlers_WithAssemblyScanning_ReturnsAllHandlers()
    {
        // Arrange
        var options = new CQRSOptionsBuilder()
            .AddHandlersFromAssemblyContaining<TestCommandHandler>()
            .Build();

        // Act
        var descriptors = HandlerLocator.LocateHandlers(options).ToList();

        // Assert
        Assert.NotEmpty(descriptors);
        Assert.Equal(8, descriptors.Count);
        Assert.Contains(descriptors, d => d.ServiceType == typeof(ICommandHandler<TestCommand>));
        Assert.Contains(descriptors, d => d.ServiceType == typeof(ICommandHandler<TestCommandWithResult, int>));
        Assert.Contains(descriptors, d => d.ServiceType == typeof(ICommandHandler<AnotherTestCommand>));
        Assert.Contains(descriptors, d => d.ServiceType == typeof(IQueryHandler<TestQuery, string>));
        Assert.Contains(descriptors, d => d.ServiceType == typeof(IQueryHandler<AnotherTestQuery, int>));
        Assert.Contains(descriptors, d => d.ServiceType == typeof(IQueryHandler<bool>));
    }

    [Fact]
    public void LocateHandlers_WithAssemblyScanning_RegistersCorrectImplementations()
    {
        // Arrange
        var options = new CQRSOptionsBuilder()
            .AddHandlersFromAssemblyContaining<TestCommandHandler>()
            .Build();

        // Act
        var descriptors = HandlerLocator.LocateHandlers(options).ToList();

        // Assert
        var commandDescriptors = descriptors.Where(d => d.ServiceType == typeof(ICommandHandler<TestCommand>)).ToList();
        Assert.Contains(commandDescriptors, d => d.ImplementationType == typeof(TestCommandHandler));

        var queryDescriptor = descriptors.First(d => d.ServiceType == typeof(IQueryHandler<TestQuery, string>));
        Assert.Equal(typeof(TestQueryHandler), queryDescriptor.ImplementationType);
    }

    [Fact]
    public void LocateHandlers_WithExplicitHandlerType_ReturnsOnlySpecifiedHandler()
    {
        // Arrange
        var options = new CQRSOptionsBuilder()
            .AddHandler<TestCommandHandler>()
            .Build();

        // Act
        var descriptors = HandlerLocator.LocateHandlers(options).ToList();

        // Assert
        Assert.Single(descriptors);
        Assert.Equal(typeof(ICommandHandler<TestCommand>), descriptors[0].ServiceType);
        Assert.Equal(typeof(TestCommandHandler), descriptors[0].ImplementationType);
    }

    [Fact]
    public void LocateHandlers_WithMultipleExplicitHandlers_ReturnsAllSpecifiedHandlers()
    {
        // Arrange
        var options = new CQRSOptionsBuilder()
            .AddHandler<TestCommandHandler>()
            .AddHandler<TestQueryHandler>()
            .Build();

        // Act
        var descriptors = HandlerLocator.LocateHandlers(options).ToList();

        // Assert
        Assert.Equal(2, descriptors.Count);
        Assert.Contains(descriptors, d => d.ServiceType == typeof(ICommandHandler<TestCommand>));
        Assert.Contains(descriptors, d => d.ServiceType == typeof(IQueryHandler<TestQuery, string>));
    }

    [Fact]
    public void LocateHandlers_WithHandlerTypeParameter_ReturnsSpecifiedHandler()
    {
        // Arrange
        var options = new CQRSOptionsBuilder()
            .AddHandler(typeof(AnotherTestCommandHandler))
            .Build();

        // Act
        var descriptors = HandlerLocator.LocateHandlers(options).ToList();

        // Assert
        Assert.Single(descriptors);
        Assert.Equal(typeof(ICommandHandler<AnotherTestCommand>), descriptors[0].ServiceType);
        Assert.Equal(typeof(AnotherTestCommandHandler), descriptors[0].ImplementationType);
    }

    [Fact]
    public void LocateHandlers_WithNoConfiguration_ReturnsEmpty()
    {
        // Arrange
        var options = new CQRSOptionsBuilder()
            .Build();

        // Act
        var descriptors = HandlerLocator.LocateHandlers(options).ToList();

        // Assert
        Assert.Empty(descriptors);
    }

    [Fact]
    public void LocateHandlers_WithAssemblyAndExplicitHandlers_ReturnsCombinedHandlers()
    {
        // Arrange
        var options = new CQRSOptionsBuilder()
            .AddHandlersFromAssemblyContaining<TestCommandHandler>()
            .AddHandler<TestCommandHandler>() // Duplicate to verify deduplication behavior
            .Build();

        // Act
        var descriptors = HandlerLocator.LocateHandlers(options).ToList();

        // Assert
        Assert.NotEmpty(descriptors);
        // Should contain all handlers from assembly plus any explicit ones
        Assert.Contains(descriptors, d => d.ServiceType == typeof(ICommandHandler<TestCommand>));
    }

    [Fact]
    public void LocateHandlers_DefaultLifetime_IsScopedLifetime()
    {
        // Arrange
        var options = new CQRSOptionsBuilder()
            .AddHandler<TestCommandHandler>()
            .Build();

        // Act
        var descriptors = HandlerLocator.LocateHandlers(options).ToList();

        // Assert
        Assert.All(descriptors, d => Assert.Equal(ServiceLifetime.Scoped, d.Lifetime));
    }

    [Fact]
    public void LocateHandlers_WithSingletonLifetime_RegistersAsSingleton()
    {
        // Arrange
        var options = new CQRSOptionsBuilder()
            .AddHandler<TestCommandHandler>()
            .WithHandlerLifetime(ServiceLifetime.Singleton)
            .Build();

        // Act
        var descriptors = HandlerLocator.LocateHandlers(options).ToList();

        // Assert
        Assert.All(descriptors, d => Assert.Equal(ServiceLifetime.Singleton, d.Lifetime));
    }

    [Fact]
    public void LocateHandlers_WithTransientLifetime_RegistersAsTransient()
    {
        // Arrange
        var options = new CQRSOptionsBuilder()
            .AddHandler<TestQueryHandler>()
            .WithHandlerLifetime(ServiceLifetime.Transient)
            .Build();

        // Act
        var descriptors = HandlerLocator.LocateHandlers(options).ToList();

        // Assert
        Assert.All(descriptors, d => Assert.Equal(ServiceLifetime.Transient, d.Lifetime));
    }

    [Fact]
    public void LocateHandlers_HandlerImplementingMultipleInterfaces_RegistersAllInterfaces()
    {
        // Arrange
        var options = new CQRSOptionsBuilder()
            .AddHandler<MultiInterfaceHandler>()
            .Build();

        // Act
        var descriptors = HandlerLocator.LocateHandlers(options).ToList();

        // Assert
        Assert.Equal(2, descriptors.Count);
        Assert.Contains(descriptors, d => d.ServiceType == typeof(ICommandHandler<MultiTestCommand>));
        Assert.Contains(descriptors, d => d.ServiceType == typeof(ICommandHandler<MultiAnotherTestCommand>));
        Assert.All(descriptors, d => Assert.Equal(typeof(MultiInterfaceHandler), d.ImplementationType));
    }

    [Fact]
    public void LocateHandlers_ParameterlessQueryHandler_IsRegistered()
    {
        // Arrange
        var options = new CQRSOptionsBuilder()
            .AddHandler<ParameterlessQueryHandler>()
            .Build();

        // Act
        var descriptors = HandlerLocator.LocateHandlers(options).ToList();

        // Assert
        Assert.Single(descriptors);
        Assert.Equal(typeof(IQueryHandler<bool>), descriptors[0].ServiceType);
        Assert.Equal(typeof(ParameterlessQueryHandler), descriptors[0].ImplementationType);
    }
}
