namespace CodeWinden.CQRS.Decorators;

/// <summary>
/// Decorator for CQRS command handlers.
/// </summary>
/// <typeparam name="TCommand"></typeparam>
public interface ICQRSCommandHandlerDecorator<TCommand> :
    ICommandHandler<TCommand>,
    ICQRSHandlerDecorator
    where TCommand : ICommand
{ }

/// <summary>
/// Decorator for CQRS command handlers with result.
/// </summary>
/// <typeparam name="TCommand">Type of the command.</typeparam>
/// <typeparam name="TResult">Type of the result.</typeparam>
public interface ICQRSCommandHandlerDecorator<TCommand, TResult> :
    ICommandHandler<TCommand, TResult>,
    ICQRSHandlerDecorator
    where TCommand : ICommand<TResult>
{ }
