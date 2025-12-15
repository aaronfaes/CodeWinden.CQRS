using System.Collections.Concurrent;
using CodeWinden.CQRS.Proxies;
using Microsoft.Extensions.DependencyInjection;

namespace CodeWinden.CQRS;

/// <summary>
/// Implementation of the CQRS service that resolves and executes command and query handlers.
/// </summary>
public class CQRSService : ICQRSService
{
    /// <summary>
    /// Service provider for resolving handlers.
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="CQRSService"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider for resolving handlers.</param>
    public CQRSService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Executes a command without return value.
    /// </summary>
    /// <typeparam name="TCommand">Type of command to execute.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task ExecuteCommand<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        // Resolve the appropriate command handler
        var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();

        // Execute the command using the handler
        return handler.Handle(command, cancellationToken);
    }

    /// <summary>
    /// Executes a command with return value.
    /// </summary>
    /// <typeparam name="TCommand">Type of command to execute.</typeparam>
    /// <typeparam name="TResult">Type of result expected from the command.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the command.</returns>
    public Task<TResult> ExecuteCommand<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>
    {
        // Resolve the appropriate command handler
        var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>();

        // Execute the command using the handler
        return handler.Handle(command, cancellationToken);
    }

    /// <summary>
    /// Executes a query without parameters and returns the result.
    /// </summary>
    /// <typeparam name="TResult">Type of result expected from the query.</typeparam>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the query.</returns>
    public Task<TResult> ExecuteQuery<TResult>(CancellationToken cancellationToken = default)
    {
        // Resolve the appropriate query handler
        var handler = _serviceProvider.GetRequiredService<IQueryHandler<TResult>>();

        // Execute the query using the handler
        return handler.Handle(cancellationToken);
    }

    /// <summary>
    /// Executes a query with parameters and returns the result.
    /// </summary>
    /// <typeparam name="TQuery">Type of query to execute.</typeparam>
    /// <typeparam name="TResult">Type of result expected from the query.</typeparam>
    /// <param name="query">The query to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the query.</returns>
    public Task<TResult> ExecuteQuery<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>
    {
        // Resolve the appropriate query handler
        var handler = _serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();

        // Execute the query using the handler
        return handler.Handle(query, cancellationToken);
    }
}