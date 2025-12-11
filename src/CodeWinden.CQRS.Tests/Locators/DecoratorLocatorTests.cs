using CodeWinden.CQRS.Decorators;
using CodeWinden.CQRS.Locators;
using CodeWinden.CQRS.Tests.Decorators;
using CodeWinden.CQRS.Tests.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace CodeWinden.CQRS.Tests.Locators;

/// <summary>
/// Unit tests for the DecoratorLocator class.
/// </summary>
public class DecoratorLocatorTests
{
    [Fact]
    public void LocateDecorators_WithSingleDecorator_ReturnsCorrectDescriptor()
    {
        // Arrange
        var options = new CQRSOptionsBuilder()
            .AddDecorator<TestCommandHandlerDecorator>()
            .Build();

        // Act
        var descriptors = DecoratorLocator.LocateDecorators(options).ToList();

        // Assert
        Assert.Single(descriptors);
        Assert.Equal(typeof(ICQRSCommandHandlerDecorator<TestCommand>), descriptors[0].ServiceType);
        Assert.Equal(typeof(TestCommandHandlerDecorator), descriptors[0].ImplementationType);
    }

    [Fact]
    public void LocateDecorators_WithMultipleDecorators_ReturnsAllDescriptorsAndCorrectImplementations()
    {
        // Arrange
        var options = new CQRSOptionsBuilder()
            .AddDecorator<TestCommandHandlerDecorator>()
            .AddDecorator<TestCommandWithResultHandlerDecorator>()
            .AddDecorator<TestQueryHandlerDecorator>()
            .AddDecorator<ParameterlessQueryHandlerDecorator>()
            .Build();

        // Act
        var descriptors = DecoratorLocator.LocateDecorators(options).ToList();

        // Assert
        Assert.Equal(4, descriptors.Count);

        // Verify all decorator types are registered
        Assert.Contains(descriptors, d => d.ServiceType == typeof(ICQRSCommandHandlerDecorator<TestCommand>));
        Assert.Contains(descriptors, d => d.ServiceType == typeof(ICQRSCommandHandlerDecorator<TestCommandWithResult, int>));
        Assert.Contains(descriptors, d => d.ServiceType == typeof(ICQRSQueryHandlerDecorator<TestQuery, string>));
        Assert.Contains(descriptors, d => d.ServiceType == typeof(ICQRSQueryHandlerDecorator<bool>));

        // Verify correct implementations
        var commandDescriptor = descriptors.First(d => d.ServiceType == typeof(ICQRSCommandHandlerDecorator<TestCommand>));
        Assert.Equal(typeof(TestCommandHandlerDecorator), commandDescriptor.ImplementationType);

        var queryDescriptor = descriptors.First(d => d.ServiceType == typeof(ICQRSQueryHandlerDecorator<TestQuery, string>));
        Assert.Equal(typeof(TestQueryHandlerDecorator), queryDescriptor.ImplementationType);
    }

    [Fact]
    public void LocateDecorators_WithNoConfiguration_ReturnsEmpty()
    {
        // Arrange
        var options = new CQRSOptionsBuilder()
            .Build();

        // Act
        var descriptors = DecoratorLocator.LocateDecorators(options).ToList();

        // Assert
        Assert.Empty(descriptors);
    }

    [Fact]
    public void LocateDecorators_DefaultLifetime_IsScopedLifetime()
    {
        // Arrange
        var options = new CQRSOptionsBuilder()
            .AddDecorator<TestCommandHandlerDecorator>()
            .Build();

        // Act
        var descriptors = DecoratorLocator.LocateDecorators(options).ToList();

        // Assert
        Assert.All(descriptors, d => Assert.Equal(ServiceLifetime.Scoped, d.Lifetime));
    }

    [Fact]
    public void LocateDecorators_WithSingletonLifetime_RegistersAsSingleton()
    {
        // Arrange
        var options = new CQRSOptionsBuilder()
            .AddDecorator<TestCommandHandlerDecorator>(ServiceLifetime.Singleton)
            .Build();

        // Act
        var descriptors = DecoratorLocator.LocateDecorators(options).ToList();

        // Assert
        Assert.All(descriptors, d => Assert.Equal(ServiceLifetime.Singleton, d.Lifetime));
    }

    [Fact]
    public void LocateDecorators_WithTransientLifetime_RegistersAsTransient()
    {
        // Arrange
        var options = new CQRSOptionsBuilder()
            .AddDecorator<TestQueryHandlerDecorator>(ServiceLifetime.Transient)
            .Build();

        // Act
        var descriptors = DecoratorLocator.LocateDecorators(options).ToList();

        // Assert
        Assert.All(descriptors, d => Assert.Equal(ServiceLifetime.Transient, d.Lifetime));
    }

    [Fact]
    public void LocateDecorators_WithMixedLifetimes_RegistersWithCorrectLifetimes()
    {
        // Arrange
        var options = new CQRSOptionsBuilder()
            .AddDecorator<TestCommandHandlerDecorator>(ServiceLifetime.Singleton)
            .AddDecorator<TestQueryHandlerDecorator>(ServiceLifetime.Transient)
            .Build();

        // Act
        var descriptors = DecoratorLocator.LocateDecorators(options).ToList();

        // Assert
        Assert.Equal(2, descriptors.Count);

        var commandDescriptor = descriptors.First(d => d.ServiceType == typeof(ICQRSCommandHandlerDecorator<TestCommand>));
        Assert.Equal(ServiceLifetime.Singleton, commandDescriptor.Lifetime);

        var queryDescriptor = descriptors.First(d => d.ServiceType == typeof(ICQRSQueryHandlerDecorator<TestQuery, string>));
        Assert.Equal(ServiceLifetime.Transient, queryDescriptor.Lifetime);
    }
}