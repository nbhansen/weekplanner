# Testing Environment Setup Guide

This guide explains how the testing environment is set up for the Giraf Weekplanner API.

## Table of Contents
- [Overview](#overview)
- [GirafWebApplicationFactory](#girafwebapplicationfactory)
- [DbSeeder System](#dbseeder-system)
- [Available Seeders](#available-seeders)
- [TestJwtToken & Authentication](#testjwttoken--authentication)
- [Writing a New Integration Test](#writing-a-new-integration-test)

## Overview

Integration tests use:
- **`WebApplicationFactory<Program>`** to create a test server (no Docker/Postgres needed)
- **SQLite file-per-test** databases for full isolation
- **Seeders** to populate each test's database with the exact scenario it needs
- **Test JWT tokens** to authenticate as different user roles

Tests run without any external services — everything is self-contained.

## GirafWebApplicationFactory

The factory creates a test server with:
1. Environment set to `"Testing"` (skips auto-migration/seeding in `Program.cs`)
2. SQLite database with a unique filename per test (`GirafTestDb_{Guid}.db`)
3. JWT authentication configured with test credentials
4. Authorization policies re-registered for the test container
5. Automatic cleanup of DB files on dispose

```csharp
// Typical test setup pattern:
var factory = new GirafWebApplicationFactory();
var seeder = new BaseCaseDb();
var scope = factory.Services.CreateScope();
factory.SeedDb(scope, seeder);
var client = factory.CreateClient();

// Authenticate as a specific user
client.AttachClaimsToken(scope, seeder.Users["admin"]);
```

## DbSeeder System

`DbSeeder` is an abstract base class providing reusable seeding methods:

| Method | What it seeds |
|--------|--------------|
| `SeedUsers(userManager)` | Creates `owner`, `admin`, `member` users with claims |
| `SeedSingleUser(userManager)` | Creates a single `user` |
| `SeedOrganization(dbContext, userManager, owner, admins, members)` | Creates an org with role claims |
| `SeedCitizens(dbContext, org)` | Creates 3 test citizens |
| `SeedPictogram(dbContext, org)` | Creates a test pictogram |
| `SeedCitizenActivity(dbContext, citizenId, pictogram)` | Creates an activity for a citizen |
| `SeedGradeActivity(dbContext, gradeId, pictogram)` | Creates an activity for a grade |
| `SeedGrade(dbContext, org)` | Creates a grade in an org |
| `AddCitizensToGrade(dbContext, gradeId, citizens)` | Assigns citizens to a grade |
| `SeedInvitation(dbContext, orgId, senderId, receiverId)` | Creates an invitation |

After seeding, access created entities via the seeder's public properties:
- `seeder.Users["owner"]`, `seeder.Users["admin"]`, `seeder.Users["member"]`
- `seeder.Organizations`, `seeder.Citizens`, `seeder.Pictograms`, `seeder.Activities`, `seeder.Grades`, `seeder.Invitations`

## Available Seeders

| Seeder | Description |
|--------|-------------|
| `EmptyDb` | No data — tests "empty" scenarios |
| `OnlyUsersAndOrgDb` | Users + 1 organization (no citizens/activities) |
| `BaseCaseDb` | Full scenario: users, org, 3 citizens, pictogram, activities, grade |

To add a new seeder, create a class extending `DbSeeder` and override `SeedData(dbContext, userManager)`.

## TestJwtToken & Authentication

`TestJwtToken.Build(claims)` creates a JWT with:
- Issuer: `TestIssuer`
- Audience: `TestAudience`
- Key: `ThisIsASecretKeyForTestingPurposes!`
- Expires in 1 hour

Use `HttpClientExtensions.AttachClaimsToken(client, scope, user)` to authenticate an `HttpClient` as any seeded user. This reads the user's claims from the `UserManager` and builds a test JWT.

## Writing a New Integration Test

```csharp
[Fact]
public async Task MyEndpoint_ReturnsExpected()
{
    // 1. Create factory and seed
    var factory = new GirafWebApplicationFactory();
    var seeder = new BaseCaseDb();
    var scope = factory.Services.CreateScope();
    factory.SeedDb(scope, seeder);
    var client = factory.CreateClient();

    // 2. Authenticate as needed
    client.AttachClaimsToken(scope, seeder.Users["admin"]);

    // 3. Act
    var response = await client.GetAsync("/your/endpoint");

    // 4. Assert
    response.EnsureSuccessStatusCode();
    var result = await response.Content.ReadFromJsonAsync<YourDTO>();
    Assert.NotNull(result);
}
```

**Key points:**
- Each test gets its own factory/DB — no shared state between tests
- Use `EmptyDb` for "not found" / empty scenarios
- Use `OnlyUsersAndOrgDb` when you need auth but no citizens/activities
- Use `BaseCaseDb` for the full happy-path scenario
- Factory disposes DB files automatically, but if tests fail mid-run, leftover `GirafTestDb_*.db` files may accumulate — they are gitignored
