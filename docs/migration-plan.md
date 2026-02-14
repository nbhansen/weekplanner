# Weekplanner Migration to giraf-core - Progress Tracker

**Last Updated:** 2026-02-14
**Status:** Phases 1-5 complete
**Test Results:** ✅ Core 115 passing, Backend 21 passing, Frontend 131 passing (100%)
**Git Branches:**
- Core: `main` @ `49e97be` feat(core): add endpoints for weekplanner frontend migration
- Backend: `main` @ current (Phase 5 complete)
- Frontend: `master` @ `ed35bdc` feat(frontend): migrate shared entity calls to giraf-core

---

## Completed Work

### ✅ Chunk 1: Update Test Expectations (2026-02-13)
**Commit:** `3bdf355` fix(tests): update auth test expectations for new JWT handler

Updated 12 integration test expectations from `NotFound`/`BadRequest` to `Forbidden` when accessing resources without org membership.

**Tests fixed:**
- `OrganizationEndpointTests`: 4 tests
- `UsersEndpointTests`: 5 tests
- `GradeEndpointTests`: 2 tests
- `CitizenEndpointTests`: 1 test

**Rationale:** The new `JwtOrgRoleHandler` denies access before endpoints can check resource existence, preventing enumeration attacks. This is correct security behavior - unauthorized users shouldn't learn whether resources exist.

**Verification:** All 103 tests pass.

---

### ✅ Chunk 2: Delete Deprecated Auth Handlers (2026-02-13)
**Commit:** `6101409` refactor(auth): remove deprecated DB-querying auth handlers

Deleted old authorization handler implementations that queried the database 3 times per request.

**Removed handlers:**
- `OrgMemberAuthorizationHandler` (from `OrgMemberRequirement.cs`)
- `OrgAdminAuthorizationHandler` (from `OrgAdminRequirement.cs`)
- `OrgOwnerAuthorizationHandler` (from `OrgOwnerRequirement.cs`)
- `OwnDataAuthorizationHandler` (from `OwnDataRequirement.cs`)

**Kept:** Requirement classes (still needed for policy definitions)

**Verification:** Build succeeds with 0 errors, 18 warnings (same as before). All 103 tests pass.

---

### ✅ Chunk 3: Commit Working JWT Auth (Phases 2 & 3) (2026-02-13)
**Commit:** `889f19b` feat(auth): migrate to Core JWT validation with org_roles claim

Implemented JWT claim-based authorization that reads `org_roles` from Core-issued tokens instead of querying the database.

**Changes:**
- `JwtSettings.cs`: Added `org_roles` claim configuration
- `ServiceExtensions.cs`: Registered `JwtOrgRoleHandler`, updated JWT validation
- `LoginEndpoints.cs`: Generate test `org_roles` claim in response
- `appsettings.json`: Aligned JWT settings with Core (issuer, audience)
- `HttpClientExtensions.cs`: Test helper generates `org_roles` claim in JWTs
- `JwtOrgRoleHandler.cs`: New claim-based auth handler (replaces 4 old handlers)

**Benefits:**
- **Security:** Returns 403 Forbidden for non-members accessing non-existent resources (prevents enumeration)
- **Performance:** Zero database queries during authorization (was 3 queries per request)

**Verification:** All 103 tests pass.

---

## Next Steps

### ✅ Phase 1: Frontend — Add Core API Client + Migrate Auth (2026-02-13)
**Status:** COMPLETE
**Commit:** `426e294` feat(frontend): migrate authentication to giraf-core
**Goal:** Frontend authenticates with Core instead of weekplanner backend.

**Verification:** User can register and log in. JWT comes from Core. Frontend still calls weekplanner backend for other operations.

---

### Phase 2: Backend — Validate Core JWTs
**Status:** ✅ COMPLETE (in Chunk 3)

---

### Phase 3: Backend — Replace Auth Handlers with JWT Claim-Based Auth
**Status:** ✅ COMPLETE (in Chunks 2 & 3)

---

### ✅ Phase 4: Frontend — Migrate Shared Entity Calls to Core (2026-02-14)
**Status:** COMPLETE
**Commits:** `49e97be` (Core) and `ed35bdc` (Frontend)
**Goal:** Frontend calls Core directly for shared domain data.

**Verification:** 115 Core tests + 131 Frontend tests all passing.

---

### ✅ Phase 5: Backend — Slim Down to Activities Only (2026-02-14)
**Status:** COMPLETE

**What was done:**
1. Made Activity FK columns explicit (CitizenId, GradeId, PictogramId)
2. Created GirafCoreClient for validating entities exist in Core API
3. Rewrote ActivityEndpoints to use explicit FKs + CoreClient validation
4. Deleted all non-Activity entities, endpoints, mappings, and tests
5. Removed ASP.NET Identity, simplified DbContext to plain `DbContext`
6. Simplified test infrastructure (no UserManager, no Identity seeding)

**DTO breaking change:** `ActivityDTO.Pictogram` (object) → `ActivityDTO.PictogramId` (int?). Frontend resolves pictogram display data from its Core-fetched cache.

**Backend structure (final):**
```
weekplannerbackend/
  GirafAPI/
    Authorization/          # JwtOrgRoleHandler + requirement classes
    Clients/                # ICoreClient + GirafCoreClient
    Configuration/          # JwtSettings
    Data/                   # GirafDbContext (Activities only)
    Endpoints/              # ActivityEndpoints.cs (the ONLY endpoints)
    Entities/Activities/    # Activity + DTOs (the ONLY entity)
    Extensions/             # ServiceExtensions, ApplicationBuilderExtensions
    Mapping/                # ActivityMapping.cs
    Program.cs              # Thin startup
```

**Test results:** 21 Activity-focused integration tests passing.

---

### Phase 6: Frontend — Update for ActivityDTO change
**Status:** TODO

The frontend needs to update activity display code to resolve pictogram data from its Core-fetched cache instead of the `Pictogram` object that was previously embedded in the activity response.

---

## End State

```
weekplannerbackend/
  GirafAPI/
    Entities/Activities/     # Activity + DTOs (the ONLY entity)
    Endpoints/ActivityEndpoints.cs  # The ONLY endpoints
    Clients/GirafCoreClient.cs      # Calls Core to validate citizen/grade exists
    Authorization/JwtOrgRoleHandler.cs  # Reads org_roles from Core JWT
    Data/GirafDbContext.cs          # Just DbSet<Activity>
    Program.cs                      # Thin startup
```

The weekplanner backend is now ~10 files. Everything else is Core's responsibility.

---

## Verification (End-to-End)

**After Full Migration:**
1. Start Core: `cd giraf-core && docker compose up`
2. Start weekplanner backend: `cd weekplannerbackend && dotnet run --project GirafAPI`
3. Start frontend: `cd weekplan-frontend && npx expo start`
4. Register user → calls Core
5. Log in → calls Core, gets JWT with `org_roles`
6. Create organization → calls Core
7. Create citizen in org → calls Core
8. Create activity for citizen → calls weekplanner backend (validates citizen exists via Core)
9. View week schedule → calls weekplanner backend for activities, Core for citizen/pictogram names
