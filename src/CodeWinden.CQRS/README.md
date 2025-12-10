# CodeWinden.CQRS

A minimal, opinionated CQRS framework for .NET that separates commands (writes) from queries (reads) to build maintainable applications with clear separation of concerns.

## Features

- **Zero boilerplate** - Automatic handler discovery and registration
- **Type-safe execution** - Strongly-typed commands, queries, and results
- **Flexible configuration** - Register handlers by assembly scanning or explicit registration
- **Dependency injection ready** - Built on `Microsoft.Extensions.DependencyInjection`
- **Supports multiple patterns** - Commands with/without return values, queries with/without parameters
- **Configurable lifetimes** - Control handler service lifetimes (Scoped, Transient, Singleton)

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

### Configuration Methods

| Method | Description |
|--------|-------------|
| `AddCQRS(Action<CQRSOptionsBuilder>)` | Registers CQRS services with configuration |
| `AddHandler<THandler>()` | Explicitly registers a single handler |
| `AddHandlersFromAssemblyContaining<T>()` | Scans assembly for all handlers |
| `WithHandlerLifetime(ServiceLifetime)` | Sets handler lifetime (Scoped, Transient, Singleton) |

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

> **Note:** Handlers are registered with `TryAdd`, so if you manually register a handler before calling `AddCQRS()`, your registration takes precedence.

## License

MIT License - see LICENSE file for details