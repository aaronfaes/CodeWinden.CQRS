using CodeWinden.CQRS.Decorators;
using FluentValidation;

namespace CodeWinden.CQRS.FluentValidation.Decorators;

/// <summary>
/// Decorator for query handlers to add FluentValidation support.
/// </summary>
/// <typeparam name="TQuery">The type of the query being handled.</typeparam>
/// <typeparam name="TResult">The type of the result returned by the query handler.</typeparam>
public class FluentValidationQueryHandlerDecorator<TQuery, TResult> :
    FluentValidationBaseDecorator<IQueryHandler<TQuery, TResult>, TQuery>,
    IQueryHandlerDecorator<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FluentValidationQueryHandlerDecorator{TQuery, TResult}"/> class.
    /// </summary>
    /// <param name="validators">The collection of validators for the query type.</param>
    public FluentValidationQueryHandlerDecorator(IEnumerable<IValidator<TQuery>> validators) : base(validators) { }

    /// <summary>
    /// Handles the query after validating it.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The result of the query.</returns>
    public async Task<TResult> Handle(TQuery query, CancellationToken cancellationToken)
    {
        // Validate the query
        await Validate(query, cancellationToken);

        // Proceed to the actual handler
        return await _handler.Handle(query, cancellationToken).ConfigureAwait(false);
    }
}
