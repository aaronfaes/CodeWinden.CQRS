using CodeWinden.CQRS.Tests.Handlers;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace CodeWinden.CQRS.Tests;

/// <summary>
/// Unit tests for the CQRSService class.
/// </summary>
public class CQRSServiceTests
{
    #region Helper Methods

    /// <summary>
    /// Creates a mock service provider that returns the specified handler.
    /// </summary>
    private static IServiceProvider CreateServiceProvider<THandler>(THandler handler)
        where THandler : class
    {
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        mockServiceProvider.GetService(typeof(THandler)).Returns(handler);
        return mockServiceProvider;
    }

    /// <summary>
    /// Creates a mock service provider with multiple handlers registered.
    /// </summary>
    private static IServiceProvider CreateServiceProviderWithHandlers(params (Type serviceType, object handler)[] handlers)
    {
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        foreach (var (serviceType, handler) in handlers)
        {
            mockServiceProvider.GetService(serviceType).Returns(handler);
        }
        return mockServiceProvider;
    }

    #endregion

    #region ExecuteCommand Tests (without result)

    [Fact]
    public async Task ExecuteCommand_WithValidCommand_CallsHandlerSuccessfully()
    {
        // Arrange
        var mockHandler = Substitute.For<ICommandHandler<TestCommand>>();
        var serviceProvider = CreateServiceProvider(mockHandler);
        var sut = new CQRSService(serviceProvider);
        var command = new TestCommand();

        // Act
        await sut.ExecuteCommand(command);

        // Assert
        await mockHandler.Received(1).Handle(command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommand_WithCancellationToken_PassesTokenToHandler()
    {
        // Arrange
        var mockHandler = Substitute.For<ICommandHandler<TestCommand>>();
        var serviceProvider = CreateServiceProvider(mockHandler);
        var sut = new CQRSService(serviceProvider);
        var command = new TestCommand();
        var cancellationToken = new CancellationToken(true);

        // Act
        await sut.ExecuteCommand(command, cancellationToken);

        // Assert
        await mockHandler.Received(1).Handle(command, cancellationToken);
    }

    [Fact]
    public async Task ExecuteCommand_WhenHandlerThrowsException_PropagatesException()
    {
        // Arrange
        var mockHandler = Substitute.For<ICommandHandler<TestCommand>>();
        var expectedException = new InvalidOperationException("Handler failed");
        mockHandler.Handle(Arg.Any<TestCommand>(), Arg.Any<CancellationToken>())
                   .Returns<Task>(_ => throw expectedException);

        var serviceProvider = CreateServiceProvider(mockHandler);
        var sut = new CQRSService(serviceProvider);
        var command = new TestCommand();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.ExecuteCommand(command));
        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public async Task ExecuteCommand_WhenHandlerNotRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        mockServiceProvider.GetService(Arg.Any<Type>())
            .Returns(x => throw new InvalidOperationException($"No service for type '{x[0]}' has been registered."));
        var sut = new CQRSService(mockServiceProvider);
        var command = new TestCommand();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.ExecuteCommand(command));
    }

    [Fact]
    public async Task ExecuteCommand_WithDefaultCancellationToken_UsesDefaultToken()
    {
        // Arrange
        var mockHandler = Substitute.For<ICommandHandler<TestCommand>>();
        var serviceProvider = CreateServiceProvider(mockHandler);
        var sut = new CQRSService(serviceProvider);
        var command = new TestCommand();

        // Act
        await sut.ExecuteCommand(command);

        // Assert
        await mockHandler.Received(1).Handle(command, default);
    }

    #endregion

    #region ExecuteCommand Tests (with result)

    [Fact]
    public async Task ExecuteCommand_WithResult_ReturnsExpectedValue()
    {
        // Arrange
        var expectedResult = 42;
        var mockHandler = Substitute.For<ICommandHandler<TestCommandWithResult, int>>();
        mockHandler.Handle(Arg.Any<TestCommandWithResult>(), Arg.Any<CancellationToken>())
                   .Returns(expectedResult);

        var serviceProvider = CreateServiceProvider(mockHandler);
        var sut = new CQRSService(serviceProvider);
        var command = new TestCommandWithResult();

        // Act
        var result = await sut.ExecuteCommand<TestCommandWithResult, int>(command);

        // Assert
        Assert.Equal(expectedResult, result);
        await mockHandler.Received(1).Handle(command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommand_WithResultAndCancellationToken_PassesTokenToHandler()
    {
        // Arrange
        var mockHandler = Substitute.For<ICommandHandler<TestCommandWithResult, int>>();
        mockHandler.Handle(Arg.Any<TestCommandWithResult>(), Arg.Any<CancellationToken>())
                   .Returns(100);

        var serviceProvider = CreateServiceProvider(mockHandler);
        var sut = new CQRSService(serviceProvider);
        var command = new TestCommandWithResult();
        var cancellationToken = new CancellationToken(true);

        // Act
        var result = await sut.ExecuteCommand<TestCommandWithResult, int>(command, cancellationToken);

        // Assert
        Assert.Equal(100, result);
        await mockHandler.Received(1).Handle(command, cancellationToken);
    }

    [Fact]
    public async Task ExecuteCommand_WithResultWhenHandlerThrows_PropagatesException()
    {
        // Arrange
        var mockHandler = Substitute.For<ICommandHandler<TestCommandWithResult, int>>();
        var expectedException = new ArgumentException("Invalid command");
        mockHandler.Handle(Arg.Any<TestCommandWithResult>(), Arg.Any<CancellationToken>())
                   .Returns<Task<int>>(_ => throw expectedException);

        var serviceProvider = CreateServiceProvider(mockHandler);
        var sut = new CQRSService(serviceProvider);
        var command = new TestCommandWithResult();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => sut.ExecuteCommand<TestCommandWithResult, int>(command));
        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public async Task ExecuteCommand_WithResultWhenHandlerNotRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        mockServiceProvider.GetService(Arg.Any<Type>())
            .Returns(x => throw new InvalidOperationException($"No service for type '{x[0]}' has been registered."));
        var sut = new CQRSService(mockServiceProvider);
        var command = new TestCommandWithResult();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.ExecuteCommand<TestCommandWithResult, int>(command));
    }

    #endregion

    #region ExecuteQuery Tests (without parameters)

    [Fact]
    public async Task ExecuteQuery_WithoutParameters_ReturnsExpectedResult()
    {
        // Arrange
        var expectedResult = true;
        var mockHandler = Substitute.For<IQueryHandler<bool>>();
        mockHandler.Handle(Arg.Any<CancellationToken>())
                   .Returns(expectedResult);

        var serviceProvider = CreateServiceProvider(mockHandler);
        var sut = new CQRSService(serviceProvider);

        // Act
        var result = await sut.ExecuteQuery<bool>();

        // Assert
        Assert.Equal(expectedResult, result);
        await mockHandler.Received(1).Handle(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteQuery_WithoutParametersAndCancellationToken_PassesTokenToHandler()
    {
        // Arrange
        var mockHandler = Substitute.For<IQueryHandler<bool>>();
        mockHandler.Handle(Arg.Any<CancellationToken>())
                   .Returns(false);

        var serviceProvider = CreateServiceProvider(mockHandler);
        var sut = new CQRSService(serviceProvider);
        var cancellationToken = new CancellationToken(true);

        // Act
        var result = await sut.ExecuteQuery<bool>(cancellationToken);

        // Assert
        Assert.False(result);
        await mockHandler.Received(1).Handle(cancellationToken);
    }

    [Fact]
    public async Task ExecuteQuery_WithoutParametersWhenHandlerThrows_PropagatesException()
    {
        // Arrange
        var mockHandler = Substitute.For<IQueryHandler<bool>>();
        var expectedException = new TimeoutException("Query timeout");
        mockHandler.Handle(Arg.Any<CancellationToken>())
                   .Returns<Task<bool>>(_ => throw expectedException);

        var serviceProvider = CreateServiceProvider(mockHandler);
        var sut = new CQRSService(serviceProvider);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TimeoutException>(
            () => sut.ExecuteQuery<bool>());
        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public async Task ExecuteQuery_WithoutParametersWhenHandlerNotRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        mockServiceProvider.GetService(Arg.Any<Type>())
            .Returns(x => throw new InvalidOperationException($"No service for type '{x[0]}' has been registered."));
        var sut = new CQRSService(mockServiceProvider);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.ExecuteQuery<bool>());
    }

    #endregion

    #region ExecuteQuery Tests (with parameters)

    [Fact]
    public async Task ExecuteQuery_WithParameters_ReturnsExpectedResult()
    {
        // Arrange
        var expectedResult = "test result";
        var mockHandler = Substitute.For<IQueryHandler<TestQuery, string>>();
        mockHandler.Handle(Arg.Any<TestQuery>(), Arg.Any<CancellationToken>())
                   .Returns(expectedResult);

        var serviceProvider = CreateServiceProvider(mockHandler);
        var sut = new CQRSService(serviceProvider);
        var query = new TestQuery();

        // Act
        var result = await sut.ExecuteQuery<TestQuery, string>(query);

        // Assert
        Assert.Equal(expectedResult, result);
        await mockHandler.Received(1).Handle(query, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteQuery_WithParametersAndCancellationToken_PassesTokenToHandler()
    {
        // Arrange
        var mockHandler = Substitute.For<IQueryHandler<TestQuery, string>>();
        mockHandler.Handle(Arg.Any<TestQuery>(), Arg.Any<CancellationToken>())
                   .Returns("result");

        var serviceProvider = CreateServiceProvider(mockHandler);
        var sut = new CQRSService(serviceProvider);
        var query = new TestQuery();
        var cancellationToken = new CancellationToken(true);

        // Act
        var result = await sut.ExecuteQuery<TestQuery, string>(query, cancellationToken);

        // Assert
        Assert.Equal("result", result);
        await mockHandler.Received(1).Handle(query, cancellationToken);
    }

    [Fact]
    public async Task ExecuteQuery_WithParametersWhenHandlerThrows_PropagatesException()
    {
        // Arrange
        var mockHandler = Substitute.For<IQueryHandler<TestQuery, string>>();
        var expectedException = new UnauthorizedAccessException("Access denied");
        mockHandler.Handle(Arg.Any<TestQuery>(), Arg.Any<CancellationToken>())
                   .Returns<Task<string>>(_ => throw expectedException);

        var serviceProvider = CreateServiceProvider(mockHandler);
        var sut = new CQRSService(serviceProvider);
        var query = new TestQuery();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => sut.ExecuteQuery<TestQuery, string>(query));
        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public async Task ExecuteQuery_WithParametersWhenHandlerNotRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        mockServiceProvider.GetService(Arg.Any<Type>())
            .Returns(x => throw new InvalidOperationException($"No service for type '{x[0]}' has been registered."));
        var sut = new CQRSService(mockServiceProvider);
        var query = new TestQuery();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.ExecuteQuery<TestQuery, string>(query));
    }

    [Fact]
    public async Task ExecuteQuery_WithDifferentQueryType_ResolvesCorrectHandler()
    {
        // Arrange
        var expectedResult = 123;
        var mockHandler = Substitute.For<IQueryHandler<AnotherTestQuery, int>>();
        mockHandler.Handle(Arg.Any<AnotherTestQuery>(), Arg.Any<CancellationToken>())
                   .Returns(expectedResult);

        var serviceProvider = CreateServiceProvider(mockHandler);
        var sut = new CQRSService(serviceProvider);
        var query = new AnotherTestQuery();

        // Act
        var result = await sut.ExecuteQuery<AnotherTestQuery, int>(query);

        // Assert
        Assert.Equal(expectedResult, result);
        await mockHandler.Received(1).Handle(query, Arg.Any<CancellationToken>());
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidServiceProvider_CreatesInstance()
    {
        // Arrange
        var mockServiceProvider = Substitute.For<IServiceProvider>();

        // Act
        var sut = new CQRSService(mockServiceProvider);

        // Assert
        Assert.NotNull(sut);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task ExecuteCommand_WithMultipleCommandTypes_ResolvesCorrectHandlers()
    {
        // Arrange
        var mockHandler1 = Substitute.For<ICommandHandler<TestCommand>>();
        var mockHandler2 = Substitute.For<ICommandHandler<AnotherTestCommand>>();

        var serviceProvider = CreateServiceProviderWithHandlers(
            (typeof(ICommandHandler<TestCommand>), mockHandler1),
            (typeof(ICommandHandler<AnotherTestCommand>), mockHandler2)
        );

        var sut = new CQRSService(serviceProvider);
        var command1 = new TestCommand();
        var command2 = new AnotherTestCommand();

        // Act
        await sut.ExecuteCommand(command1);
        await sut.ExecuteCommand(command2);

        // Assert
        await mockHandler1.Received(1).Handle(command1, Arg.Any<CancellationToken>());
        await mockHandler2.Received(1).Handle(command2, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CQRSService_ExecutingMultipleOperations_WorksCorrectly()
    {
        // Arrange
        var mockCommandHandler = Substitute.For<ICommandHandler<TestCommand>>();
        var mockQueryHandler = Substitute.For<IQueryHandler<TestQuery, string>>();
        mockQueryHandler.Handle(Arg.Any<TestQuery>(), Arg.Any<CancellationToken>())
                       .Returns("query result");

        var serviceProvider = CreateServiceProviderWithHandlers(
            (typeof(ICommandHandler<TestCommand>), mockCommandHandler),
            (typeof(IQueryHandler<TestQuery, string>), mockQueryHandler)
        );

        var sut = new CQRSService(serviceProvider);

        // Act
        await sut.ExecuteCommand(new TestCommand());
        var queryResult = await sut.ExecuteQuery<TestQuery, string>(new TestQuery());

        // Assert
        await mockCommandHandler.Received(1).Handle(Arg.Any<TestCommand>(), Arg.Any<CancellationToken>());
        await mockQueryHandler.Received(1).Handle(Arg.Any<TestQuery>(), Arg.Any<CancellationToken>());
        Assert.Equal("query result", queryResult);
    }

    #endregion
}
