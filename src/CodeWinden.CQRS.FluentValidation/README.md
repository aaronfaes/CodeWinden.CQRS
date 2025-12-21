# CodeWinden.CQRS.FluentValidation

Automatic validation extension for CodeWinden.CQRS using [FluentValidation](https://docs.fluentvalidation.net/) to validate commands and queries before handler execution.

## Features

- **Automatic validation** - Validates commands and queries before handlers execute
- **Decorator-based** - Integrates seamlessly into the CQRS pipeline
- **Zero configuration** - Automatic validator discovery and registration
- **Fail-fast behavior** - Stops execution immediately on validation failure
- **Supports all patterns** - Works with commands and queries with/without return values

## Installation

```bash
dotnet add package CodeWinden.CQRS.FluentValidation
```

## Quick Start

```csharp
using CodeWinden.CQRS;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

// 1. Register CQRS with FluentValidation
var services = new ServiceCollection();
services.AddCQRS(options => options
    .AddHandlersFromAssemblyContaining<Program>()
    .AddFluentValidation<Program>()  // Scans assembly for validators
);

// 2. Define a command
public record CreateUserCommand : ICommand<int>
{
    public required string Name { get; init; }
    public required string Email { get; init; }
}

// 3. Create a validator
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

// 4. Create the handler
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, int>
{
    public async Task<int> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        // Validation runs automatically before this executes
        return await SaveUserToDatabase(command);
    }
}

// 5. Execute - validation happens automatically
var cqrs = serviceProvider.GetRequiredService<ICQRSService>();
try
{
    var userId = await cqrs.ExecuteCommand(
        new CreateUserCommand { Name = "John Doe", Email = "john@example.com" }
    );
}
catch (ValidationException ex)
{
    foreach (var error in ex.Errors)
    {
        Console.WriteLine($"{error.PropertyName}: {error.ErrorMessage}");
    }
}
```

## Usage

### Setting Up Validation

Add FluentValidation to your CQRS configuration:

```csharp
services.AddCQRS(options => options
    .AddHandlersFromAssemblyContaining<Program>()
    .AddFluentValidation<Program>()
);
```

This automatically registers validation decorators and scans the assembly for all `AbstractValidator<T>` implementations.

### Creating Validators

Create validators using FluentValidation's syntax. See the [FluentValidation documentation](https://docs.fluentvalidation.net/) for complete validation rule options.

**Command validator**
```csharp
public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Status).NotEmpty();
    }
}
```

**Query validator**
```csharp
public class GetOrdersByDateRangeQueryValidator : AbstractValidator<GetOrdersByDateRangeQuery>
{
    public GetOrdersByDateRangeQueryValidator()
    {
        RuleFor(x => x.StartDate).LessThan(x => x.EndDate);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
```

**Validator with dependencies**
```csharp
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(IUserRepository repository)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(async (email, ct) => !await repository.EmailExistsAsync(email))
            .WithMessage("Email already in use");
    }
}
```

### Error Handling

FluentValidation throws a `ValidationException` when validation fails:

```csharp
try
{
    await cqrs.ExecuteCommand(command);
}
catch (ValidationException ex)
{
    foreach (var error in ex.Errors)
    {
        Console.WriteLine($"{error.PropertyName}: {error.ErrorMessage}");
    }
}
```

**API error response example**
```csharp
[HttpPost("users")]
public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
{
    try
    {
        var userId = await _cqrs.ExecuteCommand<CreateUserCommand, int>(command);
        return Ok(new { UserId = userId });
    }
    catch (ValidationException ex)
    {
        var errors = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        return BadRequest(new { Errors = errors });
    }
}
```

## How It Works

This extension adds validation decorators to the CQRS pipeline that:
1. Intercept command/query execution before reaching the handler
2. Run all registered validators for the command/query type
3. Collect validation errors from all validators
4. Throw `ValidationException` if any validation fails
5. Allow execution to continue to the handler only if validation succeeds

Multiple validators for the same command/query type will all execute, and validation fails if any validator finds errors.

## API Reference

| Method/Decorator | Description |
|------------------|-------------|
| `AddFluentValidation<TAssembly>()` | Adds validation to CQRS pipeline and scans assembly for validators |
| `FluentValidationCommandHandlerDecorator<TCommand>` | Validates commands without return value |
| `FluentValidationCommandWithResultHandlerDecorator<TCommand, TResult>` | Validates commands with return value |
| `FluentValidationQueryHandlerDecorator<TQuery, TResult>` | Validates queries with parameters |

## Best Practices

**Keep validators focused on input validation**
```csharp
// ✅ Validate input format and constraints
RuleFor(x => x.Email).NotEmpty().EmailAddress();
RuleFor(x => x.Price).GreaterThan(0);

// ❌ Don't perform business operations
RuleFor(x => x).Must(x => SaveToDatabase(x));
```

**Use meaningful error messages**
```csharp
RuleFor(x => x.Email)
    .EmailAddress()
    .WithMessage("Please provide a valid email address");
```

**Validators support dependency injection**
```csharp
public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator(IUserRepository repository)
    {
        // Use injected dependencies for async validation
    }
}
```

> **Note:** Validators are registered with Scoped lifetime by default.

> **Tip:** For detailed validator creation, conditional validation, custom validators, and more, see the [FluentValidation documentation](https://docs.fluentvalidation.net/).

## License

MIT License - see [LICENSE](LICENSE.md) for details
