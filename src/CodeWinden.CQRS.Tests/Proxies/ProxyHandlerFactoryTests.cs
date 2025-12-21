using CodeWinden.CQRS.Proxies;
using CodeWinden.CQRS.Tests.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace CodeWinden.CQRS.Tests.Proxies;

/// <summary>
/// Unit tests for the ProxyHandlerFactory class.
/// </summary>
public class ProxyHandlerFactoryTests
{
    [Fact]
    public void CreateProxyServiceDescriptor_WithCommandHandler_CreatesCommandHandlerProxy()
    {
        // Arrange
        var handlerDescriptor = ServiceDescriptor.Describe(
            typeof(ICommandHandler<TestCommand>),
            typeof(TestCommandHandler),
            ServiceLifetime.Scoped);

        // Act
        var proxyDescriptor = ProxyHandlerFactory.CreateProxyServiceDescriptor(handlerDescriptor);

        // Assert
        Assert.NotNull(proxyDescriptor);
        Assert.Equal(typeof(CommandHandlerProxy<ICommandHandler<TestCommand>, TestCommand>), proxyDescriptor.ServiceType);
        Assert.Equal(typeof(CommandHandlerProxy<ICommandHandler<TestCommand>, TestCommand>), proxyDescriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, proxyDescriptor.Lifetime);
    }

    [Fact]
    public void CreateProxyServiceDescriptor_WithCommandHandlerWithResult_CreatesCommandHandlerWithResultProxy()
    {
        // Arrange
        var handlerDescriptor = ServiceDescriptor.Describe(
            typeof(ICommandHandler<TestCommandWithResult, int>),
            typeof(TestCommandWithResultHandler),
            ServiceLifetime.Singleton);

        // Act
        var proxyDescriptor = ProxyHandlerFactory.CreateProxyServiceDescriptor(handlerDescriptor);

        // Assert
        Assert.NotNull(proxyDescriptor);
        Assert.Equal(typeof(CommandHandlerProxy<ICommandHandler<TestCommandWithResult, int>, TestCommandWithResult, int>), proxyDescriptor.ServiceType);
        Assert.Equal(typeof(CommandHandlerProxy<ICommandHandler<TestCommandWithResult, int>, TestCommandWithResult, int>), proxyDescriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Singleton, proxyDescriptor.Lifetime);
    }

    [Fact]
    public void CreateProxyServiceDescriptor_WithQueryHandler_CreatesQueryHandlerProxy()
    {
        // Arrange
        var handlerDescriptor = ServiceDescriptor.Describe(
            typeof(IQueryHandler<bool>),
            typeof(ParameterlessQueryHandler),
            ServiceLifetime.Transient);

        // Act
        var proxyDescriptor = ProxyHandlerFactory.CreateProxyServiceDescriptor(handlerDescriptor);

        // Assert
        Assert.NotNull(proxyDescriptor);
        Assert.Equal(typeof(QueryHandlerProxy<IQueryHandler<bool>, bool>), proxyDescriptor.ServiceType);
        Assert.Equal(typeof(QueryHandlerProxy<IQueryHandler<bool>, bool>), proxyDescriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Transient, proxyDescriptor.Lifetime);
    }

    [Fact]
    public void CreateProxyServiceDescriptor_WithQueryHandlerWithQuery_CreatesQueryHandlerWithQueryProxy()
    {
        // Arrange
        var handlerDescriptor = ServiceDescriptor.Describe(
            typeof(IQueryHandler<TestQuery, string>),
            typeof(TestQueryHandler),
            ServiceLifetime.Scoped);

        // Act
        var proxyDescriptor = ProxyHandlerFactory.CreateProxyServiceDescriptor(handlerDescriptor);

        // Assert
        Assert.NotNull(proxyDescriptor);
        Assert.Equal(typeof(QueryHandlerProxy<IQueryHandler<TestQuery, string>, TestQuery, string>), proxyDescriptor.ServiceType);
        Assert.Equal(typeof(QueryHandlerProxy<IQueryHandler<TestQuery, string>, TestQuery, string>), proxyDescriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, proxyDescriptor.Lifetime);
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public void CreateProxyServiceDescriptor_WithDifferentLifetimes_PreservesLifetime(ServiceLifetime lifetime)
    {
        // Arrange
        var handlerDescriptor = ServiceDescriptor.Describe(
            typeof(ICommandHandler<TestCommand>),
            typeof(TestCommandHandler),
            lifetime);

        // Act
        var proxyDescriptor = ProxyHandlerFactory.CreateProxyServiceDescriptor(handlerDescriptor);

        // Assert
        Assert.Equal(lifetime, proxyDescriptor.Lifetime);
    }

    [Fact]
    public void CreateProxyServiceDescriptor_WithUnsupportedHandlerType_ThrowsNotSupportedException()
    {
        // Arrange
        var unsupportedDescriptor = ServiceDescriptor.Describe(
            typeof(string),
            typeof(string),
            ServiceLifetime.Scoped);

        // Act & Assert
        var exception = Assert.Throws<NotSupportedException>(() =>
            ProxyHandlerFactory.CreateProxyServiceDescriptor(unsupportedDescriptor));

        Assert.Contains("Unsupported handler type", exception.Message);
        Assert.Contains("System.String", exception.Message);
    }

    [Fact]
    public void CreateProxyServiceDescriptor_WithNonGenericType_ThrowsNotSupportedException()
    {
        // Arrange
        var nonGenericDescriptor = ServiceDescriptor.Describe(
            typeof(object),
            typeof(object),
            ServiceLifetime.Scoped);

        // Act & Assert
        var exception = Assert.Throws<NotSupportedException>(() =>
            ProxyHandlerFactory.CreateProxyServiceDescriptor(nonGenericDescriptor));

        Assert.Contains("Unsupported handler type", exception.Message);
    }

    [Fact]
    public void CreateProxyServiceDescriptor_WithCommandHandler_ServiceTypeMatchesImplementationType()
    {
        // Arrange
        var handlerDescriptor = ServiceDescriptor.Describe(
            typeof(ICommandHandler<TestCommand>),
            typeof(TestCommandHandler),
            ServiceLifetime.Scoped);

        // Act
        var proxyDescriptor = ProxyHandlerFactory.CreateProxyServiceDescriptor(handlerDescriptor);

        // Assert - for proxy, service type and implementation type should be the same
        Assert.Equal(proxyDescriptor.ServiceType, proxyDescriptor.ImplementationType);
    }

    [Fact]
    public void CreateProxyServiceDescriptor_WithQueryHandler_ServiceTypeMatchesImplementationType()
    {
        // Arrange
        var handlerDescriptor = ServiceDescriptor.Describe(
            typeof(IQueryHandler<TestQuery, string>),
            typeof(TestQueryHandler),
            ServiceLifetime.Scoped);

        // Act
        var proxyDescriptor = ProxyHandlerFactory.CreateProxyServiceDescriptor(handlerDescriptor);

        // Assert - for proxy, service type and implementation type should be the same
        Assert.Equal(proxyDescriptor.ServiceType, proxyDescriptor.ImplementationType);
    }

    [Fact]
    public void CreateProxyServiceDescriptor_WithMultipleCommandHandlers_CreatesDistinctProxyTypes()
    {
        // Arrange
        var handler1Descriptor = ServiceDescriptor.Describe(
            typeof(ICommandHandler<TestCommand>),
            typeof(TestCommandHandler),
            ServiceLifetime.Scoped);

        var handler2Descriptor = ServiceDescriptor.Describe(
            typeof(ICommandHandler<AnotherTestCommand>),
            typeof(AnotherTestCommandHandler),
            ServiceLifetime.Scoped);

        // Act
        var proxy1Descriptor = ProxyHandlerFactory.CreateProxyServiceDescriptor(handler1Descriptor);
        var proxy2Descriptor = ProxyHandlerFactory.CreateProxyServiceDescriptor(handler2Descriptor);

        // Assert
        Assert.NotEqual(proxy1Descriptor.ServiceType, proxy2Descriptor.ServiceType);
    }

    [Fact]
    public void CreateProxyServiceDescriptor_WithMultipleQueryHandlers_CreatesDistinctProxyTypes()
    {
        // Arrange
        var handler1Descriptor = ServiceDescriptor.Describe(
            typeof(IQueryHandler<TestQuery, string>),
            typeof(TestQueryHandler),
            ServiceLifetime.Scoped);

        var handler2Descriptor = ServiceDescriptor.Describe(
            typeof(IQueryHandler<AnotherTestQuery, int>),
            typeof(AnotherTestQueryHandler),
            ServiceLifetime.Scoped);

        // Act
        var proxy1Descriptor = ProxyHandlerFactory.CreateProxyServiceDescriptor(handler1Descriptor);
        var proxy2Descriptor = ProxyHandlerFactory.CreateProxyServiceDescriptor(handler2Descriptor);

        // Assert
        Assert.NotEqual(proxy1Descriptor.ServiceType, proxy2Descriptor.ServiceType);
    }
}
