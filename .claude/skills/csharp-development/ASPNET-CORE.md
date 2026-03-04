# ASP.NET Core Patterns

Production patterns for ASP.NET Core applications.

## Service Registration (DI)

See [PATTERNS.md](PATTERNS.md) for Service Registration Extensions pattern and naming conventions.

## Options Pattern

See [PATTERNS.md](PATTERNS.md) for Options Pattern guidelines.

## Minimal APIs

**Use Minimal APIs by default** for lightweight HTTP APIs without controllers.

### Principles

- Group endpoints in `IEndpointRouteBuilder` extension methods
- Organize in feature folders with related code
- Single file for simple endpoints (DTOs and handler together)
- Extract to handlers for complex logic

### Naming Conventions

| Element      | Convention                                          |
| ------------ | --------------------------------------------------- |
| File         | `Features/{Feature}/{Feature}Endpoints.cs`          |
| Class        | `{Feature}Endpoints`                                |
| Method       | `Map{Feature}Endpoints(this IEndpointRouteBuilder)` |
| Registration | `app.MapOrderEndpoints();` in Program.cs            |

### Guidelines

- **Security**: Apply [SECURITY.md](SECURITY.md) patterns (Rate Limiting, HSTS, Secure Headers).
- **Auth**: Prefer `MapGroup().RequireAuthorization()` over individual attribute checks.
- Use `Results<TOk, TNotFound, TBadRequest>` and `TypedResults` for type-safe responses
- Define request/response DTOs inline with record types for simple endpoints
- Add cross-cutting concerns with endpoint filters (`IEndpointFilter`)

### Feature Folder Organization

```
Features/Orders/
  ├── OrderEndpoints.cs       // Endpoint mappings
  └── AddOrderServices.cs     // DI registration
```

[Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)

## Background Services

Use `BackgroundService` base class for long-running tasks.

### When to use

- Async message/queue processing
- Scheduled jobs
- Long-running operations

### Guidelines

- Register with `services.AddHostedService<TService>()`
- Handle idempotency for at-least-once delivery
- Use dead letter queues for failed messages

## References

- [ASP.NET Core Fundamentals](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/)
- [Dependency Injection](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)
