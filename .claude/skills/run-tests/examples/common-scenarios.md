# Common Test Scenarios

Examples of typical test-running workflows in ConvoClarity.

## Quick Development Workflow

```bash
# TDD cycle - run fast unit tests
/run-tests dotnet unit

# Verify after code changes (auto-detects project)
/run-tests

# Full test run before commit
/run-tests all
```

## Debugging Failing Tests

### .NET TUnit

```bash
# Run specific failing test
/run-tests dotnet MultipleOffers_ShouldAllScrapeSuccessfully

# Run tests from a specific class
/run-tests dotnet FilterMetadataServiceTests

# Run with detailed output
cd ConvoClarity && dotnet test --output Detailed
```

### Playwright

```bash
# Run with UI mode to inspect failures
/run-tests playwright --ui

# Run specific test file
cd Landing && npx playwright test homepage.spec.ts

# Run with visible browser for debugging
cd Landing && npx playwright test --headed
```

## Category-Based Testing

### .NET TUnit

```bash
# Unit tests only (fast, no Docker needed)
/run-tests dotnet unit

# Integration tests (requires Docker)
/run-tests dotnet integration

# E2E tests (slowest, full system)
/run-tests dotnet e2e
```

### Playwright

```bash
# Smoke tests - critical paths only
cd Landing && npx playwright test smoke/

# Component tests
cd Landing && npx playwright test components/

# Page tests
cd Landing && npx playwright test pages/
```

## Test Discovery

### List all available tests

```bash
# .NET TUnit - list tests
cd ConvoClarity && dotnet run --project ConvoClarityWebTests/ConvoClarityWebTests.csproj -- --list-tests

# Playwright - list tests
cd Landing && npx playwright test --list
```

### Find test patterns

```bash
# Find tests by name pattern (.NET)
cd ConvoClarity && dotnet run --project ConvoClarityWebTests/ConvoClarityWebTests.csproj -- \
  --treenode-filter "/*/*/*/*GetFilterMetadata*"

# Find tests by name pattern (Playwright)
cd Landing && npx playwright test --grep "homepage"
```

## CI/CD Integration

```bash
# .NET - suitable for CI pipelines
cd ConvoClarity && dotnet test --logger "console;verbosity=detailed"

# Playwright - suitable for CI
cd Landing && npm run test:e2e

# Both - run all tests for monorepo
/run-tests all
```

## Docker Check for Integration Tests

```bash
# Verify Docker is running before integration tests
docker ps

# If Docker is not running:
# Linux: sudo systemctl start docker
# macOS: open Docker Desktop
# Windows: start Docker Desktop
```
