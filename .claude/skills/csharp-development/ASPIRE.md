# .NET Aspire

Code-first orchestration for cloud-native .NET applications.

## Scaffolding

- Install templates: `dotnet new install Aspire.ProjectTemplates`
- New Aspire app: `dotnet new aspire` (full) or `dotnet new aspire-starter` (minimal)

## Telemetry Dashboard

Built-in observability at `http://localhost:PORT` when running AppHost:

- Distributed traces (OpenTelemetry)
- Aggregated logs from all services
- Metrics (request rates, durations, errors)
- Resource health (databases, caches)

## Integration Testing

Use `DistributedApplicationTestingBuilder` to create test AppHost and start entire application stack for integration tests.

## Environment-Agnostic Configuration

**Critical Principle**: Use Aspire's environment variable conventions in production to ensure code works identically across all environments.

### The Convention

Aspire has two package types:
- **Hosting packages** (`Aspire.Hosting.*`): Used in AppHost to orchestrate infrastructure
- **Client integrations** (`Aspire.Npgsql`, `Aspire.StackExchange.Redis`, etc.): Used in services to connect

The AppHost injects configuration using the pattern `ConnectionStrings__{resourceName}` where:
- Double underscore (`__`) separates hierarchical levels
- In code, access via `IConfiguration["ConnectionStrings:resourceName"]` (colon notation)
- .NET automatically translates `ConnectionStrings:db` ↔ `ConnectionStrings__db`

**Client integrations read from IConfiguration and work identically with or without AppHost.**

**In production, use THE SAME naming convention** (`ConnectionStrings__resourceName`). Don't create custom variable names.

### Configuration Pattern

```csharp
// AppHost (development only—uses Aspire.Hosting.* packages)
var db = builder.AddPostgres("postgres").AddDatabase("db");
var cache = builder.AddRedis("cache");
builder.AddProject<Projects.MyService>("myservice")
    .WithReference(db)    // Creates ConnectionStrings__db
    .WithReference(cache); // Creates ConnectionStrings__cache

// Service code (same in all environments—uses Aspire client integration packages)
// Aspire.Npgsql, Aspire.StackExchange.Redis read from IConfiguration
builder.AddNpgsqlDbContext<AppDbContext>("db");      // Reads ConnectionStrings:db
builder.AddRedisClient("cache");                      // Reads ConnectionStrings:cache
```

**Environment variable names must match across environments:**

| Environment | How Variables Are Set | Variable Name |
|-------------|----------------------|---------------|
| Aspire (dev) | AppHost `.WithReference()` | `ConnectionStrings__db` |
| Kubernetes | ConfigMap/Secret | `ConnectionStrings__db` |
| Docker Compose | `.env` file | `ConnectionStrings__db` |
| Azure App Service | Configuration | `ConnectionStrings__db` |

**Code reads the same way everywhere**: `IConfiguration.GetConnectionString("db")`

### Anti-Patterns

**NEVER do this:**

```csharp
// WRONG—Custom env var names that don't match Aspire
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")  // Custom name
    ?? builder.Configuration.GetConnectionString("db");  // Aspire name
```

```csharp
// WRONG—Environment-specific code paths
if (builder.Environment.IsDevelopment()) {
    builder.AddRedisClient("cache");  // Aspire convention
} else {
    // Production uses different API/config name
    builder.Services.AddStackExchangeRedisCache(options => {
        options.Configuration = Environment.GetEnvironmentVariable("REDIS_URL");
    });
}
```

**Why they're wrong:**
- Creates divergent behavior between development and production
- Defeats Aspire's goal of environment consistency
- Makes production issues harder to reproduce locally
- Adds unnecessary conditionals

### Best Practice

**Use Aspire's conventions everywhere:**

Production Kubernetes ConfigMap:
```yaml
data:
  ConnectionStrings__db: "Host=prod-postgres;Database=myapp"
  ConnectionStrings__cache: "prod-redis:6379"
```

Production Docker Compose `.env`:
```bash
ConnectionStrings__db=Host=postgres;Database=myapp
ConnectionStrings__cache=redis:6379
```

Azure App Service Configuration:
```
ConnectionStrings__db = Host=myserver.postgres.database.azure.com;Database=myapp
ConnectionStrings__cache = mycache.redis.cache.windows.net:6380,ssl=True
```

**Code stays identical across all environments**—no if statements, no custom variable names.

### Verification

Test that configuration works identically:
1. Run via Aspire AppHost: `dotnet run --project MyApp.AppHost`
2. Run with production-style variables:
   ```bash
   export ConnectionStrings__db="Host=localhost;Database=myapp"
   export ConnectionStrings__cache="localhost:6379"
   dotnet run --project MyApp.Service
   ```
3. Behavior and connection logic must be identical

## References

- [Aspire.dev](https://aspire.dev/)
- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Aspire Service Defaults](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/service-defaults)
- [Aspire Testing](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/testing)
- [OpenTelemetry in .NET](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-with-otel)
