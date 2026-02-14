# weekplanner-api
This is the backend REST API for the weekplanner branch of the GIRAF project.
The weekplanner API uses Microsoft's .NET 8 architecture with a modern MinimalAPI
setup. It also includes a containerized PostgreSql database.

## Architecture

**Current State (Phase 2 & 3 complete):**
- Backend validates JWTs issued by **giraf-core** (not self-issued)
- Authorization reads `org_roles` from JWT claims (no DB queries during auth)
- Still stores full domain model locally (users, orgs, citizens, grades, pictograms, activities)
- **Future:** Backend will slim down to activities-only, Core will be source of truth for shared entities

**JWT Flow:**
```
Frontend → Core (auth) → JWT with org_roles claim
Frontend → Weekplanner Backend (validates Core JWT)
```

## Prerequisites

1. **giraf-core must be running first** — weekplanner validates Core-issued JWTs

2. Download and install .NET 8 from:

   https://dotnet.microsoft.com/en-us/download/dotnet

3. Install Microsoft Entity Framework:
    ```bash
    dotnet tool install --global dotnet-ef
    ```
4. Download and install Docker Desktop from:

   https://www.docker.com/products/docker-desktop/

## Get Started

### Option 1: Full Stack (Recommended)
1. Start giraf-core:
    ```bash
    cd ../../giraf-core
    docker compose up -d
    ```

2. Launch weekplanner backend:
    ```bash
    cd weekplanner/weekplannerbackend
    docker compose up
    ```

### Option 2: Development Mode (API only)
1. Ensure giraf-core is running (see above)

2. Start local PostgreSQL:
    ```bash
    docker compose up -d weekplanner-db
    ```

3. Run migrations:
    ```bash
    dotnet ef database update
    ```

4. Run the API:
    ```bash
    dotnet run --project GirafAPI
    ```
    API will be available at http://localhost:5171

## Production Environment
The production environment uses a different build process from 
the development environment. To run the production environment, call:
```bash
docker compose -f docker-compose.prod.yml up --build
```

## Staging Environment
The staging environment mirrors production but allows seed data configuration.
1. Copy `.env.example` to `.env` and fill in the required values.
2. Run:
   ```bash
   docker compose -f docker-compose.staging.yml up --build
   ```

## Configuration Notes

### JWT Settings (CRITICAL)
**The backend validates JWTs issued by giraf-core.** JWT settings must match Core's configuration:

| Setting | Value | Notes |
|---------|-------|-------|
| `JwtSettings__SecretKey` | Must match Core's `JWT_SECRET` | Min 32 chars, shared secret |
| `JwtSettings__Issuer` | `"GirafCore"` | Must match Core's issuer |
| `JwtSettings__Audience` | `"GirafApps"` | Must match Core's audience |

**Environment Variables:**
```bash
# Required - must match giraf-core
JwtSettings__SecretKey=<same-as-core-JWT_SECRET>
JwtSettings__Issuer=GirafCore
JwtSettings__Audience=GirafApps

# Database
ConnectionStrings__DbConnection=<postgres-connection-string>
```

**Development:** Values in `appsettings.json` are pre-configured to match Core's defaults.

**Production/Staging:** Override via environment variables. Ensure Core and all app backends share the same `JWT_SECRET`.

### Seed Data
Seed data is disabled outside development by default. You can enable it using:

- `SeedData__Enabled=true`
- `SeedData__CreateDefaultUser=true`
- `SeedData__AddDefaultPictograms=true`

## Update the Database
If you make changes to entities or DTOs, make sure to update the database:

1. Add a new migration
   ```
   dotnet ef migrations add Your_Migration_Name_Here --output-dir Data\Migrations
   ```
2. Update the database
   ```bash
   dotnet ef database update
   ```

## Authorization

**Claim-based authorization** using `org_roles` from Core JWT:
- Policies: `OrgMember`, `OrgAdmin`, `OrgOwner`
- Role hierarchy: `owner > admin > member`
- Handler: `JwtOrgRoleHandler` reads claims (zero DB queries)

**Security behavior:**
- Unauthorized access to non-existent resources returns `403 Forbidden` (not `404`)
- Prevents resource enumeration attacks

**Example JWT claim:**
```json
{
  "id": "user-id-123",
  "org_roles": {
    "1": "owner",
    "5": "member"
  }
}
```

## Testing

Run all tests:
```bash
dotnet test
```

Run specific test project:
```bash
dotnet test Giraf.IntegrationTests/
dotnet test Giraf.UnitTests/
```

**Integration tests:**
- Use `WebApplicationFactory` with in-memory SQLite
- Generate test JWTs with `org_roles` claim via `HttpClientExtensions.AttachClaimsToken()`

## Migration Status

**Completed:**
- ✅ Phase 2: Validate Core-issued JWTs
- ✅ Phase 3: Claim-based authorization (no DB queries)

**Planned:**
- Phase 1: Frontend authenticates with Core
- Phase 4: Frontend calls Core for shared entities
- Phase 5: Backend slims down to activities-only

See `docs/migration-plan.md` for full roadmap.
