namespace CodeWinden.CQRS.Tests.Decorators;

using CodeWinden.CQRS.Decorators;
using CodeWinden.CQRS.Tests.Handlers;

public class TestCommandHandlerDecorator : ICQRSCommandHandlerDecorator<TestCommand>
{
    public Task Handle(TestCommand command, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

public class TestCommandWithResultHandlerDecorator : ICQRSCommandHandlerDecorator<TestCommandWithResult, int>
{
    public Task<int> Handle(TestCommandWithResult command, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

public class GenericCommandHandlerDecorator<TCommand> : ICQRSCommandHandlerDecorator<TCommand>
    where TCommand : ICommand
{
    public Task Handle(TCommand command, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

public class GenericCommandWithResultHandlerDecorator<TCommand, TResult> : ICQRSCommandHandlerDecorator<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    public Task<TResult> Handle(TCommand command, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}