# Implementation Team Example

## Scenario Context

User asks: "Can this user profile feature be implemented by a team? The plan includes database changes, backend API, frontend UI, and tests."

The user profile feature plan includes:
- Database schema changes (Users table extension)
- Backend API endpoints (GET, PUT, DELETE)
- Frontend profile page component
- Unit and integration tests

## Clarifying Questions Asked

**Analysis:** "This feature has clear component boundaries:
- Database layer (migrations, schema)
- Backend API (controllers, services)
- Frontend UI (Blazor components, validation)
- Tests (unit, integration)

**Proposals:**
- Option A: 4 teammates - database, backend, frontend, tests (parallel work)
- Option B: 2 teammates - backend (DB+API), frontend (UI+tests)
- Option C: Single agent implementing sequentially

Which approach would you prefer?"

**Follow-up:** "I recommend Option A because:
- Database changes can be prepared independently
- Backend can be implemented against the planned schema
- Frontend can use mock data until API is ready
- Tests can be written in parallel with implementation

Shall I proceed with this 4-teammate structure?"

## Proposed Team Structure

**Confirmed:** Option A with 4 teammates

### Team Configuration

**Team name:** `user-profile-feature-team`

**Team members:**
- **database-dev:** Schema design, migrations, seed data
- **backend-dev:** API endpoints, services, DTOs
- **frontend-dev:** Blazor components, validation, state management
- **test-dev:** Unit tests, integration tests, test fixtures

## Resulting Task Breakdown

### Task 1: Database Implementation
**Owner:** database-dev
**Blocked by:** None

1. Design extended Users table schema
2. Create Supabase migration file
3. Add new columns: bio, avatar_url, preferences (jsonb)
4. Create indexes for performance
5. Write seed data for testing
6. Test migration locally with Supabase CLI
7. Document schema changes

**Deliverable:** Working migration with schema documentation

### Task 2: Backend API Implementation
**Owner:** backend-dev
**Blocked by:** Task 1 (schema finalization)

1. Create UserProfileDto and UpdateUserProfileDto
2. Implement ProfileController with endpoints:
   - GET /api/users/{id}/profile
   - PUT /api/users/{id}/profile
   - DELETE /api/users/{id}/profile
3. Create UserProfileService for business logic
4. Add validation attributes
5. Implement error handling (not found, validation errors)
6. Add authentication requirements
7. Create integration tests skeleton

**Deliverable:** Working API endpoints with basic validation

### Task 3: Frontend UI Implementation
**Owner:** frontend-dev
**Blocked by:** None (can use mock data initially)

1. Create UserProfile.razor component
2. Implement form with fields: bio, avatar URL, preferences
3. Add client-side validation
4. Create profile page routing
5. Implement file upload for avatar
6. Add loading states and error handling
7. Create profile preview card
8. Integrate with backend API (can mock initially)

**Deliverable:** Functional profile UI with validation

### Task 4: Test Implementation
**Owner:** test-dev
**Blocked by:** Task 2 (API endpoints available)

1. Write unit tests for UserProfileService
2. Create integration tests for API endpoints
3. Add Testcontainers fixtures for database tests
4. Write component tests for UserProfile.razor
5. Create tests for validation scenarios
6. Add tests for authentication/authorization
7. Implement test data builders

**Deliverable:** Comprehensive test coverage

### Task 5: Integration and Finalization
**Owner:** Team lead
**Blocked by:** Tasks 1, 2, 3, 4

1. Connect frontend UI to real backend API
2. Replace mock data with API calls
3. Run full integration tests
4. Fix any discovered issues
5. Verify end-to-end flow works
6. Update documentation

**Deliverable:** Fully integrated feature ready for review

## Execution Flow

1. **Phase 1 (Parallel):** Database, Frontend, Test planning start immediately
2. **Phase 2 (After DB):** Backend implements against finalized schema
3. **Phase 3 (After Backend):** Tests complete implementation tests
4. **Phase 4 (Final):** Integration connects all components

## Result

Feature implemented efficiently with clear ownership. Frontend progressed independently using mock data, tests were written alongside implementation, and database work didn't block other teammates from starting their tasks.
