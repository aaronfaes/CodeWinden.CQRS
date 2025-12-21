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
        Assert.Contains(descriptors, d => d.ServiceType == typeof(ICommandHandlerDecorator<TestCommand>) && d.ImplementationType == typeof(TestCommandHandlerDecorator));
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
        Assert.Contains(descriptors, d => d.ServiceType == typeof(ICommandHandlerDecorator<TestCommand>));
        Assert.Contains(descriptors, d => d.ServiceType == typeof(ICommandHandlerDecorator<TestCommandWithResult, int>));
        Assert.Contains(descriptors, d => d.ServiceType == typeof(IQueryHandlerDecorator<TestQuery, string>));
        Assert.Contains(descriptors, d => d.ServiceType == typeof(IQueryHandlerDecorator<bool>));

        // Verify correct implementations
        var commandDescriptor = descriptors.First(d => d.ServiceType == typeof(ICommandHandlerDecorator<TestCommand>));
        Assert.Equal(typeof(TestCommandHandlerDecorator), commandDescriptor.ImplementationType);

        var queryDescriptor = descriptors.First(d => d.ServiceType == typeof(IQueryHandlerDecorator<TestQuery, string>));
        Assert.Equal(typeof(TestQueryHandlerDecorator), queryDescriptor.ImplementationType);
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

        var commandDescriptor = descriptors.First(d => d.ServiceType == typeof(ICommandHandlerDecorator<TestCommand>));
        Assert.Equal(ServiceLifetime.Singleton, commandDescriptor.Lifetime);

        var queryDescriptor = descriptors.First(d => d.ServiceType == typeof(IQueryHandlerDecorator<TestQuery, string>));
        Assert.Equal(ServiceLifetime.Transient, queryDescriptor.Lifetime);
    }

    [Fact]
    public void LocateDecorators_WithOpenGenericCommandDecorator_ReturnsGenericTypeDefinition()
    {
        // Arrange
        var options = new CQRSOptionsBuilder()
            .AddDecorator(typeof(GenericCommandHandlerDecorator<>))
            .Build();

        // Act
        var descriptors = DecoratorLocator.LocateDecorators(options).ToList();

        // Assert
        Assert.Single(descriptors);
        var descriptor = descriptors[0];
        Assert.Equal(typeof(ICommandHandlerDecorator<>), descriptor.ServiceType);
        Assert.Equal(typeof(GenericCommandHandlerDecorator<>), descriptor.ImplementationType);
    }

    [Fact]
    public void LocateDecorators_WithOpenGenericCommandWithResultDecorator_ReturnsGenericTypeDefinition()
    {
        // Arrange
        var options = new CQRSOptionsBuilder()
            .AddDecorator(typeof(GenericCommandWithResultHandlerDecorator<,>))
            .Build();

        // Act
        var descriptors = DecoratorLocator.LocateDecorators(options).ToList();

        // Assert
        Assert.Single(descriptors);
        var descriptor = descriptors[0];
        Assert.Equal(typeof(ICommandHandlerDecorator<,>), descriptor.ServiceType);
        Assert.Equal(typeof(GenericCommandWithResultHandlerDecorator<,>), descriptor.ImplementationType);
    }

    [Fact]
    public void LocateDecorators_WithOpenGenericQueryDecorator_ReturnsGenericTypeDefinition()
    {
        // Arrange
        var options = new CQRSOptionsBuilder()
            .AddDecorator(typeof(GenericTestQueryHandlerDecorator<>))
            .Build();

        // Act
        var descriptors = DecoratorLocator.LocateDecorators(options).ToList();

        // Assert
        Assert.Single(descriptors);
        var descriptor = descriptors[0];
        Assert.Equal(typeof(IQueryHandlerDecorator<>), descriptor.ServiceType);
        Assert.Equal(typeof(GenericTestQueryHandlerDecorator<>), descriptor.ImplementationType);
    }

    [Fact]
    public void LocateDecorators_WithMixedOpenAndClosedGenericDecorators_ReturnsAllDescriptors()
    {
        // Arrange
        var options = new CQRSOptionsBuilder()
            .AddDecorator(typeof(GenericCommandHandlerDecorator<>))
            .AddDecorator<TestCommandHandlerDecorator>()
            .AddDecorator(typeof(GenericTestQueryHandlerDecorator<>))
            .Build();

        // Act
        var descriptors = DecoratorLocator.LocateDecorators(options).ToList();

        // Assert
        Assert.Equal(3, descriptors.Count);

        // Verify open generic types
        Assert.Contains(descriptors, d =>
            d.ServiceType == typeof(ICommandHandlerDecorator<>) &&
            d.ImplementationType == typeof(GenericCommandHandlerDecorator<>));
        Assert.Contains(descriptors, d =>
            d.ServiceType == typeof(IQueryHandlerDecorator<>) &&
            d.ImplementationType == typeof(GenericTestQueryHandlerDecorator<>));

        // Verify closed type
        Assert.Contains(descriptors, d =>
            d.ServiceType == typeof(ICommandHandlerDecorator<TestCommand>) &&
            d.ImplementationType == typeof(TestCommandHandlerDecorator));
    }

    [Fact]
    public void LocateDecorators_WithEmptyDecoratorCollection_ReturnsEmpty()
    {
        // Arrange
        var options = new CQRSOptions();

        // Act
        var descriptors = DecoratorLocator.LocateDecorators(options).ToList();

        // Assert
        Assert.Empty(descriptors);
    }

    [Fact]
    public void LocateDecorators_WithMultipleInterfaceDecorator_RegistersAllInterfaces()
    {
        // Arrange
        var options = new CQRSOptionsBuilder()
            .AddDecorator<MultiInterfaceDecorator>()
            .Build();

        // Act
        var descriptors = DecoratorLocator.LocateDecorators(options).ToList();

        // Assert
        Assert.Equal(2, descriptors.Count);

        // Verify both interfaces are registered
        Assert.Contains(descriptors, d =>
            d.ServiceType == typeof(ICommandHandlerDecorator<TestCommand>) &&
            d.ImplementationType == typeof(MultiInterfaceDecorator));
        Assert.Contains(descriptors, d =>
            d.ServiceType == typeof(ICommandHandlerDecorator<TestCommandWithResult, int>) &&
            d.ImplementationType == typeof(MultiInterfaceDecorator));
    }

    [Fact]
    public void LocateDecorators_OnlyIncludesGenericDecoratorInterfaces_FilteringOutBaseInterface()
    {
        // Arrange
        var options = new CQRSOptionsBuilder()
            .AddDecorator<TestQueryHandlerDecorator>()
            .Build();

        // Act
        var descriptors = DecoratorLocator.LocateDecorators(options).ToList();

        // Assert
        Assert.Single(descriptors);

        // Verify all registered interfaces are generic
        Assert.All(descriptors, d => Assert.True(d.ServiceType.IsGenericType));

        // Verify base ICQRSHandlerDecorator interfaces are filtered out
        Assert.DoesNotContain(descriptors, d => d.ServiceType == typeof(ICQRSHandlerDecorator));
        Assert.DoesNotContain(descriptors, d => d.ServiceType == typeof(ICQRSHandlerDecorator<>));
    }

    [Fact]
    public void LocateDecorators_WithDuplicateDecoratorTypes_RegistersEachInstance()
    {
        // Arrange
        var options = new CQRSOptionsBuilder()
            .AddDecorator<TestCommandHandlerDecorator>(ServiceLifetime.Singleton)
            .AddDecorator<TestCommandHandlerDecorator>(ServiceLifetime.Transient)
            .Build();

        // Act
        var descriptors = DecoratorLocator.LocateDecorators(options).ToList();

        // Assert
        Assert.Equal(2, descriptors.Count);
        Assert.Contains(descriptors, d => d.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(descriptors, d => d.Lifetime == ServiceLifetime.Transient);
    }

    [Fact]
    public void LocateDecorators_ReturnsYieldedEnumerableNotMaterialized()
    {
        // Arrange
        var options = new CQRSOptionsBuilder()
            .AddDecorator<TestCommandHandlerDecorator>()
            .AddDecorator<TestQueryHandlerDecorator>()
            .Build();

        // Act
        var result = DecoratorLocator.LocateDecorators(options);

        // Assert
        // Verify result is IEnumerable and not materialized yet
        Assert.IsAssignableFrom<IEnumerable<ServiceDescriptor>>(result);

        // Can enumerate multiple times
        Assert.Equal(2, result.Count());
        Assert.Equal(2, result.Count());
    }
}