using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace Giraf.IntegrationTests.Utils;

public static class HttpClientExtensions
{
    /// <summary>
    /// Attaches a test JWT with org_roles claim for a given role.
    /// </summary>
    public static void AttachClaimsToken(this HttpClient httpClient, string userId = "test-user-id",
        string role = "member", int orgId = 1)
    {
        var orgRoles = new Dictionary<string, string>
        {
            [orgId.ToString()] = role
        };

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("sub", userId),
            new Claim("org_roles", JsonSerializer.Serialize(orgRoles))
        };

        var token = new TestJwtToken();
        var tokenString = token.Build(claims);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenString);
    }
}
