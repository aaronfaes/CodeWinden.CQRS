using System.Reflection;
using CodeWinden.CQRS.Tests.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace CodeWinden.CQRS.Tests;

public class CQRSOptionsBuilderTests
{
    [Fact]
    public void CQRSOptions_DefaultConstructor_InitializesWithDefaults()
    {
        // Act
        var options = new CQRSOptions();

        // Assert
        Assert.Null(options.AssemblyWithHandlers);
        Assert.NotNull(options.HandlerTypes);
        Assert.Empty(options.HandlerTypes);
        Assert.Equal(ServiceLifetime.Scoped, options.HandlerLifetime);
    }

    [Fact]
    public void CQRSOptions_AssemblyWithHandlers_CanBeSetDirectly()
    {
        // Arrange
        var options = new CQRSOptions();
        var assembly = typeof(TestCommandHandler).Assembly;

        // Act
        options.AssemblyWithHandlers = assembly;

        // Assert
        Assert.Equal(assembly, options.AssemblyWithHandlers);
    }

    [Fact]
    public void CQRSOptions_HandlerLifetime_CanBeSetDirectly()
    {
        // Arrange
        var options = new CQRSOptions();

        // Act
        options.HandlerLifetime = ServiceLifetime.Singleton;

        // Assert
        Assert.Equal(ServiceLifetime.Singleton, options.HandlerLifetime);
    }

    [Fact]
    public void CQRSOptions_RecordEquality_ComparesValueEquality()
    {
        // Arrange
        var assembly = typeof(TestCommandHandler).Assembly;
        var options1 = new CQRSOptions
        {
            AssemblyWithHandlers = assembly,
            HandlerLifetime = ServiceLifetime.Singleton
        };
        options1.HandlerTypes.Add(typeof(TestCommandHandler));

        var options2 = new CQRSOptions
        {
            AssemblyWithHandlers = assembly,
            HandlerLifetime = ServiceLifetime.Singleton
        };
        options2.HandlerTypes.Add(typeof(TestCommandHandler));

        // Act & Assert
        // Note: Records compare by reference for reference-type properties
        Assert.NotEqual(options1, options2); // Different HandlerTypes collection instances
    }

    [Fact]
    public void CQRSOptions_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var originalOptions = new CQRSOptions
        {
            HandlerLifetime = ServiceLifetime.Scoped
        };

        // Act
        var modifiedOptions = originalOptions with
        {
            HandlerLifetime = ServiceLifetime.Singleton
        };

        // Assert
        Assert.Equal(ServiceLifetime.Scoped, originalOptions.HandlerLifetime);
        Assert.Equal(ServiceLifetime.Singleton, modifiedOptions.HandlerLifetime);
    }

    [Fact]
    public void AddHandler_WithGenericType_AddsHandlerTypeToCollection()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();

        // Act
        var result = builder.AddHandler<TestCommandHandler>();

        // Assert
        var options = result.Build();
        Assert.Single(options.HandlerTypes);
        Assert.Contains(typeof(TestCommandHandler), options.HandlerTypes);
    }

    [Fact]
    public void AddHandler_WithGenericType_ReturnsBuilderForChaining()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();

        // Act
        var result = builder.AddHandler<TestCommandHandler>();

        // Assert
        Assert.IsType<CQRSOptionsBuilder>(result);
        Assert.Same(builder, result);
    }

    [Fact]
    public void AddHandler_WithType_AddsHandlerTypeToCollection()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();
        var handlerType = typeof(TestCommandHandler);

        // Act
        var result = builder.AddHandler(handlerType);

        // Assert
        var options = result.Build();
        Assert.Single(options.HandlerTypes);
        Assert.Contains(handlerType, options.HandlerTypes);
    }

    [Fact]
    public void AddHandler_WithType_ReturnsBuilderForChaining()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();
        var handlerType = typeof(TestCommandHandler);

        // Act
        var result = builder.AddHandler(handlerType);

        // Assert
        Assert.IsType<CQRSOptionsBuilder>(result);
        Assert.Same(builder, result);
    }

    [Fact]
    public void AddHandler_WithMultipleTypes_AddsAllHandlerTypesToCollection()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();

        // Act
        builder.AddHandler<TestCommandHandler>()
               .AddHandler<TestCommandWithResultHandler>()
               .AddHandler<AnotherTestCommandHandler>();

        // Assert
        var options = builder.Build();
        Assert.Equal(3, options.HandlerTypes.Count);
        Assert.Contains(typeof(TestCommandHandler), options.HandlerTypes);
        Assert.Contains(typeof(TestCommandWithResultHandler), options.HandlerTypes);
        Assert.Contains(typeof(AnotherTestCommandHandler), options.HandlerTypes);
    }

    [Fact]
    public void AddHandler_WithSpan_AddsAllHandlerTypesToCollection()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();
        ReadOnlySpan<Type> handlerTypes = new[]
        {
            typeof(TestCommandHandler),
            typeof(TestCommandWithResultHandler),
            typeof(AnotherTestCommandHandler)
        };

        // Act
        var result = builder.AddHandler(handlerTypes);

        // Assert
        var options = result.Build();
        Assert.Equal(3, options.HandlerTypes.Count);
        Assert.Contains(typeof(TestCommandHandler), options.HandlerTypes);
        Assert.Contains(typeof(TestCommandWithResultHandler), options.HandlerTypes);
        Assert.Contains(typeof(AnotherTestCommandHandler), options.HandlerTypes);
    }

    [Fact]
    public void AddHandler_WithSpan_ReturnsBuilderForChaining()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();
        ReadOnlySpan<Type> handlerTypes = new[] { typeof(TestCommandHandler) };

        // Act
        var result = builder.AddHandler(handlerTypes);

        // Assert
        Assert.IsType<CQRSOptionsBuilder>(result);
        Assert.Same(builder, result);
    }

    [Fact]
    public void AddHandler_WithEmptySpan_DoesNotAddAnyHandlers()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();
        ReadOnlySpan<Type> handlerTypes = Array.Empty<Type>();

        // Act
        var result = builder.AddHandler(handlerTypes);

        // Assert
        var options = result.Build();
        Assert.Empty(options.HandlerTypes);
    }

    [Fact]
    public void AddHandlersFromAssemblyContaining_WithGenericType_SetsAssembly()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();
        var expectedAssembly = typeof(TestCommandHandler).Assembly;

        // Act
        var result = builder.AddHandlersFromAssemblyContaining<TestCommandHandler>();

        // Assert
        var options = result.Build();
        Assert.Equal(expectedAssembly, options.AssemblyWithHandlers);
    }

    [Fact]
    public void AddHandlersFromAssemblyContaining_WithGenericType_ReturnsBuilderForChaining()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();

        // Act
        var result = builder.AddHandlersFromAssemblyContaining<TestCommandHandler>();

        // Assert
        Assert.IsType<CQRSOptionsBuilder>(result);
        Assert.Same(builder, result);
    }

    [Fact]
    public void AddHandlersFromAssemblyContaining_WithAssembly_SetsAssembly()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        var result = builder.AddHandlersFromAssemblyContaining(assembly);

        // Assert
        var options = result.Build();
        Assert.Equal(assembly, options.AssemblyWithHandlers);
    }

    [Fact]
    public void AddHandlersFromAssemblyContaining_WithAssembly_ReturnsBuilderForChaining()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        var result = builder.AddHandlersFromAssemblyContaining(assembly);

        // Assert
        Assert.IsType<CQRSOptionsBuilder>(result);
        Assert.Same(builder, result);
    }

    [Fact]
    public void AddHandlersFromAssemblyContaining_CalledMultipleTimes_OverridesAssembly()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();
        var firstAssembly = typeof(TestCommandHandler).Assembly;
        var secondAssembly = typeof(string).Assembly;

        // Act
        builder.AddHandlersFromAssemblyContaining(firstAssembly);
        builder.AddHandlersFromAssemblyContaining(secondAssembly);

        // Assert
        var options = builder.Build();
        Assert.Equal(secondAssembly, options.AssemblyWithHandlers);
    }

    [Fact]
    public void WithHandlerLifetime_SetsLifetime()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();

        // Act
        var result = builder.WithHandlerLifetime(ServiceLifetime.Singleton);

        // Assert
        var options = result.Build();
        Assert.Equal(ServiceLifetime.Singleton, options.HandlerLifetime);
    }

    [Fact]
    public void WithHandlerLifetime_ReturnsBuilderForChaining()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();

        // Act
        var result = builder.WithHandlerLifetime(ServiceLifetime.Transient);

        // Assert
        Assert.IsType<CQRSOptionsBuilder>(result);
        Assert.Same(builder, result);
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public void WithHandlerLifetime_WithAllLifetimes_SetsCorrectLifetime(ServiceLifetime lifetime)
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();

        // Act
        var result = builder.WithHandlerLifetime(lifetime);

        // Assert
        var options = result.Build();
        Assert.Equal(lifetime, options.HandlerLifetime);
    }

    [Fact]
    public void WithHandlerLifetime_CalledMultipleTimes_OverridesLifetime()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();

        // Act
        builder.WithHandlerLifetime(ServiceLifetime.Singleton);
        builder.WithHandlerLifetime(ServiceLifetime.Transient);

        // Assert
        var options = builder.Build();
        Assert.Equal(ServiceLifetime.Transient, options.HandlerLifetime);
    }

    [Fact]
    public void Build_ReturnsConfiguredOptions()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();
        var assembly = typeof(TestCommandHandler).Assembly;

        // Act
        builder.AddHandler<TestCommandHandler>()
               .AddHandler<TestCommandWithResultHandler>()
               .AddHandlersFromAssemblyContaining(assembly)
               .WithHandlerLifetime(ServiceLifetime.Singleton);

        var options = builder.Build();

        // Assert
        Assert.NotNull(options);
        Assert.Equal(2, options.HandlerTypes.Count);
        Assert.Equal(assembly, options.AssemblyWithHandlers);
        Assert.Equal(ServiceLifetime.Singleton, options.HandlerLifetime);
    }

    [Fact]
    public void Build_WithNoConfiguration_ReturnsDefaultOptions()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();

        // Act
        var options = builder.Build();

        // Assert
        Assert.NotNull(options);
        Assert.Empty(options.HandlerTypes);
        Assert.Null(options.AssemblyWithHandlers);
        Assert.Equal(ServiceLifetime.Scoped, options.HandlerLifetime);
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
    public void Builder_SupportsFluentChaining()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();
        var assembly = typeof(TestCommandHandler).Assembly;

        // Act
        var options = builder
            .AddHandler<TestCommandHandler>()
            .AddHandler<TestCommandWithResultHandler>()
            .AddHandlersFromAssemblyContaining<AnotherTestCommandHandler>()
            .WithHandlerLifetime(ServiceLifetime.Transient)
            .Build();

        // Assert
        Assert.NotNull(options);
        Assert.Equal(2, options.HandlerTypes.Count);
        Assert.Equal(assembly, options.AssemblyWithHandlers);
        Assert.Equal(ServiceLifetime.Transient, options.HandlerLifetime);
    }

    [Fact]
    public void AddHandler_WithDuplicateTypes_AddsBothInstances()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();

        // Act
        builder.AddHandler<TestCommandHandler>()
               .AddHandler<TestCommandHandler>();

        // Assert
        var options = builder.Build();
        Assert.Equal(2, options.HandlerTypes.Count);
        Assert.All(options.HandlerTypes, type => Assert.Equal(typeof(TestCommandHandler), type));
    }

    [Fact]
    public void HandlerTypes_IsModifiableCollection()
    {
        // Arrange
        var builder = new CQRSOptionsBuilder();

        // Act
        builder.AddHandler<TestCommandHandler>();
        var options = builder.Build();

        // Assert - verify it's a collection that can be modified
        Assert.NotNull(options.HandlerTypes);
        options.HandlerTypes.Add(typeof(AnotherTestCommandHandler));
        Assert.Equal(2, options.HandlerTypes.Count);
    }
}
