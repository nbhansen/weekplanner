import { isTokenExpired, getUserIdFromToken, getOrgRolesFromToken } from "../utils/jwtDecode";

describe("isTokenExpired", () => {
  it("returns true for expired token", () => {
    const expiredToken = "header.expiredPayload.signature";
    expect(isTokenExpired(expiredToken)).toBe(true);
  });

  it("returns false for valid token", () => {
    const validToken = "header." + btoa(JSON.stringify({ exp: Date.now() / 1000 + 1000 })) + ".signature";
    expect(isTokenExpired(validToken)).toBe(false);
  });
});

describe("getUserIdFromToken", () => {
  it("extracts user ID from token sub claim", () => {
    const userId = "user-123";
    const payload = { sub: userId, exp: Date.now() / 1000 + 3600 };
    const token = "header." + btoa(JSON.stringify(payload)) + ".signature";

    expect(getUserIdFromToken(token)).toBe(userId);
  });

  it("handles malformed token gracefully", () => {
    const malformedToken = "not.a.valid.token";
    // Should not throw, returns empty string for invalid tokens
    const result = getUserIdFromToken(malformedToken);
    expect(result).toBe("");
  });
});

describe("getOrgRolesFromToken (Core Auth)", () => {
  it("extracts org_roles from Core JWT", () => {
    const orgRoles = { "1": "owner", "5": "member", "10": "admin" };
    const payload = {
      sub: "user-123",
      exp: Date.now() / 1000 + 3600,
      org_roles: orgRoles
    };
    const token = "header." + btoa(JSON.stringify(payload)) + ".signature";

    expect(getOrgRolesFromToken(token)).toEqual(orgRoles);
  });

  it("returns empty object when org_roles is missing", () => {
    const payload = { sub: "user-123", exp: Date.now() / 1000 + 3600 };
    const token = "header." + btoa(JSON.stringify(payload)) + ".signature";

    expect(getOrgRolesFromToken(token)).toEqual({});
  });

  it("returns empty object for malformed token", () => {
    const malformedToken = "not.a.valid.token";

    expect(getOrgRolesFromToken(malformedToken)).toEqual({});
  });

  it("handles empty org_roles object", () => {
    const payload = {
      sub: "user-123",
      exp: Date.now() / 1000 + 3600,
      org_roles: {}
    };
    const token = "header." + btoa(JSON.stringify(payload)) + ".signature";

    expect(getOrgRolesFromToken(token)).toEqual({});
  });

  it("preserves org_roles structure from Core", () => {
    const orgRoles = {
      "1": "owner",
      "2": "admin",
      "3": "member",
      "100": "member",
      "999": "admin"
    };
    const payload = {
      sub: "user-456",
      exp: Date.now() / 1000 + 3600,
      org_roles: orgRoles
    };
    const token = "header." + btoa(JSON.stringify(payload)) + ".signature";

    const result = getOrgRolesFromToken(token);
    expect(result).toEqual(orgRoles);
    expect(Object.keys(result)).toHaveLength(5);
    expect(result["1"]).toBe("owner");
    expect(result["100"]).toBe("member");
  });
});
