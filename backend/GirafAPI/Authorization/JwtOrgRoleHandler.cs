using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace GirafAPI.Authorization;

/// <summary>
/// Reads the org_roles claim from a Core-issued JWT and checks the user's role
/// for the organization specified in the route ({orgId}).
/// Role hierarchy: owner > admin > member.
/// </summary>
public class JwtOrgRoleHandler :
    AuthorizationHandler<OrgMemberRequirement>,
    IAuthorizationHandler
{
    private static readonly Dictionary<string, int> RoleLevels = new()
    {
        ["member"] = 0,
        ["admin"] = 1,
        ["owner"] = 2,
    };

    private readonly IHttpContextAccessor _httpContextAccessor;

    public JwtOrgRoleHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // OrgMember — any role suffices
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrgMemberRequirement requirement)
    {
        if (HasMinRole(context, "member"))
            context.Succeed(requirement);
        else
            context.Fail();

        return Task.CompletedTask;
    }

    // Called manually for OrgAdmin and OrgOwner via IAuthorizationHandler
    async Task IAuthorizationHandler.HandleAsync(AuthorizationHandlerContext context)
    {
        foreach (var req in context.PendingRequirements.ToList())
        {
            switch (req)
            {
                case OrgMemberRequirement member:
                    await HandleRequirementAsync(context, member);
                    break;
                case OrgAdminRequirement admin:
                    if (HasMinRole(context, "admin"))
                        context.Succeed(admin);
                    else
                        context.Fail();
                    break;
                case OrgOwnerRequirement owner:
                    if (HasMinRole(context, "owner"))
                        context.Succeed(owner);
                    else
                        context.Fail();
                    break;
                case OwnDataRequirement ownData:
                    HandleOwnData(context, ownData);
                    break;
            }
        }
    }

    private bool HasMinRole(AuthorizationHandlerContext context, string minRole)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return false;

        var orgIdInUrl = httpContext.Request.RouteValues["orgId"]?.ToString();
        if (string.IsNullOrEmpty(orgIdInUrl))
            return false;

        var orgRoles = GetOrgRoles(context.User);
        if (orgRoles == null || !orgRoles.TryGetValue(orgIdInUrl, out var userRole))
            return false;

        if (!RoleLevels.TryGetValue(userRole, out var userLevel) ||
            !RoleLevels.TryGetValue(minRole, out var requiredLevel))
            return false;

        return userLevel >= requiredLevel;
    }

    private void HandleOwnData(AuthorizationHandlerContext context, OwnDataRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            context.Fail();
            return;
        }

        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? context.User.FindFirst("sub")?.Value;
        var userIdInUrl = httpContext.Request.RouteValues["userId"]?.ToString();

        if (userId != null && userId == userIdInUrl)
            context.Succeed(requirement);
        else
            context.Fail();
    }

    private static Dictionary<string, string>? GetOrgRoles(ClaimsPrincipal user)
    {
        var orgRolesClaim = user.FindFirst("org_roles")?.Value;
        if (string.IsNullOrEmpty(orgRolesClaim))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(orgRolesClaim);
        }
        catch
        {
            return null;
        }
    }
}
