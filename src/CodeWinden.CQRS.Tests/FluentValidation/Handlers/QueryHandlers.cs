using FluentValidation;

namespace CodeWinden.CQRS.Tests.FluentValidation.Handlers;

public record ValidatedQuery : IQuery<int>
{
    public required int MinValue { get; init; }
    public required int MaxValue { get; init; }
}

public class ValidatedQueryValidator : AbstractValidator<ValidatedQuery>
{
    public ValidatedQueryValidator()
    {
        RuleFor(x => x.MinValue).LessThanOrEqualTo(x => x.MaxValue)
            .WithMessage("MinValue must be less than or equal to MaxValue");
    }
}

public class ValidatedQueryHandler : IQueryHandler<ValidatedQuery, int>
{
    private readonly ExecutionTracker _tracker;

    public ValidatedQueryHandler(ExecutionTracker tracker)
    {
        _tracker = tracker;
    }

    public Task<int> Handle(ValidatedQuery query, CancellationToken cancellationToken)
    {
        _tracker.RecordExecution(nameof(ValidatedQueryHandler));
        return Task.FromResult(query.MaxValue - query.MinValue);
    }
}