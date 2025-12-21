using System;
using CodeWinden.CQRS.Decorators;
using FluentValidation;

namespace CodeWinden.CQRS.FluentValidation.Decorators;

/// <summary>
/// Decorator for command handlers to add FluentValidation support.
/// </summary>
/// <typeparam name="TCommand">The type of the command being handled.</typeparam>
public class FluentValidationCommandHandlerDecorator<TCommand> :
    FluentValidationBaseDecorator<ICommandHandler<TCommand>, TCommand>,
    ICommandHandlerDecorator<TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FluentValidationCommandHandlerDecorator{TCommand}"/> class.
    /// </summary>
    /// <param name="validators">The collection of validators for the command type.</param>
    public FluentValidationCommandHandlerDecorator(IEnumerable<IValidator<TCommand>> validators) : base(validators) { }

    /// <summary>
    /// Handles the command after validating it.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Handle(TCommand command, CancellationToken cancellationToken = default)
    {
        // Validate the command
        await Validate(command, cancellationToken);

        // Proceed to the actual handler
        await _handler.Handle(command, cancellationToken).ConfigureAwait(false);
    }
}
