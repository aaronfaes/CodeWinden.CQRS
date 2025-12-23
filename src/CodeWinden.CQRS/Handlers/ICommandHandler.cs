namespace CodeWinden.CQRS;

/// <summary>
/// Marker interface for commands.
/// </summary>
public interface ICommand { }

/// <summary>
/// Marker interface for commands to strongly type the expected result.
/// </summary>
/// <typeparam name="TResult">Type of result expected when executing a command.</typeparam>
public interface ICommand<TResult> { }

/// <summary>
/// Handler for commands without return value.
/// </summary>
/// <typeparam name="TCommand">Type of command to handle.</typeparam>
public interface ICommandHandler<TCommand> : ICQRSHandler
    where TCommand : ICommand
{
    /// <summary>
    /// Handles the given command.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
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
    /// <summary>
    /// Handles the given command.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the command.</returns>
    Task<TResult> Handle(TCommand command, CancellationToken cancellationToken);
}