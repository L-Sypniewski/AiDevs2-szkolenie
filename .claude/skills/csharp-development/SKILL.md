---
name: csharp-development
description: Expert guidance for modern .NET/C# development (C# 14 / .NET 10) covering best practices, design patterns, architecture, security, and testing. Use PROACTIVELY when: writing C# code, reviewing .cs files, designing .NET architecture, implementing ASP.NET Core/Blazor applications, working with Entity Framework, or writing .NET tests. Contains: SKILL.md (core principles), PATTERNS.md (architecture), SECURITY.md (mandatory security), TESTING.md (TUnit/TestContainers), ASPNET-CORE.md, BLAZOR.md, ASPIRE.md, and more. DO NOT use for: non-.NET languages, frontend-only JavaScript/TypeScript, or general programming questions unrelated to .NET.
---

# .NET C# Development Expert

## Core Principles

- **Security** - Secure by Design, Validated Types, Zero Trust
- **SOLID** - Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion
- **YAGNI** - Build what's needed now, not what might be needed later
- **Simplicity** - Prefer straightforward solutions over clever abstractions

**Skip interfaces for single implementations.** Create concrete classes until:

- Multiple implementations exist
- Polymorphism is needed

## Modern C# Features

Use latest C# features unless prohibited in this skill's documentation. Check project's `.editorconfig` for language rules.

## Tooling

Use `dotnet` CLI for project operations: adding packages (`dotnet add package`), project references (`dotnet add reference`), scaffolding (`dotnet new`), building/running (`dotnet build`, `dotnet run`).

## Code rules

.editorconfig settings are source of truth. Rely on LSP/analyzers for enforcement of style rules - no need to manually enforce.

**Prefer:** `required`/`init` keywords, records for DTOs, pattern matching over if-else chains

## Critical Requirements

- **Security**: Follow [SECURITY.md](SECURITY.md) guidelines implicitly.
- Use injected `TimeProvider` for all time operations (never `DateTime.Now`)
- Use configuration abstractions (`IOptions<T>`, `IConfiguration`) - never hardcode values
- Write tests for all behavioral changes

## Code Style

- **Comments**: Write WHYs not WHATs - code should be self-documenting. _IMPORTANT_: Avoid adding xml docs
- **Static methods**: Prefer static private methods when no instance state needed
- **Public APIs**: Prefer `IReadOnlyCollection<T>` as return type to expose count and prevent double enumeration
- **Private Methods**: Use specific types (e.g. `int[]` instead of `IEnumerable<int>`) for clarity and performance
- **Immutability**: Prefer records and init-only properties
- **Packages**: Prefer official Microsoft packages over third-party

## Architecture

- Feature slices over horizontal layers
- Use DbContext directly (avoid Repository Pattern over EF Core)
- Create interfaces only when multiple implementations exist
- Constructor injection with readonly fields

## Error Handling

- Exceptions for exceptional cases only - not control flow
- Result pattern for expected failures
- Never swallow exceptions - log and re-throw

## LINQ

- Multi-line for readability
- Use method syntax over query syntax
- Understand deferred vs immediate execution
- Use `.Include()` to avoid N+1 queries

## Skill Files

| File                                                       | Content                                     |
| ---------------------------------------------------------- | ------------------------------------------- |
| [SECURITY.md](SECURITY.md)                                 | Code-level security guidelines (.NET 10)    |
| [PATTERNS.md](PATTERNS.md)                                 | Architecture and design patterns            |
| [ASPNET-CORE.md](ASPNET-CORE.md)                           | ASP.NET Core web application patterns       |
| [BLAZOR.md](BLAZOR.md)                                     | Blazor component architecture               |
| [TESTING.md](TESTING.md)                                   | Testing strategies and tooling              |
| [DBCONTEXT_FEATURE_SLICES.md](DBCONTEXT_FEATURE_SLICES.md) | Database context organization               |
| [ASPIRE.md](ASPIRE.md)                                     | .NET Aspire orchestration and configuration |
| [INFRASTRUCTURE.md](INFRASTRUCTURE.md)                     | Development infrastructure patterns         |
| [CONTAINERIZATION.md](CONTAINERIZATION.md)                 | Containerization and deployment             |

**Write simple, readable, maintainable code. Correctness and clarity before performance optimization.**
