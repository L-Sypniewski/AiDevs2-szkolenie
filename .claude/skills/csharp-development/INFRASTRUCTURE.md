# Infrastructure Patterns

Modern infrastructure patterns for .NET applications.

## .NET Aspire

See [ASPIRE.md](ASPIRE.md) for comprehensive Aspire guidance including:

- Project scaffolding and setup
- Telemetry dashboard and observability
- Integration testing with `DistributedApplicationTestingBuilder`
- **Environment-agnostic configuration** (critical for production parity)

## Containerization & Deployment

See [CONTAINERIZATION.md](CONTAINERIZATION.md) for Docker and container deployment patterns including:

- csproj-based container publishing
- When to use Dockerfiles
- Configuration management across environments

## OpenTelemetry

Use OpenTelemetry for observability. Configure via ServiceDefaults or directly in each service.

## References

- [OpenTelemetry](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-with-otel)
