namespace CodeWinden.CQRS;

/// <summary>
/// Central service for executing commands and queries in a CQRS architecture.
/// </summary>
public interface ICQRSService
{
    /// <summary>
    /// Executes a query without parameters and returns the result.
    /// </summary>
    /// <typeparam name="TResult">Type of result expected from the query.</typeparam>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the query.</returns>
    Task<TResult> ExecuteQuery<TResult>(CancellationToken cancellationToken = default);
    /// <summary>
    /// Executes a query with parameters and returns the result.
    /// </summary>
    /// <typeparam name="TQuery">Type of query to execute.</typeparam>
    /// <typeparam name="TResult">Type of result expected from the query.</typeparam>
    /// <param name="query">The query to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the query.</returns>
    Task<TResult> ExecuteQuery<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>;
    /// <summary>
    /// Executes a command without return value.
    /// </summary>
    /// <typeparam name="TCommand">Type of command to execute.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ExecuteCommand<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand;
    /// <summary>
    /// Executes a command with return value.
    /// </summary>
    /// <typeparam name="TCommand">Type of command to execute.</typeparam>
    /// <typeparam name="TResult">Type of result expected from the command.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the command.</returns>
    Task<TResult> ExecuteCommand<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>;
}
