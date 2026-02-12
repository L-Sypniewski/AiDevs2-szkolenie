# Blazor Patterns

Patterns for Blazor applications - Server, WebAssembly, and Static SSR.

## Component Design

- **Keep components simple and reusable** (favor dumb/presentational components over smart components).

- **One responsibility** - Component does one thing well
- **Required parameters** - Use `[Parameter, EditorRequired]` for mandatory props
- **Events over state mutation** - Use `EventCallback<T>` for parent communication
- **Avoid business logic** - Delegate to services

## Lifecycle Usage

| Method                 | When to use                      |
| ---------------------- | -------------------------------- |
| `OnInitializedAsync`   | Load initial data                |
| `OnParametersSetAsync` | React to parameter changes       |
| `OnAfterRenderAsync`   | JS interop (check `firstRender`) |
| `Dispose`              | Unsubscribe from events, cleanup |

## State Management

### Cascading Parameters

- Use for app-wide state (user, theme, culture)
- Prefer explicit parameters for direct parent-child
- Avoid excessive cascading - consider state containers

### State Containers

- **Singleton** for app-wide state (WebAssembly)
- **Scoped** for per-circuit state (Server)
- Raise `StateChanged` event for reactivity

## Rendering Modes

Choose based on interactivity needs:

| Mode                    | Use when                                                     | Apply with                           |
| ----------------------- | ------------------------------------------------------------ | ------------------------------------ |
| Static SSR (default)    | Content-heavy pages, SEO needed, no interactivity            | (default)                            |
| Interactive Server      | Low-latency apps, server proximity, persistent connection OK | `@rendermode InteractiveServer`      |
| Interactive WebAssembly | Offline scenarios, client-side compute, larger download OK   | `@rendermode InteractiveWebAssembly` |
| Interactive Auto        | Best of both - fast first load, then client-side             | `@rendermode InteractiveAuto`        |

## Forms & Validation

- Use `<EditForm>` with model binding
- `<DataAnnotationsValidator />` for attribute-based validation
- `OnValidSubmit` when valid, `OnInvalidSubmit` when invalid
- Use built-in input components (`InputText`, `InputNumber`, etc.)

## Localization

- Create resource files: `Resources/App.resx`, `Resources/App.{culture}.resx`
- Inject: `@inject IStringLocalizer<App> Localizer`
- Use: `@Localizer["Key"]`

## Performance

- `<Virtualize>` for large lists
- `@attribute [StreamRendering]` for faster perceived load
- Override `ShouldRender()` or use `@key` to avoid re-renders
- Lazy loading for WebAssembly assemblies

## Security

- **Client Secrets**: NEVER store API keys or connection strings in Blazor WASM code. It is fully visible to the user.
- **XSS Prevention**: Avoid `MarkupString` unless absolutely necessary and sanitized. Prefer standard binding.
- **Native AOT**: Ensure components are AOT-compatible for .NET 10 deployments.

## Best Practices

**DO:**

- Keep components small and focused
- Use services for business logic
- Prefer EventCallback over direct state mutation
- Use required parameters for mandatory inputs
- Implement IDisposable for cleanup
- Use appropriate render mode for use case
- Minimize JavaScript interop

**DON'T:**

- Put business logic in components
- Share state excessively via cascading parameters
- Use Server mode for high-scale public internet apps
- Ignore component disposal
- Overuse StateHasChanged()

## References

- [Blazor Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [Blazor Components](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/)
- [Blazor State Management](https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management)
- [Blazor Render Modes](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes)
