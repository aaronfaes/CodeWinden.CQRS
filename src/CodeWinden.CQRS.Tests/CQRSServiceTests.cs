using CodeWinden.CQRS.Tests.Decorators;
using CodeWinden.CQRS.Tests.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CodeWinden.CQRS.Tests;

/// <summary>
/// Unit tests for the CQRSService class.
/// </summary>
public class CQRSServiceTests
{
    private readonly ILogger _logger;

    public CQRSServiceTests()
    {
        // Create a mock logger for test handlers that require it
        _logger = Substitute.For<ILogger>();
    }

    private IServiceProvider CreateServiceProvider(Action<CQRSOptionsBuilder>? optionsAction = null)
    {
        var services = new ServiceCollection();

        // Add logger for handlers that need it
        services.AddSingleton(_logger);

        // Configure CQRS with the provided options
        services.AddCQRS(optionsAction);

        return services.BuildServiceProvider();
    }

    private void AssertLogContains(string expectedMessage, int times = 1)
    {
        _logger.Received(times).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains(expectedMessage)),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    private void AssertLogContainsAll(params string[] expectedMessages)
    {
        foreach (var expectedMessage in expectedMessages)
        {
            _logger.Received().Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(expectedMessage)),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());
        }
    }

    // ExecuteCommand Tests (no result)
    [Fact]
    public async Task ExecuteCommand_WithValidCommand_ExecutesSuccessfully()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(builder =>
            builder.AddHandler<TestCommandHandler>());

        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();
        var command = new TestCommand { Id = 123 };

        // Act
        await cqrsService.ExecuteCommand(command);

        // Assert - no exception means success
        Assert.True(true);
    }

    [Fact]
    public async Task ExecuteCommand_WithCancellationToken_PassesTokenToHandler()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(builder =>
            builder.AddHandler<TestCommandHandler>());

        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();
        var command = new TestCommand { Id = 456 };

        using (var cts = new CancellationTokenSource())
        {
            // Act
            await cqrsService.ExecuteCommand(command, cts.Token);

            // Assert - handler executed without cancellation
            Assert.False(cts.Token.IsCancellationRequested);
        }
    }

    [Fact]
    public async Task ExecuteCommand_WithMultipleCommands_ExecutesEachIndependently()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(builder =>
        {
            builder.AddHandler<TestCommandHandler>();
            builder.AddHandler<AnotherTestCommandHandler>();
        });

        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();
        var command1 = new TestCommand { Id = 1 };
        var command2 = new AnotherTestCommand { Id = 2 };

        // Act
        await cqrsService.ExecuteCommand(command1);
        await cqrsService.ExecuteCommand(command2);

        // Assert - both commands executed successfully
        Assert.True(true);
    }

    [Fact]
    public async Task ExecuteCommand_WithHandlerFromAssembly_ExecutesSuccessfully()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(builder =>
            builder.AddHandlersFromAssemblyContaining<TestCommandHandler>());

        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();
        var command = new TestCommand { Id = 789 };

        // Act
        await cqrsService.ExecuteCommand(command);

        // Assert
        Assert.True(true);
    }

    // ExecuteCommand Tests (with result)
    [Fact]
    public async Task ExecuteCommand_WithResult_ReturnsExpectedValue()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(builder =>
            builder.AddHandler<TestCommandWithResultHandler>());

        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();
        var command = new TestCommandWithResult { Value = 10 };

        // Act
        var result = await cqrsService.ExecuteCommand(command, CancellationToken.None);

        // Assert
        Assert.Equal(420, result); // 10 * 42
    }

    [Fact]
    public async Task ExecuteCommand_WithResultAndCancellationToken_ReturnsExpectedValue()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(builder =>
            builder.AddHandler<TestCommandWithResultHandler>());

        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();
        var command = new TestCommandWithResult { Value = 5 };

        using (var cts = new CancellationTokenSource())
        {
            // Act
            var result = await cqrsService.ExecuteCommand(command, cts.Token);

            // Assert
            Assert.Equal(210, result); // 5 * 42
        }
    }

    [Fact]
    public async Task ExecuteCommand_WithResultFromAssembly_ReturnsExpectedValue()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(builder =>
            builder.AddHandlersFromAssemblyContaining<TestCommandWithResultHandler>());

        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();
        var command = new TestCommandWithResult { Value = 3 };

        // Act
        var result = await cqrsService.ExecuteCommand(command);

        // Assert
        Assert.Equal(126, result); // 3 * 42
    }

    // ExecuteQuery Tests (parameterless)
    [Fact]
    public async Task ExecuteQuery_Parameterless_ReturnsExpectedResult()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(builder =>
            builder.AddHandler<ParameterlessQueryHandler>());

        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();

        // Act
        var result = await cqrsService.ExecuteQuery<bool>();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExecuteQuery_ParameterlessWithCancellationToken_ReturnsExpectedResult()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(builder =>
            builder.AddHandler<ParameterlessQueryHandler>());

        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();

        using (var cts = new CancellationTokenSource())
        {
            // Act
            var result = await cqrsService.ExecuteQuery<bool>(cts.Token);

            // Assert
            Assert.True(result);
        }
    }

    [Fact]
    public async Task ExecuteQuery_ParameterlessFromAssembly_ReturnsExpectedResult()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(builder =>
            builder.AddHandlersFromAssemblyContaining<ParameterlessQueryHandler>());

        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();

        // Act
        var result = await cqrsService.ExecuteQuery<bool>();

        // Assert
        Assert.True(result);
    }

    // ExecuteQuery Tests (with parameters)
    [Fact]
    public async Task ExecuteQuery_WithParameters_ReturnsExpectedResult()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(builder =>
            builder.AddHandler<TestQueryHandler>());

        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();
        var query = new TestQuery { Id = 42 };

        // Act
        var result = await cqrsService.ExecuteQuery(query);

        // Assert
        Assert.Equal("Result for Id: 42", result);
    }

    [Fact]
    public async Task ExecuteQuery_WithParametersAndCancellationToken_ReturnsExpectedResult()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(builder =>
            builder.AddHandler<TestQueryHandler>());

        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();
        var query = new TestQuery { Id = 99 };

        using (var cts = new CancellationTokenSource())
        {
            // Act
            var result = await cqrsService.ExecuteQuery(query, cts.Token);

            // Assert
            Assert.Equal("Result for Id: 99", result);
        }
    }

    [Fact]
    public async Task ExecuteQuery_WithMultipleQueries_ReturnsCorrectResults()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(builder =>
        {
            builder.AddHandler<TestQueryHandler>();
            builder.AddHandler<AnotherTestQueryHandler>();
        });

        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();
        var query1 = new TestQuery { Id = 100 };
        var query2 = new AnotherTestQuery { Value = 50 };

        // Act
        var result1 = await cqrsService.ExecuteQuery(query1);
        var result2 = await cqrsService.ExecuteQuery(query2);

        // Assert
        Assert.Equal("Result for Id: 100", result1);
        Assert.Equal(100, result2); // 50 * 2
    }

    [Fact]
    public async Task ExecuteQuery_WithParametersFromAssembly_ReturnsExpectedResult()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(builder =>
            builder.AddHandlersFromAssemblyContaining<TestQueryHandler>());

        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();
        var query = new TestQuery { Id = 777 };

        // Act
        var result = await cqrsService.ExecuteQuery(query);

        // Assert
        Assert.Equal("Result for Id: 777", result);
    }

    // Service Lifetime Tests
    [Fact]
    public void CQRSService_RegisteredAsSingleton_ReturnsSameInstance()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(builder =>
            builder.AddHandler<TestCommandHandler>());

        // Act
        var service1 = serviceProvider.GetRequiredService<ICQRSService>();
        var service2 = serviceProvider.GetRequiredService<ICQRSService>();

        // Assert
        Assert.Same(service1, service2);
    }

    [Fact]
    public async Task CQRSService_WithScopedHandler_ExecutesInScope()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(builder =>
            builder.AddHandler<TestCommandHandler>(ServiceLifetime.Scoped));

        using var scope = serviceProvider.CreateScope();
        var cqrsService = scope.ServiceProvider.GetRequiredService<ICQRSService>();
        var command = new TestCommand { Id = 123 };

        // Act
        await cqrsService.ExecuteCommand(command);

        // Assert
        Assert.True(true);
    }

    [Fact]
    public async Task CQRSService_WithTransientHandler_ExecutesSuccessfully()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(builder =>
            builder.AddHandler<TestCommandHandler>(ServiceLifetime.Transient));

        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();
        var command = new TestCommand { Id = 456 };

        // Act
        await cqrsService.ExecuteCommand(command);

        // Assert
        Assert.True(true);
    }

    [Fact]
    public async Task CQRSService_WithSingletonHandler_ExecutesSuccessfully()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(builder =>
            builder.AddHandler<TestCommandWithResultHandler>(ServiceLifetime.Singleton));

        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();
        var command = new TestCommandWithResult { Value = 7 };

        // Act
        var result = await cqrsService.ExecuteCommand(command);

        // Assert
        Assert.Equal(294, result); // 7 * 42
    }

    // Decorator Tests
    [Fact]
    public async Task ExecuteCommand_WithDecorator_DecoratorIsInvoked()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(builder =>
        {
            builder.AddHandler<TestCommandHandler>();
            builder.AddDecorator<TestCommandHandlerDecorator>();
        });

        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();
        var command = new TestCommand { Id = 100 };

        // Act
        await cqrsService.ExecuteCommand(command);

        // Assert - verify logger was called by decorator
        AssertLogContains("Decorator modifying TestCommand Id 100");
    }

    [Fact]
    public async Task ExecuteCommand_WithResultAndDecorator_DecoratorIsInvoked()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(builder =>
        {
            builder.AddHandler<TestCommandWithResultHandler>();
            builder.AddDecorator<TestCommandWithResultHandlerDecorator>();
        });

        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();
        var command = new TestCommandWithResult { Value = 5 };

        // Act
        var result = await cqrsService.ExecuteCommand(command);

        // Assert
        Assert.Equal(210, result); // 5 * 42
        AssertLogContains("Decorator modifying TestCommandWithResult Value 5");
    }

    [Fact]
    public async Task ExecuteQuery_WithDecorator_DecoratorIsInvokedAndModifiesQuery()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(builder =>
        {
            builder.AddHandler<TestQueryHandler>();
            builder.AddDecorator<TestQueryHandlerDecorator>();
        });

        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();
        var query = new TestQuery { Id = 42 };

        // Act
        var result = await cqrsService.ExecuteQuery(query);

        // Assert - decorator adds 1 to the Id before passing to handler
        Assert.Equal("Result for Id: 43", result);
    }

    [Fact]
    public async Task ExecuteQuery_ParameterlessWithDecorator_DecoratorIsInvokedAndInvertsResult()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(builder =>
        {
            builder.AddHandler<ParameterlessQueryHandler>();
            builder.AddDecorator<ParameterlessQueryHandlerDecorator>();
        });

        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();

        // Act
        var result = await cqrsService.ExecuteQuery<bool>();

        // Assert - decorator inverts the result (true becomes false)
        Assert.False(result);
    }

    [Fact]
    public async Task ExecuteCommand_WithoutDecorator_HandlerExecutesDirectly()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(builder =>
            builder.AddHandler<TestCommandHandler>());

        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();
        var command = new TestCommand { Id = 200 };

        // Act
        await cqrsService.ExecuteCommand(command);

        // Assert - verify handler's logger was called directly (not via decorator)
        AssertLogContains("Handling TestCommand with Id: 200");
    }

    [Fact]
    public async Task ExecuteCommand_WithMultipleDecorators_AllDecoratorsAreApplied()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(builder =>
        {
            builder.AddHandler<TestCommandHandler>();
            // Register two different decorators
            builder.AddDecorator<TestCommandHandlerDecorator>();
            builder.AddDecorator<GenericCommandHandlerDecorator<TestCommand>>();
        });

        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();
        var command = new TestCommand { Id = 300 };

        // Act
        await cqrsService.ExecuteCommand(command);

        // Assert - verify both decorators were called
        AssertLogContainsAll("Decorator modifying TestCommand Id 300", "Generic decorator handling command of type TestCommand");
    }

    [Fact]
    public async Task ExecuteCommand_WithDecoratorFromAssembly_DecoratorIsInvoked()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(builder =>
        {
            builder.AddHandlersFromAssemblyContaining<TestCommandHandler>();
            builder.AddDecorator<TestCommandHandlerDecorator>();
        });

        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();
        var command = new TestCommand { Id = 400 };

        // Act
        await cqrsService.ExecuteCommand(command);

        // Assert
        AssertLogContains("Decorator modifying TestCommand Id 400");
    }

    [Fact]
    public async Task ExecuteCommand_WithMultiInterfaceDecorator_DecoratorHandlesBothCommandTypes()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(builder =>
        {
            builder.AddHandler<TestCommandHandler>();
            builder.AddHandler<TestCommandWithResultHandler>();
            builder.AddDecorator<MultiInterfaceDecorator>();
        });

        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();
        var command1 = new TestCommand { Id = 500 };
        var command2 = new TestCommandWithResult { Value = 10 };

        // Act
        await cqrsService.ExecuteCommand(command1);
        var result = await cqrsService.ExecuteCommand(command2);

        // Assert - both commands executed successfully through the same decorator
        Assert.Equal(420, result); // 10 * 42
        AssertLogContains("Handling TestCommand with Id: 500");
    }

    [Fact]
    public async Task ExecuteCommand_WithMultiInterfaceHandler_HandlerHandlesBothCommandTypes()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(builder =>
            builder.AddHandler<MultiInterfaceHandler>());

        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();
        var command1 = new MultiTestCommand();
        var command2 = new MultiAnotherTestCommand();

        // Act
        await cqrsService.ExecuteCommand(command1);
        await cqrsService.ExecuteCommand(command2);

        // Assert - verify both commands were handled by the same handler
        AssertLogContains("Handling MultiTestCommand");
        AssertLogContains("Handling MultiAnotherTestCommand");
    }

    [Fact]
    public void CQRSService_WithAdditionalRegistrations_ExecutesAdditionalRegistrations()
    {
        // Arrange
        var additionalServiceCalled = false;
        var services = new ServiceCollection();
        services.AddSingleton(_logger);

        services.AddCQRS(builder =>
        {
            builder.AddHandler<TestCommandHandler>();
            builder.AddAdditionalRegistration(svc =>
            {
                additionalServiceCalled = true;
                svc.AddSingleton<string>("custom-service");
            });
        });

        // Act
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.True(additionalServiceCalled);
        var customService = serviceProvider.GetService<string>();
        Assert.Equal("custom-service", customService);
    }

    [Fact]
    public async Task CQRSService_WithAdditionalRegistrationsAndHandlers_BothWorkTogether()
    {
        // Arrange
        var customValue = "test-custom-value";
        var services = new ServiceCollection();
        services.AddSingleton(_logger);

        services.AddCQRS(builder =>
        {
            builder.AddHandler<TestCommandHandler>();
            builder.AddAdditionalRegistration(svc =>
            {
                svc.AddSingleton(customValue);
            });
        });

        var serviceProvider = services.BuildServiceProvider();
        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();
        var command = new TestCommand { Id = 999 };

        // Act
        await cqrsService.ExecuteCommand(command);
        var retrievedValue = serviceProvider.GetService<string>();

        // Assert - both CQRS and additional registration work
        AssertLogContains("Handling TestCommand with Id: 999");
        Assert.Equal(customValue, retrievedValue);
    }

    // Error Handling Tests
    [Fact]
    public async Task ExecuteCommand_WithoutRegisteredHandler_ThrowsException()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(); // No handlers registered
        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();
        var command = new TestCommand { Id = 999 };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await cqrsService.ExecuteCommand(command));
    }

    [Fact]
    public async Task ExecuteCommand_WithResult_WithoutRegisteredHandler_ThrowsException()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(); // No handlers registered
        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();
        var command = new TestCommandWithResult { Value = 99 };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await cqrsService.ExecuteCommand(command));
    }

    [Fact]
    public async Task ExecuteQuery_WithoutRegisteredHandler_ThrowsException()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(); // No handlers registered
        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();
        var query = new TestQuery { Id = 888 };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await cqrsService.ExecuteQuery(query));
    }

    [Fact]
    public async Task ExecuteQuery_Parameterless_WithoutRegisteredHandler_ThrowsException()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(); // No handlers registered
        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await cqrsService.ExecuteQuery<bool>());
    }
}
