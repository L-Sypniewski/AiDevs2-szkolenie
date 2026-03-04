# Code Review Team Example

## Scenario Context

User asks: "Can you review this authentication module for security issues, performance problems, and test coverage?"

The authentication module is a critical component that needs thorough review from multiple perspectives. A single review might miss issues that only become apparent through specific lenses.

## Clarifying Questions Asked

**Analysis:** "This code review would benefit from multiple specialized perspectives:
- Security vulnerabilities (OWASP Top 10, auth-specific attacks)
- Performance under load (caching, query optimization)
- Test coverage completeness (unit, integration, edge cases)
- Code quality and maintainability

**Proposals:**
- Option A: 4 teammates - one for each perspective (security, performance, tests, quality)
- Option B: 2 teammates - one for security+performance, one for tests+quality
- Option C: Single comprehensive reviewer covering all aspects

Which approach would work best for this review?"

## Proposed Team Structure

**Confirmed:** Option A with 4 specialized reviewers

### Team Configuration

**Team name:** `auth-review-team`

**Team members:**
- **security-reviewer:** OWASP Top 10 focus, auth-specific vulnerabilities
- **performance-reviewer:** Database queries, caching, response times
- **test-coverage-reviewer:** Unit tests, integration tests, edge cases
- **code-quality-reviewer:** SOLID principles, maintainability, readability

## Resulting Task Breakdown

### Task 1: Security Review
**Owner:** security-reviewer

1. Review authentication flow for common vulnerabilities
2. Check for proper password hashing (not MD5/SHA1)
3. Verify session management (token expiration, refresh logic)
4. Analyze input validation and sanitization
5. Check for authorization bypass possibilities
6. Review error messages for information leakage
7. Verify HTTPS enforcement and secure headers

**Deliverable:** Security findings report with severity ratings and remediation steps

### Task 2: Performance Review
**Owner:** performance-reviewer

1. Analyze database query patterns (N+1 queries, missing indexes)
2. Check for unnecessary database round-trips
3. Review caching strategy (what's cached, cache invalidation)
4. Identify synchronous operations that could be async
5. Check for memory leaks or resource disposal issues
6. Analyze response time hotspots
7. Review pagination and data fetching patterns

**Deliverable:** Performance findings report with optimization recommendations

### Task 3: Test Coverage Review
**Owner:** test-coverage-reviewer

1. Analyze existing test coverage percentages
2. Identify untested critical paths (login, logout, password reset)
3. Check for missing edge case tests (invalid tokens, expired sessions)
4. Review integration test coverage for auth flows
5. Verify negative test cases (wrong password, locked account)
6. Check for security-specific test scenarios
7. Assess test quality and effectiveness

**Deliverable:** Test coverage report with gaps identified and test recommendations

### Task 4: Code Quality Review
**Owner:** code-quality-reviewer

1. Review adherence to SOLID principles
2. Check code complexity and cyclomatic complexity
3. Analyze separation of concerns
4. Review naming conventions and code clarity
5. Check for code duplication
6. Assess error handling completeness
7. Review logging and debugging support
8. Verify documentation and comments where needed

**Deliverable:** Code quality report with refactoring suggestions

### Task 5: Synthesis and Prioritization
**Owner:** Team lead

1. Compile all review reports
2. Identify overlapping issues mentioned by multiple reviewers
3. Prioritize findings by severity and impact
4. Create unified action plan
5. Group related issues for efficient fixing
6. Generate summary for user

**Deliverable:** Comprehensive review summary with prioritized action items

## Execution Flow

1. All four reviewers work independently on same codebase
2. Each produces a focused report for their specialty
3. Team lead synthesizes findings, identifies overlaps, prioritizes
4. User receives comprehensive review covering all critical dimensions

## Result

User receives thorough coverage from four specialized perspectives. Issues that might be missed in a general review are caught by specialists. The synthesis provides prioritized, actionable recommendations.
