using CodeWinden.CQRS.Decorators;
using FluentValidation;

namespace CodeWinden.CQRS.FluentValidation.Decorators;

/// <summary>
/// Decorator for command handlers with result to add FluentValidation support.
/// </summary>
/// <typeparam name="TCommand">The type of the command being handled.</typeparam>
/// <typeparam name="TResult">The type of the result returned by the command handler.</typeparam>
public class FluentValidationCommandWithResultHandlerDecorator<TCommand, TResult> :
    FluentValidationBaseDecorator<ICommandHandler<TCommand, TResult>, TCommand>,
    ICommandHandlerDecorator<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FluentValidationCommandWithResultHandlerDecorator{TCommand, TResult}"/> class.
    /// </summary>
    /// <param name="validators">The collection of validators for the command type.</param>
    public FluentValidationCommandWithResultHandlerDecorator(IEnumerable<IValidator<TCommand>> validators) : base(validators) { }

    /// <summary>
    /// Handles the command after validating it.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The result of the command.</returns>
    public async Task<TResult> Handle(TCommand command, CancellationToken cancellationToken)
    {
        // Validate the command
        await Validate(command, cancellationToken);

        // Proceed to the actual handler
        return await _handler.Handle(command, cancellationToken).ConfigureAwait(false);
    }
}