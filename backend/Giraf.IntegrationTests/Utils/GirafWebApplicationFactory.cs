using System.Text;
using Giraf.IntegrationTests.Utils.DbSeeders;
using GirafAPI.Authorization;
using GirafAPI.Clients;
using GirafAPI.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace Giraf.IntegrationTests.Utils;

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
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<GirafDbContext>));

            var dbFileName = $"GirafTestDb_{Guid.NewGuid()}.db";
            _dbFiles.Add(dbFileName);

            services.AddDbContext<GirafDbContext>(options =>
            {
                options.UseSqlite($"Data Source={dbFileName}");
            });

            // Configure JWT for testing
            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters.ValidateIssuer = false;
                options.TokenValidationParameters.ValidateAudience = false;
                options.TokenValidationParameters.IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ThisIsASecretKeyForTestingPurposes!"));
            });

            // Authorization
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

            if (_stubCoreClient)
            {
                services.RemoveAll<ICoreClient>();
                services.AddSingleton<ICoreClient, StubCoreClient>();
            }

            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();

            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
        });
    }

    public void SeedDb(IServiceScope scope, DbSeeder seeder)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
        seeder.SeedData(dbContext);
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
