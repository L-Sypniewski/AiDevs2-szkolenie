# .NET TUnit Testing Reference

Complete reference for running TUnit tests in the ConvoClarity project.

## Test Organization

Tests are organized by category using `[Category("Unit")]`, `[Category("Integration")]`, and `[Category("E2E")]` attributes.

| Category | Filter Pattern | Description |
|----------|---------------|-------------|
| Unit | `/*/*/*/*[Category=Unit]` | Fast, isolated unit tests |
| Integration | `/*/*/*/*[Category=Integration]` | Tests with real dependencies (requires Docker) |
| E2E | `/*/*/*/*[Category=E2E]` | End-to-end tests with Playwright |

## Commands Reference

### Run all tests

```bash
cd ConvoClarity
dotnet test                          # Fastest way to run all tests
dotnet test --output Detailed       # With detailed output
```

### Filter by category

```bash
# IMPORTANT: Use 4 wildcards before the Category filter
# Pattern: /*/*/*/*[Category={CategoryName}]
dotnet run --project ConvoClarityWebTests/ConvoClarityWebTests.csproj -- \
  --treenode-filter "/*/*/*/*[Category=Unit]"

dotnet run --project ConvoClarityWebTests/ConvoClarityWebTests.csproj -- \
  --treenode-filter "/*/*/*/*[Category=Integration]"
```

### Run tests from a single file (test class)

```bash
# Pattern: /*/*/{ClassName}/*
dotnet run --project ConvoClarityWebTests/ConvoClarityWebTests.csproj -- \
  --treenode-filter "/*/*/*FilterMetadataServiceTests/*"
```

### Run specific test methods

```bash
# Pattern: /*/*/*/{ExactTestName}
dotnet run --project ConvoClarityWebTests/ConvoClarityWebTests.csproj -- \
  --treenode-filter "/*/*/*/*MultipleOffers_ShouldAllScrapeSuccessfully"
```

### Wildcard matching

```bash
# Match tests containing a specific string
dotnet run --project ConvoClarityWebTests/ConvoClarityWebTests.csproj -- \
  --treenode-filter "/*/*/*/*GetFilterMetadata*"
```

## Important Notes

1. **Use `dotnet run` for treenode-filter** - TUnit requires this syntax for filtering
2. **Project path** - Always specify `--project ConvoClarityWebTests/ConvoClarityWebTests.csproj`
3. **TUnit version** - Check `Directory.Packages.props` for current version
4. **`--list-tests` limitation** - May show all tests even with filter applied

## Common Issues

### Filter not working

- Ensure you're using `dotnet run` not `dotnet test`
- For Category filters, use exactly 4 wildcards: `/*/*/*/*[Category=Unit]`
- For single class filters, use 3 wildcards + class name: `/*/*/*ClassName/*`
- For specific test filters, use 4 wildcards + test name: `/*/*/*/*TestName`
- Try running without filter first to verify tests exist

### Tests not found

- Ensure the project is built: `dotnet build`
- Check the test name matches exactly (case-sensitive)
- Verify the Category attribute is applied for category-based filtering

### Docker required for integration tests

Integration tests use Testcontainers and require Docker to be running:

```bash
# Check Docker status
docker ps

# Start Docker if needed (Linux)
sudo systemctl start docker
```

## External References

- [TUnit Test Filters](https://tunit.dev/docs/execution/test-filters)
- [TUnit Command-Line Flags](https://tunit.dev/docs/reference/command-line-flags)
- [Project Testing Guidelines](../../../../docs/testing.md)
