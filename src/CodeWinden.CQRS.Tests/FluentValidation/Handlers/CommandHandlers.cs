using FluentValidation;

namespace CodeWinden.CQRS.Tests.FluentValidation.Handlers;

public record ValidatedCommand : ICommand
{
    public required string Name { get; init; }
    public required int Age { get; init; }
}

public record ValidatedCommandWithResult : ICommand<string>
{
    public required string Email { get; init; }
}

public record MultiValidatorCommand : ICommand
{
    public required string Value { get; init; }
}

public class ValidatedCommandValidator : AbstractValidator<ValidatedCommand>
{
    public ValidatedCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.Age).GreaterThan(0).WithMessage("Age must be greater than 0");
        RuleFor(x => x.Age).LessThan(150).WithMessage("Age must be less than 150");
    }
}

public class ValidatedCommandWithResultValidator : AbstractValidator<ValidatedCommandWithResult>
{
    public ValidatedCommandWithResultValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required");
        RuleFor(x => x.Email).EmailAddress().WithMessage("Email must be a valid email address");
    }
}

public class MultiValidatorCommand_FirstValidator : AbstractValidator<MultiValidatorCommand>
{
    public MultiValidatorCommand_FirstValidator()
    {
        RuleFor(x => x.Value).NotEmpty().WithMessage("Value is required");
    }
}

public class MultiValidatorCommand_SecondValidator : AbstractValidator<MultiValidatorCommand>
{
    public MultiValidatorCommand_SecondValidator()
    {
        RuleFor(x => x.Value).MinimumLength(5).WithMessage("Value must be at least 5 characters");
    }
}

public class ValidatedCommandHandler : ICommandHandler<ValidatedCommand>
{
    private readonly ExecutionTracker _tracker;

    public ValidatedCommandHandler(ExecutionTracker tracker)
    {
        _tracker = tracker;
    }

    public Task Handle(ValidatedCommand command, CancellationToken cancellationToken)
    {
        _tracker.RecordExecution(nameof(ValidatedCommandHandler));
        return Task.CompletedTask;
    }
}

public class ValidatedCommandWithResultHandler : ICommandHandler<ValidatedCommandWithResult, string>
{
    private readonly ExecutionTracker _tracker;

    public ValidatedCommandWithResultHandler(ExecutionTracker tracker)
    {
        _tracker = tracker;
    }

    public Task<string> Handle(ValidatedCommandWithResult command, CancellationToken cancellationToken)
    {
        _tracker.RecordExecution(nameof(ValidatedCommandWithResultHandler));
        return Task.FromResult($"Processed: {command.Email}");
    }
}

public class MultiValidatorCommandHandler : ICommandHandler<MultiValidatorCommand>
{
    private readonly ExecutionTracker _tracker;

    public MultiValidatorCommandHandler(ExecutionTracker tracker)
    {
        _tracker = tracker;
    }

    public Task Handle(MultiValidatorCommand command, CancellationToken cancellationToken)
    {
        _tracker.RecordExecution(nameof(MultiValidatorCommandHandler));
        return Task.CompletedTask;
    }
}
