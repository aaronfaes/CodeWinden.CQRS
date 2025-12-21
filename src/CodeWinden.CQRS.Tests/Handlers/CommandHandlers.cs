using Microsoft.Extensions.Logging;

namespace CodeWinden.CQRS.Tests.Handlers;

// Test commands
public record TestCommand : ICommand
{
    public required int Id { get; init; }
};

public record TestCommandWithResult : ICommand<int>
{
    public required int Value { get; init; }
};

public record AnotherTestCommand : ICommand
{
    public required int Id { get; init; }
};
public record MultiTestCommand : ICommand;
public record MultiAnotherTestCommand : ICommand;

// Test command handlers
public class TestCommandHandler : ICommandHandler<TestCommand>
{
    private readonly ILogger _logger;
    public TestCommandHandler(ILogger logger)
    {
        _logger = logger;
    }

    public Task Handle(TestCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Handling TestCommand with Id: {command.Id}");

        return Task.CompletedTask;
    }
}

public class TestCommandWithResultHandler : ICommandHandler<TestCommandWithResult, int>
{
    public Task<int> Handle(TestCommandWithResult command, CancellationToken cancellationToken)
    {
        return Task.FromResult(command.Value * 42);
    }
}

public class AnotherTestCommandHandler : ICommandHandler<AnotherTestCommand>
{
    private readonly ILogger _logger;
    public AnotherTestCommandHandler(ILogger logger)
    {
        _logger = logger;
    }

    public Task Handle(AnotherTestCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Handling TestCommand with Id: {command.Id}");

        return Task.CompletedTask;
    }
}

// Test Handler implementing multiple interfaces
public class MultiInterfaceHandler :
    ICommandHandler<MultiTestCommand>,
    ICommandHandler<MultiAnotherTestCommand>
{
    private readonly ILogger _logger;

    public MultiInterfaceHandler(ILogger logger)
    {
        _logger = logger;
    }

    public Task Handle(MultiTestCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling MultiTestCommand");
        return Task.CompletedTask;
    }

    public Task Handle(MultiAnotherTestCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling MultiAnotherTestCommand");
        return Task.CompletedTask;
    }
}