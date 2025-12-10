namespace CodeWinden.CQRS.Tests.Handlers;

// Test commands
public record TestCommand : ICommand;
public record TestCommandWithResult : ICommand<int>;
public record AnotherTestCommand : ICommand;
public record MultiTestCommand : ICommand;
public record MultiAnotherTestCommand : ICommand;

// Test command handlers
public class TestCommandHandler : ICommandHandler<TestCommand>
{
    public Task Handle(TestCommand command, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

public class TestCommandWithResultHandler : ICommandHandler<TestCommandWithResult, int>
{
    public Task<int> Handle(TestCommandWithResult command, CancellationToken cancellationToken)
    {
        return Task.FromResult(42);
    }
}

public class AnotherTestCommandHandler : ICommandHandler<AnotherTestCommand>
{
    public Task Handle(AnotherTestCommand command, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

// Test Handler implementing multiple interfaces
public class MultiInterfaceHandler :
    ICommandHandler<MultiTestCommand>,
    ICommandHandler<MultiAnotherTestCommand>
{
    public Task Handle(MultiTestCommand command, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task Handle(MultiAnotherTestCommand command, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}