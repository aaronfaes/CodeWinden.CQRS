namespace CodeWinden.CQRS.Decorators;

/// <summary>
/// Decorator for CQRS command handlers.
/// </summary>
/// <typeparam name="TCommand"></typeparam>
public interface ICommandHandlerDecorator<TCommand> :
    ICommandHandler<TCommand>,
    ICQRSHandlerDecorator<ICommandHandler<TCommand>>
    where TCommand : ICommand
{ }

/// <summary>
/// Decorator for CQRS command handlers with result.
/// </summary>
/// <typeparam name="TCommand">Type of the command.</typeparam>
/// <typeparam name="TResult">Type of the result.</typeparam>
public interface ICommandHandlerDecorator<TCommand, TResult> :
    ICommandHandler<TCommand, TResult>,
    ICQRSHandlerDecorator<ICommandHandler<TCommand, TResult>>
    where TCommand : ICommand<TResult>
{ }
