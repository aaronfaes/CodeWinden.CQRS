# CodeWinden.CQRS

A minimal, opinionated CQRS framework for .NET that separates commands (writes) from queries (reads) to build maintainable applications with clear separation of concerns.

## Features

- **Zero boilerplate** - Automatic handler discovery and registration
- **Type-safe execution** - Strongly-typed commands, queries, and results
- **Flexible configuration** - Register handlers by assembly scanning or explicit registration
- **Decorator support** - Add cross-cutting concerns like logging, validation, and caching without modifying handlers
- **Dependency injection ready** - Built on `Microsoft.Extensions.DependencyInjection`
- **Supports multiple patterns** - Commands with/without return values, queries with/without parameters
- **Configurable lifetimes** - Control handler and decorator service lifetimes (Scoped, Transient, Singleton)

## Installation

```bash
dotnet add package CodeWinden.CQRS
```

## Quick Start

```csharp
using CodeWinden.CQRS;
using Microsoft.Extensions.DependencyInjection;

// 1. Register CQRS services
var services = new ServiceCollection();
services.AddCQRS(options => options
    .AddHandlersFromAssemblyContaining<Program>()
);
var serviceProvider = services.BuildServiceProvider();

// 2. Define a command
public record CreateUserCommand : ICommand<int>
{
    public required string Name { get; init; }
    public required string Email { get; init; }
}

// 3. Create the handler
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, int>
{
    public async Task<int> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        // Save user to database
        return 123; // Return new user ID
    }
}

// 4. Execute the command
var cqrs = serviceProvider.GetRequiredService<ICQRSService>();
var userId = await cqrs.ExecuteCommand(
    new CreateUserCommand { Name = "John Doe", Email = "john@example.com" }
);
```

## Usage

### Registering Handlers

**Option 1: Automatic assembly scanning (recommended)**
```csharp
services.AddCQRS(options => options
    .AddHandlersFromAssemblyContaining<Program>()
);
```

**Option 2: Explicit handler registration**
```csharp
services.AddCQRS(options => options
    .AddHandler<CreateUserCommandHandler>()
    .AddHandler<GetUserQueryHandler>()
);
```

**Option 3: Configure handler lifetime**
```csharp
services.AddCQRS(options => options
    .AddHandlersFromAssemblyContaining<Program>()
    .WithHandlerLifetime(ServiceLifetime.Transient)
);
// Default lifetime is Scoped
```

### Commands

Commands represent write operations that change system state.

**Command without return value**
```csharp
// Define the command
public record DeleteUserCommand : ICommand
{
    public required int UserId { get; init; }
}

// Create the handler
public class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand>
{
    private readonly IUserRepository _repository;
    
    public DeleteUserCommandHandler(IUserRepository repository)
    {
        _repository = repository;
    }
    
    public async Task Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        await _repository.DeleteAsync(command.UserId);
    }
}

// Execute the command
await cqrs.ExecuteCommand(new DeleteUserCommand { UserId = 123 });
```

**Command with return value**
```csharp
// Define the command
public record UpdateUserCommand : ICommand<bool>
{
    public required int Id { get; init; }
    public required string Name { get; init; }
}

// Create the handler
public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, bool>
{
    private readonly IUserRepository _repository;
    
    public UpdateUserCommandHandler(IUserRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<bool> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(command.Id);
        if (user == null) return false;
        
        user.Name = command.Name;
        await _repository.UpdateAsync(user);
        return true;
    }
}

// Execute the command
var success = await cqrs.ExecuteCommand<UpdateUserCommand, bool>(
    new UpdateUserCommand { Id = 123, Name = "Jane Doe" }
);
```

### Queries

Queries represent read operations that retrieve data without modifying state.

**Query with parameters**
```csharp
// Define the query and result DTO
public record GetUserQuery : IQuery<UserDto>
{
    public required int UserId { get; init; }
}

public record UserDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
}

// Create the handler
public class GetUserQueryHandler : IQueryHandler<GetUserQuery, UserDto>
{
    private readonly IUserRepository _repository;
    
    public GetUserQueryHandler(IUserRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<UserDto> Handle(GetUserQuery query, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(query.UserId);
        return new UserDto { Id = user.Id, Name = user.Name, Email = user.Email };
    }
}

// Execute the query
var user = await cqrs.ExecuteQuery<GetUserQuery, UserDto>(
    new GetUserQuery { UserId = 123 }
);
```

**Query without parameters**
```csharp
// Define the result DTO (no query class needed)
public record SystemStatsDto
{
    public required int TotalUsers { get; init; }
    public required int ActiveSessions { get; init; }
}

// Create the handler
public class GetSystemStatsQueryHandler : IQueryHandler<SystemStatsDto>
{
    private readonly IStatsService _statsService;
    
    public GetSystemStatsQueryHandler(IStatsService statsService)
    {
        _statsService = statsService;
    }
    
    public async Task<SystemStatsDto> Handle(CancellationToken cancellationToken)
    {
        var users = await _statsService.GetTotalUsersAsync();
        var sessions = await _statsService.GetActiveSessionsAsync();
        return new SystemStatsDto { TotalUsers = users, ActiveSessions = sessions };
    }
}

// Execute the query
var stats = await cqrs.ExecuteQuery<SystemStatsDto>();
```

### Dependency Injection in Handlers

Handlers support constructor injection for accessing services:

```csharp
public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, int>
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<CreateOrderCommandHandler> _logger;
    private readonly IEmailService _emailService;
    
    public CreateOrderCommandHandler(
        IOrderRepository repository,
        ILogger<CreateOrderCommandHandler> logger,
        IEmailService emailService)
    {
        _repository = repository;
        _logger = logger;
        _emailService = emailService;
    }
    
    public async Task<int> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating order for {CustomerId}", command.CustomerId);
        
        var orderId = await _repository.CreateAsync(command);
        await _emailService.SendOrderConfirmationAsync(orderId);
        
        return orderId;
    }
}
```

### Decorators

Decorators wrap handlers to add cross-cutting concerns like logging, validation, caching, or transaction management without modifying handler code.

**Creating a logging decorator**
```csharp
using CodeWinden.CQRS.Decorators;
using Microsoft.Extensions.Logging;

// Generic decorator that wraps all command handlers
public class LoggingCommandDecorator<TCommand> : 
    BaseDecorator<ICommandHandler<TCommand>>,
    ICommandHandlerDecorator<TCommand>
    where TCommand : ICommand
{
    private readonly ILogger<LoggingCommandDecorator<TCommand>> _logger;
    
    public LoggingCommandDecorator(ILogger<LoggingCommandDecorator<TCommand>> logger)
    {
        _logger = logger;
    }
    
    public async Task Handle(TCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing command: {CommandType}", typeof(TCommand).Name);
        
        try
        {
            // Call the wrapped handler via _handler (injected by framework)
            await _handler.Handle(command, cancellationToken);
            _logger.LogInformation("Command executed successfully: {CommandType}", typeof(TCommand).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Command failed: {CommandType}", typeof(TCommand).Name);
            throw;
        }
    }
}

// Register the decorator - applies to all commands
services.AddCQRS(options => options
    .AddHandlersFromAssemblyContaining<Program>()
    .AddDecorator(typeof(LoggingCommandDecorator<>))
);
```

**Combining multiple decorators**
```csharp
// Decorators execute in registration order (first registered = outermost)
services.AddCQRS(options => options
    .AddHandlersFromAssemblyContaining<Program>()
    .AddDecorator(typeof(LoggingCommandDecorator<>))           // Executes first (outer)
    .AddDecorator(typeof(ValidationCommandDecorator<,>))       // Executes second
    .AddDecorator(typeof(TransactionCommandDecorator<>))       // Executes third (inner)
);

// Execution flow: Logging → Validation → Transaction → Handler → Transaction → Validation → Logging
```

Decorators support dependency injection and can be registered with different lifetimes (`Scoped`, `Transient`, `Singleton`). Available interfaces: `ICommandHandlerDecorator<TCommand>`, `ICommandHandlerDecorator<TCommand, TResult>`, `IQueryHandlerDecorator<TResult>`, `IQueryHandlerDecorator<TQuery, TResult>`.

## API Reference

### Core Interfaces

| Interface | Purpose |
|-----------|---------|
| `ICQRSService` | Central service for executing commands and queries |
| `ICommand` | Marker interface for commands without return value |
| `ICommand<TResult>` | Marker interface for commands with return value |
| `IQuery<TResult>` | Marker interface for queries with parameters |
| `ICommandHandler<TCommand>` | Handler for commands without return value |
| `ICommandHandler<TCommand, TResult>` | Handler for commands with return value |
| `IQueryHandler<TResult>` | Handler for queries without parameters |
| `IQueryHandler<TQuery, TResult>` | Handler for queries with parameters |
| `ICommandHandlerDecorator<TCommand>` | Decorator for commands without return value |
| `ICommandHandlerDecorator<TCommand, TResult>` | Decorator for commands with return value |
| `IQueryHandlerDecorator<TResult>` | Decorator for parameterless queries |
| `IQueryHandlerDecorator<TQuery, TResult>` | Decorator for queries with parameters |
| `BaseDecorator<THandler>` | Base class for creating decorators with automatic handler injection |

### Configuration Methods

| Method | Description |
|--------|-------------|
| `AddCQRS(Action<CQRSOptionsBuilder>)` | Registers CQRS services with configuration |
| `AddHandler<THandler>()` | Explicitly registers a single handler |
| `AddHandlersFromAssemblyContaining<T>()` | Scans assembly for all handlers |
| `WithHandlerLifetime(ServiceLifetime)` | Sets handler lifetime (Scoped, Transient, Singleton) |
| `AddDecorator<TDecorator>()` | Registers a decorator to wrap handlers |
| `AddDecorator(Type)` | Registers a generic decorator type (e.g., `typeof(LoggingDecorator<>)`) |

## Best Practices

**Use records for commands and queries**
```csharp
// ✅ Immutable with value-based equality
public record CreateUserCommand : ICommand<int>
{
    public required string Name { get; init; }
    public required string Email { get; init; }
}

// ❌ Mutable
public class CreateUserCommand : ICommand<int>
{
    public string Name { get; set; }
    public string Email { get; set; }
}
```

**Keep handlers focused and single-purpose**
```csharp
// ✅ One handler per command/query
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, int> { }

// ❌ Don't handle multiple unrelated operations in one handler
```

**Separate DTOs from domain models**
```csharp
// ✅ Use DTOs for query results
public record UserDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
}

// ❌ Don't expose domain entities directly
```

**Use meaningful naming conventions**
- Commands: `{Verb}{Entity}Command` (e.g., `CreateUserCommand`, `UpdateOrderCommand`)
- Queries: `Get{Entity}Query`, `Find{Entities}Query` (e.g., `GetUserQuery`, `FindActiveOrdersQuery`)
- Handlers: `{CommandOrQuery}Handler` (e.g., `CreateUserCommandHandler`)

**Handle cancellation tokens properly**
```csharp
public async Task<UserDto> Handle(GetUserQuery query, CancellationToken cancellationToken)
{
    // Pass cancellation token to async operations
    var user = await _repository.GetByIdAsync(query.UserId, cancellationToken);
    return MapToDto(user);
}
```

**Use decorators for cross-cutting concerns**
```csharp
// ✅ Add logging, validation, caching via decorators
public class LoggingCommandDecorator<TCommand> : 
    BaseDecorator<ICommandHandler<TCommand>>,
    ICommandHandlerDecorator<TCommand>
    where TCommand : ICommand
{
    // Decorator implementation
}

// ❌ Don't add cross-cutting logic directly in handlers
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, int>
{
    public async Task<int> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating user..."); // Avoid - use decorator instead
        // Handler logic
    }
}
```

> **Note:** Handlers are registered with `TryAdd`, so if you manually register a handler before calling `AddCQRS()`, your registration takes precedence.

## License

MIT License - see [LICENSE](LICENSE.md) for details
