# Playwright Testing Reference

Complete reference for running Playwright tests in the Landing page project.

## Test Organization

Tests are organized by directory structure under `Landing/tests/`:

| Directory | Purpose | Examples |
|-----------|---------|----------|
| `smoke/` | Core smoke tests (critical paths) | `homepage.spec.ts`, `global-css.spec.ts` |
| `components/` | Component-level tests | `header.spec.ts`, `footer.spec.ts` |
| `pages/` | Full page tests | `contact.spec.ts`, `demo.spec.ts` |
| `demo/` | Demo-specific component tests | `transcript-viewer.spec.ts` |
| `data/` | Data/fixture tests | `demo-data.spec.ts` |
| `accessibility/` | Dedicated accessibility suite | `accessibility-suite.spec.ts` |

### Test Suites

The project has two test suites for different scenarios:

| Suite | Tests | Duration | When to Run |
|-------|-------|----------|-------------|
| **Smoke** | ~120 (@smoke tagged) | ~1-2 min | PRs, fast feedback |
| **Full** | 482 (all tests) | ~3-4 min | Merge to master, full validation |

## Commands Reference

### Run all tests

```bash
cd Landing
npm run test:e2e                  # Run all tests (full suite)
npm run test:e2e:full             # Alias for full suite
npm run test:e2e:smoke            # Run smoke tests only (@smoke tagged)
npm run test:e2e:ui              # Run with UI mode
npm run test:e2e:debug           # Run in debug mode
npm run test:e2e:report          # Show HTML report
```

### Filter by test file

```bash
# Run a specific test file
npx playwright test homepage.spec.ts

# Run multiple files matching pattern
npx playwright test smoke/
npx playwright test components/
```

### Filter by test name

```bash
# Run tests matching name pattern
npx playwright test --grep "homepage"
npx playwright test --grep "i18n"

# Run tests with @smoke tag (equivalent to npm run test:e2e:smoke)
npx playwright test --grep "@smoke"
```

### Run tests in specific project

```bash
# Currently only chromium is configured
npx playwright test --project=chromium
```

## Debugging

### UI Mode

```bash
npm run test:e2e:ui
# or
npx playwright test --ui
```

UI Mode provides:
- Visual test explorer
- Time-travel debugging
- Watch mode for rerunning tests
- Inspect selectors

### Debug Mode

```bash
npm run test:e2e:debug
# or
npx playwright test --debug
```

Debug mode:
- Opens browser with developer tools
- Pauses execution on failure
- Allows stepping through tests

### Heading Mode (run with visible browser)

```bash
npx playwright test --headed
```

## Configuration

### Test Configurations

The project has two Playwright configurations:

| Config | Purpose | File |
|-------|---------|------|
| **Base** | Full test suite with 482 tests | `playwright.config.ts` |
| **Smoke** | Filters @smoke tagged tests only | `playwright-smoke.config.ts` |

### Base URL

Tests use `BASE_URL` environment variable (default: `http://localhost:4321`)

```bash
BASE_URL=http://localhost:8080 npm run test:e2e
```

### Parallel Execution

- **Local**: Tests run with available workers (auto-detected)
- **CI**: 4 parallel workers for speed
- **Retries**: 1 retry on CI, 0 locally

### Output Directory

Test results and screenshots are saved to `Landing/test-results/`

### CI Behavior

The GitHub Actions workflow (`.github/workflows/landing-e2e.yml`) automatically:

- **Pull Requests**: Runs smoke tests only (~1-2 min)
- **Push to master**: Runs full suite (~3-4 min)

## Important Notes

1. **Dev server auto-start** - Playwright config starts `npm run preview` automatically
2. **Screenshots on failure** - Configured to capture screenshots only on failure
3. **Parallel execution** - Tests run in parallel by default (4 workers on CI)
4. **Retries** - 1 retry on CI for flaky test handling
5. **@smoke tag** - Use `{ tag: ['@smoke'] }` to mark tests for smoke suite
6. **Test count** - 482 tests total (optimized from ~660)

## Common Issues

### Port already in use

```bash
# Kill process using port 4321
lsof -ti:4321 | xargs kill -9

# Or use a different port
BASE_URL=http://localhost:4322 npx playwright test
```

### Tests timeout

- Increase timeout in `playwright.config.ts` or per test:
  ```ts
  test.setTimeout(60000); // 60 seconds
  ```

### Flaky tests

- Add explicit waits using `await expect(locator).toBeVisible()`
- Use `waitFor`/`waitForSelector` for dynamic content
- Check for race conditions with async operations

## External References

- [Playwright Documentation](https://playwright.dev/docs/intro)
- [Playwright Config](https://playwright.dev/docs/test-configuration)
- [Project Testing Guidelines](../../../../docs/testing.md)
