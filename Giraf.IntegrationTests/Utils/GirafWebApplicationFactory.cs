using System.Text;
using Giraf.IntegrationTests.Utils.DbSeeders;
using GirafAPI.Authorization;
using GirafAPI.Clients;
using GirafAPI.Data;
using GirafAPI.Configuration;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Giraf.IntegrationTests.Utils;

// This factory creates a Giraf web API configured for testing.
internal class GirafWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly List<string> _dbFiles = new();
    private readonly bool _stubCoreClient;

    public GirafWebApplicationFactory(bool stubCoreClient = false)
    {
        _stubCoreClient = stubCoreClient;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing"); // Set the environment to "Testing"
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<GirafDbContext>));

            // Use a unique SQLite database file for each test to avoid concurrency issues
            var dbFileName = $"GirafTestDb_{Guid.NewGuid()}.db";
            _dbFiles.Add(dbFileName);

            // Configure the DbContext for testing
            services.AddDbContext<GirafDbContext>(options =>
            {
                options.UseSqlite($"Data Source={dbFileName}");
            });

            // Configure JwtSettings for testing
            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters.ValidateIssuer = false;
                options.TokenValidationParameters.ValidateAudience = false;
                options.TokenValidationParameters.IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ThisIsASecretKeyForTestingPurposes!"));
            });

            // Add authorization — single JWT claim-based handler
            services.AddScoped<IAuthorizationHandler, JwtOrgRoleHandler>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("OrganizationMember", policy =>
                    policy.Requirements.Add(new OrgMemberRequirement()));
                options.AddPolicy("OrganizationAdmin", policy =>
                    policy.Requirements.Add(new OrgAdminRequirement()));
                options.AddPolicy("OrganizationOwner", policy =>
                    policy.Requirements.Add(new OrgOwnerRequirement()));
                options.AddPolicy("OwnData", policy =>
                    policy.Requirements.Add(new OwnDataRequirement()));
            });

            // Optionally replace ICoreClient with a stub
            if (_stubCoreClient)
            {
                services.RemoveAll<ICoreClient>();
                services.AddSingleton<ICoreClient, StubCoreClient>();
            }

            // Build the service provider and create a scope
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();

            // Clear the database before seeding
            dbContext.Database.EnsureDeleted();

            // Use migrations to apply schema, especially for identity tables
            dbContext.Database.Migrate();
        });
    }

    public void SeedDb(IServiceScope scope, DbSeeder seeder)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>();
        seeder.SeedData(dbContext, userManager);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            foreach (var dbFile in _dbFiles)
            {
                try { File.Delete(dbFile); } catch { /* best-effort cleanup */ }
            }
        }
    }
}
