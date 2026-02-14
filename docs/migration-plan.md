# Weekplanner Migration to giraf-core - Progress Tracker

**Last Updated:** 2026-02-14
**Status:** Phases 1-4 complete, ready for Phase 5
**Test Results:** ✅ Core 115 passing, Backend 103 passing, Frontend 131 passing (100%)
**Git Branches:**
- Core: `main` @ `49e97be` feat(core): add endpoints for weekplanner frontend migration
- Backend: `main` @ `801c27b` docs(backend): update README for Core JWT validation
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

**Files to create:**
- `weekplan-frontend/apis/coreAxiosConfig.ts` — axios instance for Core API

**Files to modify:**
- `weekplan-frontend/.env` / `.env.example` — add `EXPO_PUBLIC_CORE_API_URL`
- `weekplan-frontend/utils/globals.ts` — add `CORE_BASE_URL`
- `weekplan-frontend/apis/authorizationAPI.ts` — login calls Core `POST /api/v1/token/pair`
- `weekplan-frontend/providers/AuthenticationProvider.tsx` — store `org_roles`, set bearer on both axios instances
- `weekplan-frontend/utils/jwtDecode.ts` — add `getOrgRolesFromToken()`
- `weekplan-frontend/apis/registerAPI.ts` — register calls Core `POST /api/v1/auth/register`

**Core login response format:**
```json
{ "access": "eyJ...", "refresh": "eyJ...", "org_roles": {"1": "owner", "5": "member"} }
```

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

**Core extensions made (needed for frontend compatibility):**
- `MemberOut` schema — added `first_name`, `last_name`, `email` fields
- `PATCH /api/v1/organizations/{id}` — update organization name
- `DELETE /api/v1/organizations/{id}` — delete organization (owner only)
- `POST/DELETE /api/v1/organizations/{id}/grades/{gid}/citizens` — add/remove citizens from grades
- `POST /api/v1/pictograms` — upload pictogram with image file

**Frontend files created:**
- `apis/coreApiMappers.ts` — snake_case ↔ camelCase mapping layer for all Core responses

**Frontend files modified (12):**
- `apis/userAPI.ts` — profile calls → Core `/api/v1/users/me`
- `apis/organizationAPI.ts` — CRUD → Core `/api/v1/organizations/*`
- `apis/invitationAPI.ts` — invitations → Core `/api/v1/invitations/*`
- `apis/pictogramAPI.ts` — pictograms → Core `/api/v1/pictograms*`
- `apis/gradeAPI.ts` — grades → Core `/api/v1/organizations/{orgId}/grades*`
- `apis/citizenAPI.ts` — citizens → Core `/api/v1/organizations/{orgId}/citizens*`
- `components/InvitationList.tsx` — adapted to Core response format
- `components/GradeItem.tsx` — adapted to Core response format
- `components/GradeSelector.tsx` — adapted to Core response format
- `app/(app)/viewOrganization/[id].tsx` — adapted to Core response format
- `app/(app)/viewCitizen/[id].tsx` — adapted to Core response format
- `app/(app)/settings/index.tsx` — adapted to Core response format

**Verification:** 115 Core tests + 131 Frontend tests all passing.

---

### Phase 5: Backend — Slim Down to Activities Only

**5A: Add GirafCoreClient for Activity validation**
- Create `GirafAPI/Clients/GirafCoreClient.cs` — HTTP client calling Core API
- Create `GirafAPI/Clients/DTOs/` — minimal DTOs (CitizenDto, GradeDto, PictogramDto)
- Update `ServiceExtensions.cs` — register `HttpClient` for Core
- Update `appsettings.json` — add `GirafCore:BaseUrl`

**5B: Make Activity FKs explicit and drop constraints**
- Update `Activity.cs` — add `int? CitizenId`, `int? GradeId`, `int? PictogramId`
- Update DTOs — include ID fields
- Update `GirafDbContext.cs` — remove FK relationships
- Update `ActivityMapping.cs` — use ID fields instead of navigation properties
- Migration: `dotnet ef migrations add RemoveDomainEntityForeignKeys`

**5C: Update Activity endpoints**
- Replace `dbContext.Citizens.FindAsync()` → `girafCoreClient.GetCitizenAsync()`
- Replace `dbContext.Grades.FindAsync()` → `girafCoreClient.GetGradeAsync()`
- Query activities by `CitizenId`/`GradeId` column directly

**5D: Remove unused entities and endpoints**
Delete entire directories:
- `GirafAPI/Entities/{Citizens,Organizations,Grades,Pictograms,Invitations,Users}/`
- `GirafAPI/Endpoints/{Citizen,Organization,Grade,Pictogram,Invitation,User,Login}Endpoints.cs`
- `GirafAPI/Mapping/{Citizen,Organization,Grade,Pictogram,Invitation,User}Mapping.cs`

Update:
- `Program.cs` — keep only `MapActivityEndpoints()`
- `GirafDbContext.cs` — change to plain `DbContext`, keep only `DbSet<Activity>`
- `GirafAPI.csproj` — remove `Microsoft.AspNetCore.Identity.EntityFrameworkCore`

Migration: `dotnet ef migrations add RemoveLegacyDomainTables`

---

### Phase 6: Documentation + CLAUDE.md
Update:
- `/home/nbhansen/dev/GIRAF/software/CLAUDE.md` — reflect new architecture
- `weekplannerbackend/.github/copilot-instructions.md` — slim backend conventions
- `weekplannerbackend/README.md` — setup instructions (must run Core first)

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

The weekplanner backend becomes ~5-6 files. Everything else is Core's responsibility.

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
