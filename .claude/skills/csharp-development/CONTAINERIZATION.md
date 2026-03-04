# Containerization Patterns

Container publishing and deployment patterns for .NET applications.

## Container Publishing (csproj-based)

Publish containers directly from project files without Dockerfiles.

### Enable in csproj

```xml
<PropertyGroup>
  <PublishProfile>DefaultContainer</PublishProfile>
  <ContainerImageName>myapp</ContainerImageName>
  <ContainerImageTag>latest</ContainerImageTag>
</PropertyGroup>
```

### Common Properties

| Property             | Purpose                                      |
| -------------------- | -------------------------------------------- |
| `ContainerImageName` | Image name                                   |
| `ContainerImageTag`  | Image tag                                    |
| `ContainerRegistry`  | Registry URL (e.g., `myregistry.azurecr.io`) |
| `ContainerBaseImage` | Custom base image                            |
| `ContainerPort`      | Exposed port                                 |

### Publishing

- **Local Docker:** `dotnet publish -t:PublishContainer`
- **To registry:** Add `-p:ContainerRegistry=myregistry.azurecr.io`
- **Custom tag:** Add `-p:ContainerImageTag=v1.2.3`

## When to Use Dockerfiles Instead

Use traditional Dockerfiles when:

- Need multi-stage builds with custom build steps
- Require OS-level dependencies (apt-get, apk)
- Custom base image not in Microsoft registry
- Complex file copying/manipulation
- Integration with existing Docker Compose workflows

**For most .NET apps, csproj-based publishing is simpler.**

## Configuration Management

| Environment | Approach                                  |
| ----------- | ----------------------------------------- |
| Development | .NET Aspire AppHost manages configuration |
| Production  | Environment variables                     |

Use `IConfiguration` consistently across environments.

**Critical**: When using Aspire with containers, ensure environment-agnostic configuration. See [ASPIRE.md](ASPIRE.md#environment-agnostic-configuration) for details.

## References

- [.NET Container Publishing](https://learn.microsoft.com/en-us/dotnet/core/docker/publish-as-container)
- [Container Configuration](https://learn.microsoft.com/en-us/dotnet/core/docker/container-images)
- [Docker with .NET](https://learn.microsoft.com/en-us/dotnet/core/docker/introduction)
