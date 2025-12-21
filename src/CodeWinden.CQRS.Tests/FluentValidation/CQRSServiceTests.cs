using CodeWinden.CQRS.Tests.FluentValidation.Handlers;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CodeWinden.CQRS.Tests.FluentValidation;

/// <summary>
/// End-to-end tests for FluentValidation integration with CQRS.
/// </summary>
public class CQRSServiceTests
{
    private CQRSServiceTestContext CreateServiceProvider()
    {
        var services = new ServiceCollection();
        var tracker = new ExecutionTracker();

        // Register the execution tracker
        services.AddSingleton(tracker);

        // Configure CQRS with FluentValidation
        services.AddCQRS(options =>
        {
            options.AddHandler<ValidatedCommandHandler>();
            options.AddHandler<ValidatedCommandWithResultHandler>();
            options.AddHandler<MultiValidatorCommandHandler>();
            options.AddHandler<ValidatedQueryHandler>();
            options.AddFluentValidation<CQRSServiceTests>();
        });

        var serviceProvider = services.BuildServiceProvider();
        var cqrsService = serviceProvider.GetRequiredService<ICQRSService>();

        return new CQRSServiceTestContext { Service = cqrsService, Tracker = tracker };
    }

    [Fact]
    public async Task ExecuteCommand_WithValidCommand_PassesValidationAndExecutesHandler()
    {
        // Arrange
        var context = CreateServiceProvider();
        var command = new ValidatedCommand { Name = "John Doe", Age = 30 };

        // Act
        await context.Service.ExecuteCommand(command);

        // Assert
        Assert.True(context.Tracker.WasCalled(nameof(ValidatedCommandHandler)));
    }

    [Fact]
    public async Task ExecuteCommand_WithEmptyName_ThrowsValidationException()
    {
        // Arrange
        var context = CreateServiceProvider();
        var command = new ValidatedCommand { Name = "", Age = 30 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            async () => await context.Service.ExecuteCommand(command));

        Assert.Single(exception.Errors);
        Assert.Equal("Name is required", exception.Errors.First().ErrorMessage);

        // Handler should not be called
        Assert.False(context.Tracker.WasCalled(nameof(ValidatedCommandHandler)));
    }

    [Fact]
    public async Task ExecuteCommand_WithInvalidAge_ThrowsValidationException()
    {
        // Arrange
        var context = CreateServiceProvider();
        var command = new ValidatedCommand { Name = "John Doe", Age = 0 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            async () => await context.Service.ExecuteCommand(command));

        Assert.Single(exception.Errors);
        Assert.Equal("Age must be greater than 0", exception.Errors.First().ErrorMessage);

        // Handler should not be called
        Assert.False(context.Tracker.WasCalled(nameof(ValidatedCommandHandler)));
    }

    [Fact]
    public async Task ExecuteCommand_WithAgeTooHigh_ThrowsValidationException()
    {
        // Arrange
        var context = CreateServiceProvider();
        var command = new ValidatedCommand { Name = "John Doe", Age = 200 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            async () => await context.Service.ExecuteCommand(command));

        Assert.Single(exception.Errors);
        Assert.Equal("Age must be less than 150", exception.Errors.First().ErrorMessage);

        // Handler should not be called
        Assert.False(context.Tracker.WasCalled(nameof(ValidatedCommandHandler)));
    }

    [Fact]
    public async Task ExecuteCommand_WithMultipleValidationErrors_ThrowsValidationExceptionWithAllErrors()
    {
        // Arrange
        var context = CreateServiceProvider();
        var command = new ValidatedCommand { Name = "", Age = 0 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            async () => await context.Service.ExecuteCommand(command));

        Assert.Equal(2, exception.Errors.Count());
        Assert.Contains(exception.Errors, e => e.ErrorMessage == "Name is required");
        Assert.Contains(exception.Errors, e => e.ErrorMessage == "Age must be greater than 0");

        // Handler should not be called
        Assert.False(context.Tracker.WasCalled(nameof(ValidatedCommandHandler)));
    }

    [Fact]
    public async Task ExecuteCommand_WithValidCommandWithResult_PassesValidationAndReturnsResult()
    {
        // Arrange
        var context = CreateServiceProvider();
        var command = new ValidatedCommandWithResult { Email = "test@example.com" };

        // Act
        var result = await context.Service.ExecuteCommand<ValidatedCommandWithResult, string>(command);

        // Assert
        Assert.Equal("Processed: test@example.com", result);
        Assert.True(context.Tracker.WasCalled(nameof(ValidatedCommandWithResultHandler)));
    }

    [Fact]
    public async Task ExecuteCommand_WithInvalidEmail_ThrowsValidationException()
    {
        // Arrange
        var context = CreateServiceProvider();
        var command = new ValidatedCommandWithResult { Email = "invalid-email" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            async () => await context.Service.ExecuteCommand<ValidatedCommandWithResult, string>(command));

        Assert.Single(exception.Errors);
        Assert.Equal("Email must be a valid email address", exception.Errors.First().ErrorMessage);

        // Handler should not be called
        Assert.False(context.Tracker.WasCalled(nameof(ValidatedCommandWithResultHandler)));
    }

    [Fact]
    public async Task ExecuteCommand_WithEmptyEmail_ThrowsValidationException()
    {
        // Arrange
        var context = CreateServiceProvider();
        var command = new ValidatedCommandWithResult { Email = "" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            async () => await context.Service.ExecuteCommand<ValidatedCommandWithResult, string>(command));

        Assert.Contains(exception.Errors, e => e.ErrorMessage == "Email is required");

        // Handler should not be called
        Assert.False(context.Tracker.WasCalled(nameof(ValidatedCommandWithResultHandler)));
    }

    [Fact]
    public async Task ExecuteQuery_WithValidQuery_PassesValidationAndReturnsResult()
    {
        // Arrange
        var context = CreateServiceProvider();
        var query = new ValidatedQuery { MinValue = 10, MaxValue = 20 };

        // Act
        var result = await context.Service.ExecuteQuery<ValidatedQuery, int>(query);

        // Assert
        Assert.Equal(10, result);
        Assert.True(context.Tracker.WasCalled(nameof(ValidatedQueryHandler)));
    }

    [Fact]
    public async Task ExecuteQuery_WithMinGreaterThanMax_ThrowsValidationException()
    {
        // Arrange
        var context = CreateServiceProvider();
        var query = new ValidatedQuery { MinValue = 50, MaxValue = 20 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            async () => await context.Service.ExecuteQuery<ValidatedQuery, int>(query));

        Assert.Single(exception.Errors);
        Assert.Equal("MinValue must be less than or equal to MaxValue", exception.Errors.First().ErrorMessage);

        // Handler should not be called
        Assert.False(context.Tracker.WasCalled(nameof(ValidatedQueryHandler)));
    }

    [Fact]
    public async Task ExecuteQuery_WithEqualMinAndMax_PassesValidation()
    {
        // Arrange
        var context = CreateServiceProvider();
        var query = new ValidatedQuery { MinValue = 20, MaxValue = 20 };

        // Act
        var result = await context.Service.ExecuteQuery<ValidatedQuery, int>(query);

        // Assert
        Assert.Equal(0, result);
        Assert.True(context.Tracker.WasCalled(nameof(ValidatedQueryHandler)));
    }

    [Fact]
    public async Task ExecuteCommand_WithMultipleValidators_AllValidatorsAreExecuted()
    {
        // Arrange
        var context = CreateServiceProvider();
        var command = new MultiValidatorCommand { Value = "TestValue" };

        // Act
        await context.Service.ExecuteCommand(command);

        // Assert - Both validators should pass (not empty and >= 5 chars)
        Assert.True(context.Tracker.WasCalled(nameof(MultiValidatorCommandHandler)));
    }

    [Fact]
    public async Task ExecuteCommand_WithMultipleValidators_FailsFirstValidator_ThrowsValidationException()
    {
        // Arrange
        var context = CreateServiceProvider();
        var command = new MultiValidatorCommand { Value = "" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            async () => await context.Service.ExecuteCommand(command));

        Assert.Contains(exception.Errors, e => e.ErrorMessage == "Value is required");

        // Handler should not be called
        Assert.False(context.Tracker.WasCalled(nameof(MultiValidatorCommandHandler)));
    }

    [Fact]
    public async Task ExecuteCommand_WithMultipleValidators_FailsSecondValidator_ThrowsValidationException()
    {
        // Arrange
        var context = CreateServiceProvider();
        var command = new MultiValidatorCommand { Value = "Test" }; // Only 4 characters

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            async () => await context.Service.ExecuteCommand(command));

        Assert.Single(exception.Errors);
        Assert.Equal("Value must be at least 5 characters", exception.Errors.First().ErrorMessage);

        // Handler should not be called
        Assert.False(context.Tracker.WasCalled(nameof(MultiValidatorCommandHandler)));
    }

    [Fact]
    public async Task ExecuteCommand_WithMultipleValidators_FailsBothValidators_ThrowsValidationExceptionWithAllErrors()
    {
        // Arrange
        var context = CreateServiceProvider();
        var command = new MultiValidatorCommand { Value = "" }; // Empty fails both

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            async () => await context.Service.ExecuteCommand(command));

        // Both validators should report errors (empty string fails both NotEmpty and MinimumLength)
        Assert.True(exception.Errors.Count() >= 2);
        Assert.Contains(exception.Errors, e => e.ErrorMessage == "Value is required");
        Assert.Contains(exception.Errors, e => e.ErrorMessage == "Value must be at least 5 characters");

        // Handler should not be called
        Assert.False(context.Tracker.WasCalled(nameof(MultiValidatorCommandHandler)));
    }

    [Fact]
    public async Task ExecuteCommand_WithCancellationToken_PropagatesCancellation()
    {
        // Arrange
        var context = CreateServiceProvider();
        var command = new ValidatedCommand { Name = "John Doe", Age = 30 };

        using (var cts = new CancellationTokenSource())
        {
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                async () => await context.Service.ExecuteCommand(command, cts.Token));

            // Handler should not be called due to cancellation during validation
            Assert.False(context.Tracker.WasCalled(nameof(ValidatedCommandHandler)));
        }
    }

    [Fact]
    public async Task ExecuteQuery_WithCancellationToken_PropagatesCancellation()
    {
        // Arrange
        var context = CreateServiceProvider();
        var query = new ValidatedQuery { MinValue = 10, MaxValue = 20 };

        using (var cts = new CancellationTokenSource())
        {
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                async () => await context.Service.ExecuteQuery<ValidatedQuery, int>(query, cts.Token));

            // Handler should not be called due to cancellation during validation
            Assert.False(context.Tracker.WasCalled(nameof(ValidatedQueryHandler)));
        }
    }
}
