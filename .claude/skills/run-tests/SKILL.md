---
name: run-tests
description: Use this skill PROACTIVELY whenever running, debugging, investigating failing tests, or during TDD cycles.ALWAYS use this skill for running tests - never use raw Bash commands for running tests.
allowed-tools: "Bash,Grep,Read,AskUserQuestion,Glob"
context: fork
---

# Run Tests

Helper for running tests across the ConvoClarity monorepo - supporting both .NET TUnit tests and Playwright E2E tests.

## Expected output

A markdown table with test case names and their pass/fail status.

## Tests to run:

```
$ARGUMENTS
```

## Execution Rules

**CRITICAL: Execute exactly ONE test command.** Do not try multiple approaches in parallel.

### Single Command Rule
- Pick the appropriate command based on context (built/not built, arguments provided)
- Run exactly that one command
- If it fails, report the error - do NOT retry with different flags automatically
- Parallel test executions cause conflicts (especially with Supabase CLI tests)

### Multiple Filters? Combine with OR
When user requests running multiple test classes/filters, combine them into ONE command using logical operators:

**❌ WRONG - Multiple commands (parallel execution):**
```bash
dotnet test --filter "FullyQualifiedName~TestClass1"
dotnet test --filter "FullyQualifiedName~TestClass2"
```

**✅ CORRECT - Single command with OR operator (`|`):**
```bash
dotnet test --filter "FullyQualifiedName~TestClass1|FullyQualifiedName~TestClass2"
```

**Examples:**
- Multiple classes: `--filter "Class1|Class2|Class3"`
- Mixed patterns: `--filter "FullyQualifiedName~Supabase*|Category=Integration"`
- .NET TUnit category: `--filter "Category=Unit|Category=Integration"`

---

## Quick Reference

| Project | Framework | Test Location | Quick Command |
|---------|-----------|---------------|---------------|
| ConvoClarity | TUnit | `ConvoClarity/ConvoClarityWebTests/` | `/run-tests dotnet` or `/run-tests tunit` |
| Landing | Playwright | `Landing/tests/` | `/run-tests playwright` or `/run-tests landing` |
| Both | - | - | `/run-tests all` |

## Usage

```
/run-tests                           # Detect and run tests for current directory
/run-tests dotnet                    # Run .NET TUnit tests
/run-tests playwright                # Run Playwright tests
/run-tests all                       # Run all test suites

# .NET TUnit specific
/run-tests dotnet unit               # Run unit tests only
/run-tests dotnet integration        # Run integration tests only
/run-tests dotnet ClassName          # Run tests in specific class

# Playwright specific
/run-tests playwright smoke          # Run smoke tests
/run-tests playwright components     # Run component tests
/run-tests playwright demo           # Run demo-related tests
```

## Test Detection

The skill automatically detects which test suite to run based on the current working directory:

- Working in `ConvoClarity/` → Runs TUnit tests
- Working in `Landing/` → Runs Playwright tests
- Working in root → Asks which suite to run

## When to Use

| Scenario | Command |
|----------|---------|
| **Quick validation after code changes** | `/run-tests` (auto-detect) |
| **TDD red-green-refactor cycle** | `/run-tests dotnet unit` (fast feedback) |
| **Testing Landing page UI** | `/run-tests playwright` |
| **Debugging specific test class** | `/run-tests dotnet ClassName` |
| **Pre-commit verification** | `/run-tests all` |

---

## Progressive Disclosure

**This file contains core essentials only.** For detailed documentation on specific frameworks, see:

- **.NET TUnit**: `references/tunit.md` - Category filtering, test patterns, common issues
- **Playwright**: `references/playwright.md` - Test organization, debugging, UI mode
- **Examples**: `examples/` - Common test scenarios and commands
