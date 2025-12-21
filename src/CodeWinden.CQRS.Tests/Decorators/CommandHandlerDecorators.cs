using CodeWinden.CQRS.Decorators;
using CodeWinden.CQRS.Tests.Handlers;
using Microsoft.Extensions.Logging;

namespace CodeWinden.CQRS.Tests.Decorators;

public abstract class AbstractTestDecorator :
    BaseDecorator<ICommandHandler<TestCommand>>,
    ICommandHandlerDecorator<TestCommand>
{
    public abstract Task Handle(TestCommand command, CancellationToken cancellationToken = default);
}

public interface ITestCommandHandlerDecorator : ICommandHandlerDecorator<TestCommand> { }
public class TestCommandHandlerDecorator :
    BaseDecorator<ICommandHandler<TestCommand>>,
    ITestCommandHandlerDecorator
{
    private readonly ILogger _logger;

    public TestCommandHandlerDecorator(ILogger logger)
    {
        _logger = logger;
    }

    public Task Handle(TestCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Decorator modifying TestCommand Id {command.Id}.");
        return _handler.Handle(command, cancellationToken);
    }
}

public class TestCommandWithResultHandlerDecorator :
    BaseDecorator<ICommandHandler<TestCommandWithResult, int>>,
    ICommandHandlerDecorator<TestCommandWithResult, int>
{
    private readonly ILogger _logger;

    public TestCommandWithResultHandlerDecorator(ILogger logger)
    {
        _logger = logger;
    }

    public Task<int> Handle(TestCommandWithResult command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Decorator modifying TestCommandWithResult Value {command.Value}.");
        return _handler.Handle(command, cancellationToken);
    }
}

public class GenericCommandHandlerDecorator<TCommand> :
    BaseDecorator<ICommandHandler<TCommand>>,
    ICommandHandlerDecorator<TCommand>
    where TCommand : ICommand
{
    private readonly ILogger _logger;

    public GenericCommandHandlerDecorator(ILogger logger)
    {
        _logger = logger;
    }

    public Task Handle(TCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Generic decorator handling command of type {typeof(TCommand).Name}.");
        return _handler.Handle(command, cancellationToken);
    }
}

public class GenericCommandWithResultHandlerDecorator<TCommand, TResult> :
    BaseDecorator<ICommandHandler<TCommand, TResult>>,
    ICommandHandlerDecorator<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly ILogger _logger;

    public GenericCommandWithResultHandlerDecorator(ILogger logger)
    {
        _logger = logger;
    }

    public Task<TResult> Handle(TCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Generic decorator handling command of type {typeof(TCommand).Name} with result type {typeof(TResult).Name}.");
        return _handler.Handle(command, cancellationToken);
    }
}

// Test decorator that implements multiple decorator interfaces
public class MultiInterfaceDecorator :
    ICommandHandlerDecorator<TestCommand>,
    ICommandHandlerDecorator<TestCommandWithResult, int>
{
    private ICommandHandler<TestCommand> _commandHandler = null!;
    private ICommandHandler<TestCommandWithResult, int> _commandWithResultHandler = null!;

    public Task Handle(TestCommand command, CancellationToken cancellationToken = default)
    {
        return _commandHandler.Handle(command, cancellationToken);
    }

    public Task<int> Handle(TestCommandWithResult command, CancellationToken cancellationToken = default)
    {
        return _commandWithResultHandler.Handle(command, cancellationToken);
    }

    void ICQRSHandlerDecorator<ICommandHandler<TestCommand>>.SetHandler(ICommandHandler<TestCommand> handler)
    {
        _commandHandler = handler;
    }

    void ICQRSHandlerDecorator<ICommandHandler<TestCommandWithResult, int>>.SetHandler(ICommandHandler<TestCommandWithResult, int> handler)
    {
        _commandWithResultHandler = handler;
    }
}