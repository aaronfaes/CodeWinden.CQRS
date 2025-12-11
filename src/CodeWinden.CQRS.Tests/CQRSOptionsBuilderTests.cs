using System.Reflection;
using CodeWinden.CQRS.Tests.Decorators;
using CodeWinden.CQRS.Tests.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace CodeWinden.CQRS.Tests;

/// <summary>
/// Unit tests for the CQRSOptionsBuilder class.
/// </summary>
public class CQRSOptionsBuilderTests
{
    [Fact]
    public void CQRSOptions_DefaultConstructor_InitializesWithDefaults()
    {
        // Act
        var options = new CQRSOptions();

        // Assert
        Assert.Null(options.AssemblyWithHandlers);
        Assert.NotNull(options.Handlers);
        Assert.Empty(options.Handlers);
        Assert.NotNull(options.Decorators);
        Assert.Empty(options.Decorators);
        Assert.Equal(ServiceLifetime.Scoped, options.HandlerFromAssemblyLifetime);
    }

    [Fact]
    public void CQRSOptions_PropertiesCanBeSetDirectly()
    {
        // Arrange
        var options = new CQRSOptions();
        var assembly = typeof(TestCommandHandler).Assembly;

        // Act
        options.AssemblyWithHandlers = assembly;
        options.HandlerFromAssemblyLifetime = ServiceLifetime.Singleton;

        // Assert
        Assert.Equal(assembly, options.AssemblyWithHandlers);
        Assert.Equal(ServiceLifetime.Singleton, options.HandlerFromAssemblyLifetime);
    }

    [Fact]
    public void CQRSOptions_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var originalOptions = new CQRSOptions { HandlerFromAssemblyLifetime = ServiceLifetime.Scoped };

        // Act
        var modifiedOptions = originalOptions with { HandlerFromAssemblyLifetime = ServiceLifetime.Singleton };

        // Assert
        Assert.Equal(ServiceLifetime.Scoped, originalOptions.HandlerFromAssemblyLifetime);
        Assert.Equal(ServiceLifetime.Singleton, modifiedOptions.HandlerFromAssemblyLifetime);
    }

    [Fact]
    public void CQRSOptions_CollectionsAreModifiable()
    {
        // Arrange
        var options = new CQRSOptions();

        // Act & Assert
        options.Handlers.Add(new CQRSDIConfiguration { Type = typeof(TestCommandHandler), Lifetime = ServiceLifetime.Scoped });
        Assert.Single(options.Handlers);

        options.Decorators.Add(new CQRSDIConfiguration { Type = typeof(TestCommandHandlerDecorator), Lifetime = ServiceLifetime.Scoped });
        Assert.Single(options.Decorators);
    }

    // AddHandler Tests
    [Fact]
    public void AddHandler_WithGenericType_AddsHandlerWithDefaultLifetime()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();

        // Act
        var result = builder.AddHandler<TestCommandHandler>();

        // Assert
        var options = result.Build();
        Assert.Same(builder, result);
        Assert.Single(options.Handlers);
        var handler = options.Handlers.First();
        Assert.Equal(typeof(TestCommandHandler), handler.Type);
        Assert.Equal(ServiceLifetime.Scoped, handler.Lifetime);
    }

    [Fact]
    public void AddHandler_WithType_AddsHandlerWithCustomLifetime()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();
        var handlerType = typeof(TestCommandHandler);

        // Act
        var result = builder.AddHandler(handlerType, ServiceLifetime.Transient);

        // Assert
        var options = result.Build();
        Assert.Same(builder, result);
        Assert.Single(options.Handlers);
        var handler = options.Handlers.First();
        Assert.Equal(handlerType, handler.Type);
        Assert.Equal(ServiceLifetime.Transient, handler.Lifetime);
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public void AddHandler_WithAllLifetimes_AddsHandlerWithCorrectLifetime(ServiceLifetime lifetime)
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();

        // Act
        var result = builder.AddHandler<TestCommandHandler>(lifetime);

        // Assert
        var options = result.Build();
        var handler = options.Handlers.First();
        Assert.Equal(lifetime, handler.Lifetime);
    }

    [Fact]
    public void AddHandler_WithMultipleTypes_AddsAllHandlersWithDifferentLifetimes()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();

        // Act
        builder.AddHandler<TestCommandHandler>()
               .AddHandler<TestCommandWithResultHandler>(ServiceLifetime.Singleton)
               .AddHandler<AnotherTestCommandHandler>(ServiceLifetime.Transient);

        // Assert
        var options = builder.Build();
        Assert.Equal(3, options.Handlers.Count);

        var handlerTypes = options.Handlers.Select(h => h.Type).ToList();
        Assert.Contains(typeof(TestCommandHandler), handlerTypes);
        Assert.Contains(typeof(TestCommandWithResultHandler), handlerTypes);
        Assert.Contains(typeof(AnotherTestCommandHandler), handlerTypes);

        Assert.Contains(options.Handlers, h => h.Type == typeof(TestCommandHandler) && h.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(options.Handlers, h => h.Type == typeof(TestCommandWithResultHandler) && h.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(options.Handlers, h => h.Type == typeof(AnotherTestCommandHandler) && h.Lifetime == ServiceLifetime.Transient);
    }

    [Fact]
    public void AddHandler_WithDuplicateTypes_AddsBothInstancesWithDifferentLifetimes()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();

        // Act
        builder.AddHandler<TestCommandHandler>(ServiceLifetime.Scoped)
               .AddHandler<TestCommandHandler>(ServiceLifetime.Singleton);

        // Assert
        var options = builder.Build();
        Assert.Equal(2, options.Handlers.Count);
        Assert.All(options.Handlers, handler => Assert.Equal(typeof(TestCommandHandler), handler.Type));

        var lifetimes = options.Handlers.Select(h => h.Lifetime).ToList();
        Assert.Contains(ServiceLifetime.Scoped, lifetimes);
        Assert.Contains(ServiceLifetime.Singleton, lifetimes);
    }

    // AddHandlersFromAssemblyContaining Tests
    [Fact]
    public void AddHandlersFromAssemblyContaining_WithGenericType_SetsAssemblyWithDefaultLifetime()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();
        var expectedAssembly = typeof(TestCommandHandler).Assembly;

        // Act
        var result = builder.AddHandlersFromAssemblyContaining<TestCommandHandler>();

        // Assert
        var options = result.Build();
        Assert.Same(builder, result);
        Assert.Equal(expectedAssembly, options.AssemblyWithHandlers);
        Assert.Equal(ServiceLifetime.Scoped, options.HandlerFromAssemblyLifetime);
    }

    [Fact]
    public void AddHandlersFromAssemblyContaining_WithAssembly_SetsAssemblyWithCustomLifetime()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        var result = builder.AddHandlersFromAssemblyContaining(assembly, ServiceLifetime.Transient);

        // Assert
        var options = result.Build();
        Assert.Same(builder, result);
        Assert.Equal(assembly, options.AssemblyWithHandlers);
        Assert.Equal(ServiceLifetime.Transient, options.HandlerFromAssemblyLifetime);
    }

    [Fact]
    public void AddHandlersFromAssemblyContaining_CalledMultipleTimes_OverridesAssemblyAndLifetime()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();
        var firstAssembly = typeof(TestCommandHandler).Assembly;
        var secondAssembly = typeof(string).Assembly;

        // Act
        builder.AddHandlersFromAssemblyContaining(firstAssembly, ServiceLifetime.Singleton);
        builder.AddHandlersFromAssemblyContaining(secondAssembly, ServiceLifetime.Transient);

        // Assert
        var options = builder.Build();
        Assert.Equal(secondAssembly, options.AssemblyWithHandlers);
        Assert.Equal(ServiceLifetime.Transient, options.HandlerFromAssemblyLifetime);
    }

    // AddDecorator Tests
    [Fact]
    public void AddDecorator_WithGenericType_AddsDecoratorWithDefaultLifetime()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();

        // Act
        var result = builder.AddDecorator<TestCommandHandlerDecorator>();

        // Assert
        var options = result.Build();
        Assert.Same(builder, result);
        Assert.Single(options.Decorators);
        var decorator = options.Decorators.First();
        Assert.Equal(typeof(TestCommandHandlerDecorator), decorator.Type);
        Assert.Equal(ServiceLifetime.Scoped, decorator.Lifetime);
    }

    [Fact]
    public void AddDecorator_WithType_AddsDecoratorWithCustomLifetime()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();
        var decoratorType = typeof(TestQueryHandlerDecorator);

        // Act
        var result = builder.AddDecorator(decoratorType, ServiceLifetime.Transient);

        // Assert
        var options = result.Build();
        Assert.Same(builder, result);
        Assert.Single(options.Decorators);
        var decorator = options.Decorators.First();
        Assert.Equal(decoratorType, decorator.Type);
        Assert.Equal(ServiceLifetime.Transient, decorator.Lifetime);
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public void AddDecorator_WithAllLifetimes_AddsDecoratorWithCorrectLifetime(ServiceLifetime lifetime)
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();

        // Act
        var result = builder.AddDecorator<TestCommandHandlerDecorator>(lifetime);

        // Assert
        var options = result.Build();
        var decorator = options.Decorators.First();
        Assert.Equal(lifetime, decorator.Lifetime);
    }

    [Fact]
    public void AddDecorator_WithMultipleTypes_AddsAllDecoratorsIncludingQueryDecorators()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();

        // Act
        builder.AddDecorator<TestCommandHandlerDecorator>()
               .AddDecorator<TestQueryHandlerDecorator>(ServiceLifetime.Singleton)
               .AddDecorator<ParameterlessQueryHandlerDecorator>(ServiceLifetime.Transient)
               .AddDecorator(typeof(GenericCommandHandlerDecorator<>), ServiceLifetime.Scoped);

        // Assert
        var options = builder.Build();
        Assert.Equal(4, options.Decorators.Count);

        var decoratorTypes = options.Decorators.Select(d => d.Type).ToList();
        Assert.Contains(typeof(TestCommandHandlerDecorator), decoratorTypes);
        Assert.Contains(typeof(TestQueryHandlerDecorator), decoratorTypes);
        Assert.Contains(typeof(ParameterlessQueryHandlerDecorator), decoratorTypes);
        Assert.Contains(typeof(GenericCommandHandlerDecorator<>), decoratorTypes);

        Assert.Contains(options.Decorators, d => d.Type == typeof(TestCommandHandlerDecorator) && d.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(options.Decorators, d => d.Type == typeof(TestQueryHandlerDecorator) && d.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(options.Decorators, d => d.Type == typeof(ParameterlessQueryHandlerDecorator) && d.Lifetime == ServiceLifetime.Transient);
    }

    [Fact]
    public void AddDecorator_WithGenericDecoratorTypes_AddsOpenAndClosedGenerics()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();

        // Act
        builder.AddDecorator<GenericCommandHandlerDecorator<TestCommand>>(ServiceLifetime.Scoped)
               .AddDecorator(typeof(GenericTestQueryHandlerDecorator<>), ServiceLifetime.Singleton)
               .AddDecorator(typeof(GenericCommandWithResultHandlerDecorator<,>), ServiceLifetime.Transient);

        // Assert
        var options = builder.Build();
        Assert.Equal(3, options.Decorators.Count);

        var decoratorTypes = options.Decorators.Select(d => d.Type).ToList();
        Assert.Contains(typeof(GenericCommandHandlerDecorator<TestCommand>), decoratorTypes);
        Assert.Contains(typeof(GenericTestQueryHandlerDecorator<>), decoratorTypes);
        Assert.Contains(typeof(GenericCommandWithResultHandlerDecorator<,>), decoratorTypes);
    }

    [Fact]
    public void AddDecorator_WithDuplicateTypes_AddsBothInstancesWithDifferentLifetimes()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();

        // Act
        builder.AddDecorator<TestCommandHandlerDecorator>(ServiceLifetime.Scoped)
               .AddDecorator<TestCommandHandlerDecorator>(ServiceLifetime.Singleton);

        // Assert
        var options = builder.Build();
        Assert.Equal(2, options.Decorators.Count);
        Assert.All(options.Decorators, decorator => Assert.Equal(typeof(TestCommandHandlerDecorator), decorator.Type));

        var lifetimes = options.Decorators.Select(d => d.Lifetime).ToList();
        Assert.Contains(ServiceLifetime.Scoped, lifetimes);
        Assert.Contains(ServiceLifetime.Singleton, lifetimes);
    }

    // Builder Integration Tests
    [Fact]
    public void Build_WithNoConfiguration_ReturnsDefaultOptions()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();

        // Act
        var options = builder.Build();

        // Assert
        Assert.NotNull(options);
        Assert.Empty(options.Handlers);
        Assert.Empty(options.Decorators);
        Assert.Null(options.AssemblyWithHandlers);
        Assert.Equal(ServiceLifetime.Scoped, options.HandlerFromAssemblyLifetime);
    }

    [Fact]
    public void Build_CalledMultipleTimes_ReturnsSameOptionsInstance()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();

        // Act
        var firstOptions = builder.Build();
        var secondOptions = builder.Build();

        // Assert
        Assert.Same(firstOptions, secondOptions);
    }

    [Fact]
    public void Builder_SupportsFluentChainingWithHandlersDecoratorsAndAssembly()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();
        var assembly = typeof(TestCommandHandler).Assembly;

        // Act
        var options = builder
            .AddHandler<TestCommandHandler>(ServiceLifetime.Singleton)
            .AddDecorator<TestCommandHandlerDecorator>(ServiceLifetime.Scoped)
            .AddHandler<TestCommandWithResultHandler>(ServiceLifetime.Transient)
            .AddDecorator<TestQueryHandlerDecorator>(ServiceLifetime.Singleton)
            .AddDecorator<ParameterlessQueryHandlerDecorator>(ServiceLifetime.Transient)
            .AddHandlersFromAssemblyContaining<AnotherTestCommandHandler>(ServiceLifetime.Scoped)
            .Build();

        // Assert
        Assert.NotNull(options);
        Assert.Equal(2, options.Handlers.Count);
        Assert.Equal(3, options.Decorators.Count);
        Assert.Equal(assembly, options.AssemblyWithHandlers);
        Assert.Equal(ServiceLifetime.Scoped, options.HandlerFromAssemblyLifetime);

        // Verify handler types
        var handlerTypes = options.Handlers.Select(h => h.Type).ToList();
        Assert.Contains(typeof(TestCommandHandler), handlerTypes);
        Assert.Contains(typeof(TestCommandWithResultHandler), handlerTypes);

        // Verify decorator types including query decorators
        var decoratorTypes = options.Decorators.Select(d => d.Type).ToList();
        Assert.Contains(typeof(TestCommandHandlerDecorator), decoratorTypes);
        Assert.Contains(typeof(TestQueryHandlerDecorator), decoratorTypes);
        Assert.Contains(typeof(ParameterlessQueryHandlerDecorator), decoratorTypes);
    }
}
