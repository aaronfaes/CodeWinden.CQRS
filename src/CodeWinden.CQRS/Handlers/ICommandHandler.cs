namespace CodeWinden.CQRS;

/// <summary>
/// Marker interface for commands
/// </summary>
public interface ICommand { }

/// <summary>
/// Marker interface for queries to strongly type the expected result.
/// </summary>
/// <typeparam name="TResult"></typeparam>
public interface ICommand<TResult> : ICommand { }

/// <summary>
/// Handler for commands without return value
/// </summary>
/// <typeparam name="TCommand">Type of command to handle</typeparam>
public interface ICommandHandler<TCommand> : ICQRSHandler
    where TCommand : ICommand
{
    Task Handle(TCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// Handler for commands with return value.
/// </summary>
/// <typeparam name="TCommand">Type of command to handle.</typeparam>
/// <typeparam name="TResult">Type of result expected when executing a command.</typeparam>
public interface ICommandHandler<TCommand, TResult> : ICQRSHandler
    where TCommand : ICommand<TResult>
{
    Task<TResult> Handle(TCommand command, CancellationToken cancellationToken);
}