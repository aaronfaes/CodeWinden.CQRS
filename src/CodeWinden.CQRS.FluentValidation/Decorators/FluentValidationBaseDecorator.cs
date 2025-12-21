using CodeWinden.CQRS.Decorators;
using FluentValidation;
using FluentValidation.Results;

namespace CodeWinden.CQRS.FluentValidation.Decorators;

/// <summary>
/// Base class for FluentValidation decorators.
/// </summary>
/// <typeparam name="THandler">The type of the handler being decorated.</typeparam>
/// <typeparam name="TRequest">The type of the request being validated.</typeparam>
public abstract class FluentValidationBaseDecorator<THandler, TRequest> :
    BaseDecorator<THandler>
    where THandler : ICQRSHandler
    where TRequest : notnull
{
    /// <summary>
    /// The collection of validators for the request type.
    /// </summary>
    protected readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentValidationBaseDecorator{THandler, TRequest}"/> class.
    /// </summary>
    /// <param name="validators">The collection of validators for the request type.</param>
    public FluentValidationBaseDecorator(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    /// <summary>
    /// Validates the given request using FluentValidation.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="ValidationException">Thrown when validation fails.</exception>
    protected async Task Validate(TRequest request, CancellationToken cancellationToken)
    {
        // Create a new validation context
        var context = new ValidationContext<TRequest>(request);

        // Validate
        var errors = new List<ValidationFailure>();
        foreach (var validator in _validators)
        {
            var validationResult = await validator.ValidateAsync(context, cancellationToken)
                .ConfigureAwait(false);

            if (!validationResult.IsValid)
            {
                errors.AddRange(validationResult.Errors);
            }
        }

        // If there are errors, throw an exception
        if (errors.Any())
        {
            throw new ValidationException(errors);
        }
    }
}