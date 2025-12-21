using CodeWinden.CQRS.Decorators;

namespace CodeWinden.CQRS.Proxies;

/// <summary>
/// Proxy for CQRS command handlers.
/// </summary>
/// <typeparam name="THandler">Type of the command handler.</typeparam>
/// <typeparam name="TCommand">Type of the command.</typeparam>
public class CommandHandlerProxy<THandler, TCommand> :
    ProxyHandlerBase<ICommandHandler<TCommand>, THandler, ICommandHandlerDecorator<TCommand>>,
    ICommandHandler<TCommand>
    where TCommand : ICommand
    where THandler : ICommandHandler<TCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandHandlerProxy{THandler, TCommand}"/> class.
    /// </summary>
    /// <param name="handler">The handler.</param>
    /// <param name="decorators">The decorators.</param>
    public CommandHandlerProxy(THandler handler, IEnumerable<ICommandHandlerDecorator<TCommand>> decorators) : base(handler, decorators) { }

    /// <summary>
    /// Handles the specified command.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task Handle(TCommand command, CancellationToken cancellationToken)
    {
        // Get the decorated handler
        var handler = GetDecoratedHandler();

        // Execute the command using the handler
        return handler.Handle(command, cancellationToken);
    }
}

/// <summary>
/// Proxy for CQRS command handlers with result.
/// </summary>
/// <typeparam name="THandler">Type of the command handler.</typeparam>
/// <typeparam name="TCommand">Type of the command.</typeparam>
/// <typeparam name="TResult">Type of the result.</typeparam>
public class CommandHandlerProxy<THandler, TCommand, TResult> :
    ProxyHandlerBase<ICommandHandler<TCommand, TResult>, THandler, ICommandHandlerDecorator<TCommand, TResult>>,
    ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
    where THandler : ICommandHandler<TCommand, TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandHandlerProxy{THandler, TCommand, TResult}"/> class.
    /// </summary>
    /// <param name="handler">The handler.</param>
    /// <param name="decorators">The decorators.</param>
    public CommandHandlerProxy(THandler handler, IEnumerable<ICommandHandlerDecorator<TCommand, TResult>> decorators) : base(handler, decorators) { }

    /// <summary>
    /// Handles the specified command.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation with the result.</returns>
    public Task<TResult> Handle(TCommand command, CancellationToken cancellationToken)
    {
        // Get the decorated handler
        var handler = GetDecoratedHandler();

        // Execute the command using the handler
        return handler.Handle(command, cancellationToken);
    }
}