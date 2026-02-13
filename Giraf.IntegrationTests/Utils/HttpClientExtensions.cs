using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Giraf.IntegrationTests.Utils;

public static class HttpClientExtensions
{
    public static void AttachClaimsToken(this HttpClient httpClient, IServiceScope scope, GirafUser user)
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>();
        var dbClaims = userManager.GetClaimsAsync(user).GetAwaiter().GetResult().ToList();

        // Build org_roles claim from old-style OrgMember/OrgAdmin/OrgOwner claims
        var orgRoles = new Dictionary<string, string>();
        foreach (var claim in dbClaims)
        {
            if (claim.Type == "OrgOwner")
                orgRoles[claim.Value] = "owner";
            else if (claim.Type == "OrgAdmin" && !orgRoles.ContainsKey(claim.Value))
                orgRoles[claim.Value] = "admin";
            else if (claim.Type == "OrgMember" && !orgRoles.ContainsKey(claim.Value))
                orgRoles[claim.Value] = "member";
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim("sub", user.Id),
            new Claim("org_roles", JsonSerializer.Serialize(orgRoles))
        };

        var token = new TestJwtToken();
        var tokenString = token.Build(claims);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenString);
    }
}