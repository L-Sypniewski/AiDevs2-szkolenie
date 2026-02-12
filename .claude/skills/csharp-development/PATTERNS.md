# Enterprise Architecture Patterns

Design pattern guidance emphasizing simplicity, SOLID principles, and avoiding over-engineering.

## Feature Slices Architecture

**Vertical slices over horizontal layers** - organize code by feature, not by technical role.

### Principles

- **Self-contained features** - Each feature folder contains models, handlers, data access, DI registration
- **High cohesion** - Related code stays together
- **Accept duplication** - Keep feature logic within feature boundaries; only extract cross-cutting concerns
- **Clear boundaries** - Features evolve independently

### Structure

```
src/Features/{FeatureName}/
  ├── {Action}.cs              // Endpoint/command handler
  ├── {Model}.cs               // Domain model
  ├── Add{Feature}Services.cs  // DI registration
```

### Extract to Shared Only

- Authentication/authorization
- Logging/telemetry
- Middleware/filters
- Infrastructure services

## Service Registration Extensions

**Single source of truth for DI registration pattern**

| Element | Convention                                                                 |
| ------- | -------------------------------------------------------------------------- |
| File    | `Features/{FeatureName}/Add{FeatureName}Services.cs`                       |
| Class   | `Add{FeatureName}ServicesExtension`                                        |
| Method  | `Add{FeatureName}Services(this IServiceCollection, WebApplicationBuilder)` |
| Return  | `IServiceCollection` for chaining                                          |

**Usage in Program.cs:** `builder.Services.AddInvoicePrintingServices(builder);`

## Options Pattern

**Single source of truth for type-safe configuration.**

| Requirement  | Convention                                   |
| ------------ | -------------------------------------------- |
| Section name | `const string SectionName = "ConfigSection"` |
| Properties   | Use `required` keyword                       |
| Validation   | Data annotations + startup validation        |
| Location     | Options class in feature folder              |

## Repository Pattern with EF Core

**Usually unnecessary** - DbContext already implements Repository + Unit of Work patterns.

### Use DbContext directly when

- Standard CRUD operations
- Need full EF Core capabilities (Include, projections, query filters)
- Feature-specific data access

## Specification Pattern

### Use when

- Complex query logic reused in multiple places
- Business rules need composition (AND/OR logic)
- Queries benefit from isolated testing

### Don't use when

- Simple single-use queries (inline LINQ instead)
- Over-engineering simple data access

## Unit of Work Pattern

**DbContext already implements this.** Use `SaveChangesAsync()` to commit transactions.

## Over-abstraction Warning

**Skip interfaces for single implementations.** Create concrete classes until:

- Multiple implementations exist
- Polymorphism is needed

If testing REALLY requires mocking (prefer TestContainers over mocks) use virtual methods that can be overridden in mocks.

## Key Takeaways

1. **DbContext is Repository + Unit of Work** - Don't wrap unless abstracting non-EF sources
2. **Feature slices over layers** - Keep features independent
3. **Extract only cross-cutting concerns** - Auth, logging, middleware
4. **YAGNI** - Create abstractions only when needed
5. **Specification Pattern** - Only for complex, reusable queries

**Prefer simplicity over abstractions. Build what you need now.**

## References

- [Dependency Injection](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)
- [Configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
