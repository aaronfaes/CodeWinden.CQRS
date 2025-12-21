# CodeWinden.CQRS

An opinionated framework for CQRS, incorporating established best practices.

## Packages

### CodeWinden.CQRS

A minimal CQRS framework that separates commands (writes) from queries (reads) to build maintainable applications with clear separation of concerns.

**Key Features:**
- Zero boilerplate with automatic handler discovery
- Type-safe command and query execution
- Built on Microsoft.Extensions.DependencyInjection
- Configurable handler lifetimes

[**View Documentation →**](src/CodeWinden.CQRS/README.md)

```bash
dotnet add package CodeWinden.CQRS
```

### CodeWinden.CQRS.FluentValidation

Automatic validation extension for CodeWinden.CQRS using FluentValidation to validate commands and queries before handler execution.

**Key Features:**
- Automatic validation before handler execution
- Decorator-based integration
- Fail-fast behavior with clear error messages
- Works with all command and query patterns

[**View Documentation →**](src/CodeWinden.CQRS.FluentValidation/README.md)

```bash
dotnet add package CodeWinden.CQRS.FluentValidation
```

---

## License

MIT License - see [LICENSE](LICENSE.md) for details
